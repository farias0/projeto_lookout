using Godot;
using Godot.Collections;
using projeto_lookout.libs;
using System;

public partial class Npc : Area3D
{
	public enum NpcState
	{
		Patrolling,
		ShootingBack,
		InDialogue
	}

	[Export]
	public int Health { get; set; } = 100;
	[Export]
	public float SpeedPatrolling { get; set; } = 2.8f;
	[Export]
	public float VisionAngle { get; set; } = 55;
	[Export]
	public float VisionDistance { get; set; } = 30;
	[Export]
	public float ArrowLoadTime { get; set; } = 2f;

	[Export]
	public Array<PatrolPoint> PatrolPoints { get; set; } = new Array<PatrolPoint>();


	private readonly float TurnSpeed = 20f;


	public NpcAudio NpcAudio;

	public bool SeesPlayer;
	public Vector3 LastKnownPlayerPos;
	public Vector3 TurnTarget;
	public NpcState State;
	public Dialogue ActiveDialogue;


	private MeshInstance3D _mesh;
	private NavigationAgent3D _navAgent;
	private Node3D _bow;
	private Node3D _meshNode;

	private float _tookDamageCountdown = -1;
	private int _patrolIndex = 0;
	private float _speed;
	private float _arrowLoadCountdown = -1;
	private Node3D _arrow;
	private float _navMeshStuckCountdown = -1;
	private BowAudio _bowAudio;
	private Vector3 _shootBackPos;


	/// <summary>
	/// Initiates an interaction with another entity
	/// </summary>
	/// <param name="entity">The entity that initiated the interaction</param>
	public virtual void InteractWith(Node3D entity)
	{
		if (State == NpcState.InDialogue)
		{
			ActiveDialogue.NextLine();
		}
	}

	public override void _Ready()
	{
		_meshNode = GetNode<Node3D>("MeshNode");
		_mesh = _meshNode.GetNode<MeshInstance3D>("Mesh");
		_navAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");
		_bow = _meshNode.GetNode<Node3D>("Bow");
		_bowAudio = _bow.GetNode<BowAudio>("AudioStreamPlayer3D");
		NpcAudio = GetNode<NpcAudio>("AudioStreamPlayer3D");

		Monitoring = true;
		Connect("body_entered", new Callable(this, nameof(OnBodyEntered)));

		SetBowVisibility(false);

		StartPatrolling();
	}

