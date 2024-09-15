using Godot;
using projeto_lookout.libs;
using System;
using Godot.Collections;

public partial class Enemy : Area3D
{
	private enum State
	{
		Patrolling,
		Alert,
		Searching,
		Chasing
	}

	public enum EnemyType
	{
		Melee,
		Ranged

	}


	[Export]
	public EnemyType Type
	{
		get => _type;
		set => ChangeType(value);
	}
	[Export]
	public int Health { get; set; } = 100;
	[Export]
	public int MeleeDamage { get; set; } = 30;

	[ExportGroup("Speed")]
	[Export]
	public float SpeedPatrolling { get; set; } = 2.8f;
	[Export]
	public float SpeedSearching { get; set; } = 4.6f;
	[Export]
	public float SpeedChasing { get; set; } = 7f;

	[ExportGroup("Vision")]
	[Export]
	public float VisionAngle { get; set; } = 55;
	[Export]
	public float VisionDistance { get; set; } = 30;
	[Export]
	public float VisionConfirmDistance { get; set; } = 18;

	[ExportGroup("Timers")]
	[Export]
	public float AlertCountdown { get; set; } = 5; // How long it takes to go back to patrolling after losing sight of the player
	[Export]
	public float AlertGaugeTime { get; set; } = 2; // For how long it has to see the player while in 'alert' to go investigate
	[Export]
	public float SearchGiveUpCountdown { get; set; } = 5; // How long it takes to give up searching for the player after losing sight of him

	[ExportGroup("Shooting")]
	[Export]
	public float ShootingDistance { get; set; } = 18; // Distance from which the enemy shoots
	[Export]
	public float ShootingLoadTime { get; set; } = 2f; // How long with the player on sight it takes for the enemy to take a shot
	[Export]
	public float ArrowSpeed { get; set; } = 90;
	[Export]
	public int ArrowDamage { get; set; } = 30;


	[ExportGroup("")]
	[Export]
	public Array<PatrolPoint> PatrolPoints { get; set; } = new Array<PatrolPoint>();


	private readonly float TurnSpeed = 20f;

	private CharacterBody3D _player;
	private MeshInstance3D _mesh;
	private NavigationAgent3D _navAgent;
	private Node3D _bow;

	private float _tookDamageCountdown = -1;
	private Vector3 _lastSeenPlayerPos;
	private int _patrolIndex = 0;
	private State _state;
	private float _speed;
	private bool _seesPlayer;
	private float _alertCountdown = -1;
	private float _alertGauge = 0;
	private Vector3 _turnTarget;
	private float _searchGiveUpCountdown;
	private EnemyType _type;
	private float _shootingLoadGauge = -1;
	private Node3D _arrow;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_player = Resources.Player;
		if (_player == null)
		{
			throw new InvalidOperationException("Couldn't find player.");
		}

		_mesh = FindChild("MeshNode").GetChild<MeshInstance3D>(0);
		if (_mesh == null)
		{
			throw new InvalidOperationException("Couldn't find enemy's mesh.");
		}

		_navAgent = FindChild("NavigationAgent3D") as NavigationAgent3D;
		if (_navAgent == null)
		{
			throw new InvalidOperationException("Couldn't find enemy's navigation agent.");
		}

		_bow = FindChild("Bow") as Node3D;
		if (_bow == null)
		{
			throw new InvalidOperationException("Couldn't find enemy's bow.");
		}


		Monitoring = true;
		Connect("body_entered", new Callable(this, nameof(OnBodyEntered)));

		SyncEnemyType();
		StartPatrolling();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		ProcessDamageCountdown((float)delta);

		if (_state != State.Chasing && _arrow != null)
		{
			DestroyArrow();
		}


		Vector3? seesPlayer = SeesPlayer();
		_seesPlayer = seesPlayer != null;
		if (_seesPlayer) _lastSeenPlayerPos = seesPlayer.Value;


		switch (_state)
		{
			case State.Patrolling:
				KeepPatrolling();
				break;
			case State.Alert:
				KeepAlert((float)delta);
				break;
			case State.Searching:
				KeepSearching((float)delta);
				break;
			case State.Chasing:
				KeepChasing((float)delta);
				break;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		TurnTowardsTarget((float)delta);
		MoveTowardsTarget((float)delta);
	}

	public void TakeDamage(Vector3 origin, int damage)
	{
		if (_tookDamageCountdown > 0)
		{
			return;
		}

		if (Health <= 0)
		{
			throw new InvalidOperationException($"Dead enemy {Name} took damage.");
		}

		Health -= damage;
		_tookDamageCountdown = 2;
		_lastSeenPlayerPos = origin;
		StartAlert();
		Debug.Log($"{Name} took damage. Health: {Health}");

		if (Health <= 0)
		{
			Die();
		}
	}
	public void ChangeType(EnemyType type)
	{
		_type = type;
		SyncEnemyType();
	}

