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
	private Player _player = null;
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

	public void SetPlayer(Player player)
	{
		_player = player;
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
	/// Only applies to Hook arrows that are hooked to something
	/// </summary>
	/// <returns>If the player is being pulled towards something, instead of pulling something towards them</returns>
	public bool HookIsPlayerPulled()
	{
		return _hookedPickup == null;
	}

	private void MoveArrow(float delta)
	{
		Position -= Transform.Basis.Z * Speed * delta;
	}

	private void OnBodyEntered(Node body)
	{
		if (_state != State.Flying) return;

		// Collision with selves
		if (body is Player && _player != null) return;
		else if (body is Enemy && _player == null) return;


		_lifeTime = LifeTime;
		_rigidBody.Freeze = true;
		CallDeferred("reparent", body);


		if (_type == ArrowType.Normal)
		{
			_state = State.Hit;
			if (body is Enemy enemy)
			{
				enemy.TakeDamage(_player.GlobalPosition, Damage);
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
		_player.ArrowHooked(this);

		if (body is PickUp pickup)
		{
			_hookedPickup = pickup;
			_hookedPickup.ArrowHooked(this);
		}
	}

	private void DrawHookLine()
	{
		_hookLine?.QueueFree();
		_hookLine = Draw.Line3D(_player.GetParent(), GlobalPosition, _player.GlobalPosition, _hookColor);
	}

	public void Destroy()
	{
		_player?.ArrowHooked(null);
		_hookedPickup?.ArrowHooked(null);

		_hookLine?.QueueFree();
		QueueFree();
	}
}