	public override void _Process(double delta)
	{
		ProcessDamageCountdown((float)delta);

		Vector3? seesPlayer = CanSeePlayer();
		SeesPlayer = seesPlayer != null;
		if (SeesPlayer) LastKnownPlayerPos = seesPlayer.Value;

		switch (State)
		{
			case NpcState.Patrolling:
				ContinuePatrolling();
				break;
			case NpcState.ShootingBack:
				ContinueShootingBack((float)delta);
				break;
			case NpcState.InDialogue:
				ContinueInDialogue((float)delta);
				break;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		TurnTowardsTarget((float)delta);
		MoveTowardsTarget((float)delta);
		SnapToFloor();
	}

	public void TakeDamage(Vector3 origin, int damage)
	{
		if (_tookDamageCountdown > 0)
			return;

		if (Health <= 0)
			throw new InvalidOperationException($"Dead player {Name} took damage.");

		if (State == NpcState.InDialogue)
		{
			FinishDialogue();
		}

		Health -= damage;
		_tookDamageCountdown = 2;
		StartShootingBack(origin);

		Debug.Log($"{Name} took damage. Health: {Health}");

		if (Health <= 0)
		{
			Die();
		}

		NpcAudio.PlayGotHit();
	}

	public void SetBowVisibility(bool visible)
	{
		_bow.Visible = visible;
	}

	public void ToggleBowVisibility()
	{
		_bow.Visible = !_bow.Visible;
	}

	private void OnBodyEntered(Node body)
	{
		if (body.GetParent() is Arrow arrow)
		{
			arrow.OnBodyEntered(this);
		}
	}

	private void ProcessDamageCountdown(float delta)
	{
		if (_tookDamageCountdown <= 0) return;

		_tookDamageCountdown -= delta;

		// Blink effect
		_meshNode.Visible = _tookDamageCountdown % 0.2f > 0.1f;

		if (_tookDamageCountdown <= 0)
		{
			_tookDamageCountdown = -1;

			// Stop blinking
			_meshNode.Visible = true;
		}
	}

	private void Die()
	{
		Debug.Log($"NPC {Name} died.");
		QueueFree();
	}

	private float GetHeight()
	{
		CollisionShape3D collisionShape = GetNode<CollisionShape3D>("CollisionShape3D");
		CylinderShape3D cylinder = collisionShape.Shape as CylinderShape3D;
		return cylinder.Height;
	}

	private void SetTarget(Vector3 pos)
	{
		_navAgent.TargetPosition = pos;
	}

	private void MoveTowardsTarget(float delta)
	{
		Vector3 pos = GlobalPosition;
		Vector3 targetPos = _navAgent.GetNextPathPosition();

		if (pos.DistanceTo(targetPos) < 1.0f)
		{
			return;
		}

		TurnTarget = targetPos;

		pos.Y = 0;
		targetPos.Y = 0;
		GlobalPosition += (targetPos - pos).Normalized() * _speed * delta;
	}

	private void TurnTowardsTarget(float delta)
	{
		Vector3 forwardLocalAxis = new(0, 0, -1);
		Vector3 forwardDir = (GlobalTransform.Basis * forwardLocalAxis).Normalized();
		Vector3 targetDir = (TurnTarget - GlobalPosition).Normalized();

		if (Math.Abs(forwardDir.Dot(targetDir)) > 1e-4)
		{
			var rot = Rotation;
			rot.Y += (forwardDir.Cross(targetDir) * TurnSpeed * delta).Y;
			Rotation = rot;
		}
	}

	private void StopInPlace()
	{
		SetTarget(GlobalPosition);
	}

	/// <summary>
	/// Snaps the NPC to the floor, allowing them to use slopes and generally sticking to the ground.
	/// </summary>
	private void SnapToFloor()
	{
		float snapY = GlobalPosition.Y;

		var ray1Offset = GetHeight() * 0.5f;
		var ray1Origin = GlobalPosition + new Vector3(0, ray1Offset, 0);
		var ray1Result = Raycast.CastRayInDirection(GetWorld3D(), ray1Origin, new(0, -1, 0), ray1Offset);
		if (ray1Result.ContainsKey("collider") && (Node)ray1Result["collider"] != this)
		{
			// Snap up
			snapY = ((Vector3)ray1Result["position"]).Y;
		}
		else
		{
			var ray2result = Raycast.CastRayInDirection(GetWorld3D(), GlobalPosition, new(0, -1, 0), 3);
			if (ray2result.ContainsKey("collider"))
			{
				// Snap down
				snapY = ((Vector3)ray2result["position"]).Y;
			}
		}

		var pos = GlobalPosition;
		pos.Y = snapY;
		GlobalPosition = pos;
	}

	public void PullArrowBack()
	{
		if (_arrow != null)
		{
			throw new InvalidOperationException("NPC can't pull arrow back; already has one.");
		}

		Node node = Resources.Instance.Arrow.Instantiate();
		Node3D node3d = node as Node3D;

		Vector3 spawnPos = node3d!.Basis.X.Normalized() * 0.1f +
								node3d!.Basis.Z.Normalized() * -1.0f +
								node3d!.Basis.Y.Normalized() * 1.8f;
		node3d!.GlobalPosition = spawnPos;


		Arrow arrow = node3d as Arrow;
		arrow.SetType(ArrowType.Normal);


		AddChild(node3d);
		_arrow = node3d;

		_bowAudio.PlayTensing();
	}

	public void FireArrowAt(Vector3 target)
	{
		if (_arrow == null) return;

		Vector3 pos = _arrow.GlobalPosition;
		RemoveChild(_arrow);
		GetParent().AddChild(_arrow);
		_arrow.GlobalPosition = pos;

		// ATENTION: It compensates for the *player*'s height. If the target end up not being the player, wel...
		var player = Resources.Instance.Player;
		var targetCompensated = target + (player.GlobalTransform.Basis.Y * player.GetHeight() * 0.5f);
		_arrow.LookAt(targetCompensated);

		{
			Arrow a = _arrow as Arrow;
			a!.Fire();
		}

		_arrow = null;

		_bowAudio.PlayFired();
	}

	private void CancelArrow()
	{
		SetBowVisibility(false);

		if (_arrow == null) return;

		_arrow.QueueFree();
		_arrow = null;
	}

	/// <returns>The player's position, if the NPC can see him.</returns>
	private Vector3? CanSeePlayer()
	{
		var player = Resources.Instance.Player;

		{   // Is within distance
			float distance = GlobalPosition.DistanceTo(player.GlobalPosition);
			if (distance > VisionDistance)
			{
				return null;
			}
		}

		{   // Is within the vision cone
			Vector3 direction = (player.GlobalPosition - GlobalPosition).Normalized();
			float angle = GlobalTransform.Basis.Z.AngleTo(direction * -1);
			if (angle > Mathf.DegToRad(VisionAngle))
			{
				return null;
			}
		}

		{ // Has no obstacles between them
			Vector3 heightOffset = new(0, 1.3f, 0);
			Vector3 origin = GlobalPosition + heightOffset;
			Vector3 target = player.GlobalPosition + heightOffset;

			var rayResult = Raycast.CastRay(GetWorld3D(), origin, target);

			if (rayResult.ContainsKey("collider") && (Node)rayResult["collider"] != player)
			{
				return null;
			}
		}

		return player.GlobalPosition;
	}

	/*
	 *		AI STUFF
	 */

	private void StartPatrolling()
	{
		SetTarget(PatrolPoints[_patrolIndex].Pos);
		_speed = SpeedPatrolling;
		State = NpcState.Patrolling;
		Debug.Log($"{Name} started patrolling.");
	}

	private void ContinuePatrolling()
	{
		var target = PatrolPoints[_patrolIndex].Pos;
		if (GlobalPosition.DistanceTo(target) < 1.5f)
		{
			_patrolIndex = (_patrolIndex + 1) % PatrolPoints.Count;
			SetTarget(PatrolPoints[_patrolIndex].Pos);
		}
	}

	private void StartShootingBack(Vector3 origin)
	{
		StopInPlace();
		SetBowVisibility(true);
		State = NpcState.ShootingBack;
		PullArrowBack();
		_arrowLoadCountdown = ArrowLoadTime;
		_shootBackPos = origin;
		TurnTarget = _shootBackPos;
		Debug.Log($"{Name} started shooting back.");
	}

	private void ContinueShootingBack(float delta)
	{
		if (_arrowLoadCountdown > 0)
		{
			_arrowLoadCountdown -= delta;
			if (_arrowLoadCountdown <= 0)
			{
				FireArrowAt(_shootBackPos);
			}
		}
		else
		{
			SetBowVisibility(false);
			StartPatrolling();
		}
	}

	public void StartDialogue(Dialogue dialogue)
	{
		StopInPlace();
		CancelArrow();
		TurnTarget = LastKnownPlayerPos;
		State = NpcState.InDialogue;
		ActiveDialogue = dialogue;
		ActiveDialogue.Start();
	}

	private void ContinueInDialogue(float delta)
	{
		// TODO finish dialogue if the player walks away

		TurnTarget = LastKnownPlayerPos;
		ActiveDialogue.Process(delta);

		if (ActiveDialogue.IsFinished)
		{
			FinishDialogue();
		}
	}
	
	private void FinishDialogue()
	{
		ActiveDialogue.Stop();
		ActiveDialogue = null;
		StartPatrolling();
	}
}