	private void ChangeMeshMaterial(StandardMaterial3D material)
	{
		if (_mesh == null) return;
		_mesh.MaterialOverride = material;
	}

	/// <summary>
	/// Modify the enemy's properties based on its type.
	/// </summary>
	private void SyncEnemyType()
	{
		if (_bow != null)
			_bow.Visible = _type == EnemyType.Ranged;

		switch (_type)
		{
			case EnemyType.Melee:
				ChangeMeshMaterial(Resources.EnemyMeleeMaterial);
				break;
			case EnemyType.Ranged:
				ChangeMeshMaterial(Resources.EnemyRangedMaterial);
				break;
		}
	}

	private void OnBodyEntered(Node body)
	{
		if (body is Player player)
		{
			player.TakeDamage(MeleeDamage);
		}
	}

	private void ProcessDamageCountdown(float delta)
	{
		if (_tookDamageCountdown <= 0) return;

		_tookDamageCountdown -= delta;

		// Blink effect
		_mesh.Visible = _tookDamageCountdown % 0.2f > 0.1f;

		if (_tookDamageCountdown <= 0)
		{
			_tookDamageCountdown = -1;

			// Stop blinking
			_mesh.Visible = true;
		}
	}

	private void Die()
	{
		QueueFree();
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

		_turnTarget = targetPos;

		pos.Y = 0;
		targetPos.Y = 0;
		GlobalPosition += (targetPos - pos).Normalized() * _speed * delta;
	}

