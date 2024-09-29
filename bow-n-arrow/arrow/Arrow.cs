using Godot;
using System;
using projeto_lookout.libs;

public enum ArrowType
{
	Normal,
	Hook
}

public partial class Arrow : Node3D
{
	public enum State
	{
		PulledBack,
		Flying,
		Hit,
		Hooked,
	}

	[Export]
	public float Speed { get; set; } = 45;
	[Export]
	public int Damage { get; set; } = 40;

	private const float LifeTime = 5;
	private readonly PackedScene RB_Normal = GD.Load<PackedScene>("res://bow-n-arrow/arrow/types/normal.tscn");
	private readonly PackedScene RB_Hook = GD.Load<PackedScene>("res://bow-n-arrow/arrow/types/hook.tscn");

	private float _lifeTime = LifeTime;
	private RigidBody3D _rigidBody;
	private State _state = State.PulledBack;
	private ArrowType _type = ArrowType.Normal;
	private Player _shooter = null;
	private Node3D _hookLine;
	private PickUp _hookedPickup = null;
	private ArrowAudio _audio;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_rigidBody = GetChild<RigidBody3D>(0);
		_rigidBody.ContactMonitor = true; // Necessary for detecting collision from the RigidBody3D
		_rigidBody.MaxContactsReported = 1;
		_rigidBody.Connect("body_entered", new Callable(this, nameof(OnBodyEntered)));
		_rigidBody.Freeze = true;

		_audio = GetNode<ArrowAudio>("AudioStreamPlayer3D");

		SetType(_type);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (_state == State.Flying || _state == State.Hit)
		{
			_lifeTime -= (float)delta;
			if (_lifeTime <= 0)
			{
				QueueFree();
				return;
			}
		}
		else if (_state == State.Hooked)
		{
			if (_shooter == null)
			{
				QueueFree();
				return;
			}

			DrawHookLine();
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_state == State.Flying)
		{
			MoveArrow((float)delta);
		}
	}

	public override void _Notification(int notification)
	{
		if (notification == NotificationPredelete)
		{
			ClearForDestruction();
		}
	}

	new public ArrowType GetType()
	{
		return _type;
	}

	public void SetType(ArrowType type)
	{
		_type = type;

		if (_rigidBody == null) return;

		switch (_type)
		{
			case ArrowType.Normal:
				_rigidBody.GetNode<CollisionShape3D>("Normal_CollisionShape3D").Disabled = false;
				_rigidBody.GetNode<Node3D>("Normal_MeshNode").Visible = true;
				_rigidBody.GetNode<CollisionShape3D>("Hook_CollisionShape3D").Disabled = true;
				_rigidBody.GetNode<Node3D>("Hook_MeshNode").Visible = false;
				break;
			case ArrowType.Hook:
				_rigidBody.GetNode<CollisionShape3D>("Normal_CollisionShape3D").Disabled = true;
				_rigidBody.GetNode<Node3D>("Normal_MeshNode").Visible = false;
				_rigidBody.GetNode<CollisionShape3D>("Hook_CollisionShape3D").Disabled = false;
				_rigidBody.GetNode<Node3D>("Hook_MeshNode").Visible = true;
				break;
		}
	}

	public void SetShooter(Player shooter)
	{
		_shooter = shooter;
	}

	public void Fire()
	{
		if (_state != State.PulledBack)
		{
			throw new InvalidOperationException("Arrow must be in PulledBack state to fire.");
		}

		_state = State.Flying;
		_rigidBody.Freeze = false;

		_audio.PlayFlying();
	}

	/// <summary>
	/// Only applies to Hook arrows that are hooked to something
	/// </summary>
	/// <returns>If the player is being pulled towards something, instead of pulling something towards them</returns>
	public bool HookIsShooterPulled()
	{
		return _hookedPickup == null;
	}

	/// <summary>
	/// Only applies to Hook arrows that are hooked to something
	/// </summary>
	/// <returns>The direction in which the hook is pulling</returns>
	public Vector3 HookGetPullDirection()
	{
		if (_shooter == null)
			throw new InvalidOperationException("Can't calculate pull direction without a shooter hooked.");

		if (HookIsShooterPulled())
		{
			return (GlobalPosition - _shooter.GlobalPosition).Normalized();
		}
		else
		{
			return (_shooter.GlobalPosition - GlobalPosition).Normalized();
		}
	}

	private void MoveArrow(float delta)
	{
		Position -= Transform.Basis.Z * Speed * delta;
	}

	public void OnBodyEntered(Node body)
	{
		if (_state != State.Flying) return;

		// Collision with selves
		if (body is Player && _shooter != null) return;
		else if (body is Enemy && _shooter == null) return;
		else if (body is Npc && _shooter == null) return;
		// TODO if everyone that shoots arrow registered as a shooter,
		// we could allow NPCs and enemies to shoot each other by fixing these checks


		_lifeTime = LifeTime;
		_rigidBody.Freeze = true;
		CallDeferred("reparent", body);


		if (_type == ArrowType.Normal)
		{
			_state = State.Hit;
			if (body is Enemy enemy)
			{
				enemy.TakeDamage(_shooter.GlobalPosition, Damage);
			}
			else if (body is Npc npc)
			{
				npc.TakeDamage(_shooter.GlobalPosition, Damage);
			}
			else if (body is Player player)
			{
				player.TakeDamage(Damage);
			}
		}
		else if (_type == ArrowType.Hook)
		{
			HookTo((Node3D)body);
		}

		_audio.PlayHit();
	}

	private void HookTo(Node3D body)
	{
		if (_type == ArrowType.Normal)
				throw new InvalidOperationException("Only hook arrows can hook to something.");


		_state = State.Hooked;

		// For now, only the player uses hook arrows
		_shooter.ArrowHooked(this);

		if (body is PickUp pickup)
		{
			_hookedPickup = pickup;
			_hookedPickup.ArrowHooked(this);
		}
	}

	private void DrawHookLine()
	{
		_hookLine?.QueueFree();

		var _shooterPos = _shooter.GlobalPosition + (_shooter.Basis.Y * _shooter.GetHeight() * 0.5f);
		_hookLine = Draw.Line3D(_shooter.GetParent(), GlobalPosition, _shooterPos, new(0,0,0));
	}

	public void ClearForDestruction()
	{
		if (_type == ArrowType.Hook && _state == State.Hooked)
		{
			_shooter?.ArrowHooked(null);
			_hookedPickup?.ArrowHooked(null);
		}

		_hookLine?.QueueFree();
	}
}
