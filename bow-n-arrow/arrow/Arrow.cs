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

	private float _lifeTime = LifeTime;
	private RigidBody3D _rigidBody;
	private State _state = State.PulledBack;
	private ArrowType _type = ArrowType.Normal;
	private Player _shooter = null;
	private Node3D _hookLine;
	private Color _hookColor;
	private PickUp _hookedPickup = null;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_rigidBody = GetChild<RigidBody3D>(0);
		_rigidBody.ContactMonitor = true; // Necessary for detecting collision from the RigidBody3D
		_rigidBody.MaxContactsReported = 1;
		_rigidBody.Connect("body_entered", new Callable(this, nameof(OnBodyEntered)));
		_rigidBody.Freeze = true;

		_hookColor = Resources.ArrowHookMaterial.AlbedoColor;

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
				Destroy();
				return;
			}
		}
		else if (_state == State.Hooked)
		{
			if (_shooter == null)
			{
				Destroy();
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

	private void ChangeMeshMaterial(StandardMaterial3D material)
	{
		_rigidBody.GetNode<MeshInstance3D>("MeshNode/Arrow").MaterialOverride = material;
	}

	public void SetType(ArrowType type)
	{
		_type = type;

		if (_rigidBody == null) return;

		switch (_type)
		{
			case ArrowType.Normal:
				ChangeMeshMaterial(Resources.ArrowNormalMaterial);
				break;
			case ArrowType.Hook:
				ChangeMeshMaterial(Resources.ArrowHookMaterial);
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
	}

	/// <summary>
	/// It was hooked, but who fired it doesn't want it anymore.
	/// </summary>
	public void DetachShooter()
	{
		if (_shooter == null)
			throw new InvalidOperationException("Can't detach shooter if there isn't one.");

		_shooter = null;
		Destroy();
	}

	/// <summary>
	/// It was hooked to a pickup, but the pickup deatached from it.
	/// </summary>
	public void DetachPickup()
	{
		_hookedPickup = null;
		Destroy();
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

	private void OnBodyEntered(Node body)
	{
		if (_state != State.Flying) return;

		// Collision with selves
		if (body is Player && _shooter != null) return;
		else if (body is Enemy && _shooter == null) return;


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
			else if (body is Player player)
			{
				player.TakeDamage(Damage);
			}
		}
		else if (_type == ArrowType.Hook)
		{
			HookTo((Node3D)body);
		}
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
		_hookLine = Draw.Line3D(_shooter.GetParent(), GlobalPosition, _shooterPos, _hookColor);
	}

	public void Destroy()
	{
		if (_type == ArrowType.Hook)
		{
			_shooter?.ArrowHooked(null);
			_hookedPickup?.ArrowHooked(null);
		}

		_hookLine?.QueueFree();
		QueueFree();
	}
}