	private void TurnTowardsTarget(float delta)
	{
		Vector3 forwardLocalAxis = new(0, 0, -1);
		Vector3 forwardDir = (GlobalTransform.Basis * forwardLocalAxis).Normalized();
		Vector3 targetDir = (_turnTarget - GlobalPosition).Normalized();

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

	/// <returns>The player's position, if the enemy can see him.</returns>
	private Vector3? SeesPlayer()
	{
		{   // Is within distance
			float distance = GlobalPosition.DistanceTo(_player.GlobalPosition);
			if (distance > VisionDistance)
			{
				return null;
			}
		}

		{   // Is within the vision cone
			Vector3 direction = (_player.GlobalPosition - GlobalPosition).Normalized();
			float angle = GlobalTransform.Basis.Z.AngleTo(direction * -1);
			if (angle > Mathf.DegToRad(VisionAngle))
			{
				return null;
			}
		}

		{ // Has no obstacles between them
			Vector3 heightOffset = new(0, 1.3f, 0);
			Vector3 origin = GlobalPosition + heightOffset;
			Vector3 target = _player.GlobalPosition + heightOffset;

			var rayParams = new PhysicsRayQueryParameters3D
			{
				From = origin,
				To = target,
				CollideWithBodies = true,
				CollideWithAreas = true
			};
			var rayResult = GetWorld3D().DirectSpaceState.IntersectRay(rayParams);

			if (rayResult.ContainsKey("collider") && (Node)rayResult["collider"] != _player)
			{
				return null;
			}
		}

		return _player.GlobalPosition;
	}

	private void PullArrowBack()
	{
		if (_arrow != null)
		{
			throw new InvalidOperationException("Enemy can't pull arrow back; already has one.");
		}

		Node node = Resources.Arrow.Instantiate();
		Node3D node3d = node as Node3D;

		Vector3 spawnPos = node3d!.Basis.X.Normalized() * 0.1f +
								node3d!.Basis.Z.Normalized() * -1.0f +
								node3d!.Basis.Y.Normalized() * 1.8f;
		node3d!.GlobalPosition = spawnPos;


		Arrow arrow = node3d as Arrow;
		arrow.SetType(ArrowType.Normal);
		arrow!.Speed = ArrowSpeed;
		arrow!.Damage = ArrowDamage;


		AddChild(node3d);
		_arrow = node3d;
	}

	private void FireArrowAt(Vector3 target)
	{
		if (_arrow == null) return;

		Vector3 pos = _arrow.GlobalPosition;
		RemoveChild(_arrow);
		GetParent().AddChild(_arrow);
		_arrow.GlobalPosition = pos;

		_arrow.LookAt(CompensateForPlayersHeight(target));

		{
			Arrow a = _arrow as Arrow;
			a!.Fire();
		}

		_arrow = null;
	}

	private Vector3 CompensateForPlayersHeight(Vector3 playerPos)
	{
		playerPos += _player.GlobalTransform.Basis.Y *
						(_player as Player).GetHeight() *
							0.5f;
		return playerPos;
	}

	private void DestroyArrow()
	{
		if (_arrow != null)
		{
			(_arrow as Arrow).QueueFree();
		}
		_arrow = null;
	}


	/*
	 *      AI STUFF 
	 */

	private void StartPatrolling()
	{
		SetTarget(PatrolPoints[_patrolIndex].Pos);
		_speed = SpeedPatrolling;
		_state = State.Patrolling;
		Debug.Log($"{Name} started patrolling.");
	}

	private void KeepPatrolling()
	{
		var target = PatrolPoints[_patrolIndex].Pos;
		if (GlobalPosition.DistanceTo(target) < 1.5f)
		{
			_patrolIndex = (_patrolIndex + 1) % PatrolPoints.Count;
			SetTarget(PatrolPoints[_patrolIndex].Pos);
		}

		if (_seesPlayer)
		{
			var dist = GlobalPosition.DistanceTo(_player.GlobalPosition);
			if (dist <= VisionConfirmDistance)
			{
				StartChasing();
			}
			else
			{
				StartAlert();
			}
		}
	}

	private void StartAlert()
	{
		_state = State.Alert;
		_speed = 0;
		_alertCountdown = AlertCountdown;
		_alertGauge = 0;
		Debug.Log($"{Name} started alert.");
	}

	private void KeepAlert(float delta)
	{
		StopInPlace();
		_turnTarget = _lastSeenPlayerPos;

		if (_seesPlayer)
		{
			var dist = GlobalPosition.DistanceTo(_player.GlobalPosition);
			if (dist <= VisionConfirmDistance)
			{
				StartChasing();
			}
			else
			{
				_alertCountdown = AlertCountdown;
				_alertGauge += delta;
				if (_alertGauge >= AlertGaugeTime)
				{
					StartSearching();
				}
			}
		}
		else
		{
			_alertCountdown -= delta;
			_alertGauge = 0;
			if (_alertCountdown <= 0)
			{
				StartPatrolling();
			}
		}
	}

	private void StartSearching()
	{
		_state = State.Searching;
		_speed = SpeedSearching;
		_searchGiveUpCountdown = SearchGiveUpCountdown;
		Debug.Log($"{Name} started searching.");
	}

	private void KeepSearching(float delta)
	{
		SetTarget(_lastSeenPlayerPos);

		if (_seesPlayer)
		{
			_searchGiveUpCountdown = SearchGiveUpCountdown;

			if (GlobalPosition.DistanceTo(_player.GlobalPosition) <= VisionConfirmDistance)
			{
				StartChasing();
				return;
			}
		}
		else
		{
			if (GlobalPosition.DistanceTo(_lastSeenPlayerPos) < 1.5f)
			{
				_searchGiveUpCountdown -= delta;

				if (_searchGiveUpCountdown <= 0)
				{
					StartPatrolling();
					return;
				}
			}
		}
	}

	private void StartChasing()
	{
		_state = State.Chasing;
		_speed = SpeedChasing;
		_shootingLoadGauge = -1;
		Debug.Log($"{Name} started chasing.");
	}

	private void KeepChasing(float delta)
	{
		if (_shootingLoadGauge >= 0)
		{
			_shootingLoadGauge += delta;
			if (!_seesPlayer)
			{
				_shootingLoadGauge = -1;
				DestroyArrow();
			}
		}


		if ((_player as Player).IsInvincible())
		{
			_turnTarget = _lastSeenPlayerPos;
		}
		else
		{
			// Attack player
			if (_type == EnemyType.Melee)
			{
				SetTarget(_lastSeenPlayerPos);
			}
			else if (_type == EnemyType.Ranged)
			{
				var dist = GlobalPosition.DistanceTo(_lastSeenPlayerPos);
				if (dist <= ShootingDistance)
				{
					StopInPlace();
					_turnTarget = _lastSeenPlayerPos;

					if (_shootingLoadGauge == -1 && _seesPlayer)
					{
						_shootingLoadGauge = 0;
						PullArrowBack();
					}
					else if (_shootingLoadGauge >= ShootingLoadTime)
					{
						_shootingLoadGauge = -1;
						FireArrowAt(_lastSeenPlayerPos);
						_arrow = null;
					}
				}
				else SetTarget(_lastSeenPlayerPos);
			}
		}

		if (GlobalPosition.DistanceTo(_lastSeenPlayerPos) < 1.5f && !_seesPlayer)
		{
			StartAlert();
		}
	}
}
