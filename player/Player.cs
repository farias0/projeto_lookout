#nullable enable

using Godot;
using projeto_lookout.libs;

public partial class Player : CharacterBody3D
{
	[Export]
	public int Speed { get; set; } = 12;
	[Export]
	public int SpeedCrouched { get; set; } = 6;
	[Export]
	public int FallAcceleration { get; set; } = 40;
	[Export]
	public int JumpHeight { get; set; } = 14;
	[Export]
	public int JumpHeightHooked { get; set; } = 19;
	[Export]
	public float HookSpeed { get; set; } = 3000;
	[Export]
	public int Health { get; set; } = 100;
	[ExportGroup("Stamina")]
	[Export]
	public float Stamina { get; set; } = 100;
	[Export]
	public float StaminaRegenRate { get; set; } = 3.0f;
	[Export]
	public float StaminaRegenDelay { get; set; } = 1.0f;
	[Export]
	public float StaminaCostHook { get; set; } = 40;
	[ExportGroup("")]
	[Export]
	public float InvincibilityTime { get; set; } = 2.0f;


	private const float MinY = -70;


	private Vector3 _targetVelocity = Vector3.Zero;
	private bool _isCrouching = false;
	private Node3D? _pulledBackArrow;
	private Vector3 _startingPos;
	private Node3D? _hookedArrow;
	private int _maxHealth;
	private float _invincibilityCountdown;
	private Vector3 _startingRot;
	private Node3D _bow = new();
	private int _gold = 0;
	private float _maxStamina;
	private float _staminaRegenCountdown;


	public override void _Ready()
	{
		Resources.Player = this;

		_bow = GetNode<Node3D>("Bow");

		_startingPos = GlobalPosition;
		_startingRot = GlobalRotation;
		_maxHealth = Health;
		_maxStamina = Stamina;

		Resources.HUD.SetGoldAmount(_gold);
	}

	public override void _Process(double delta)
	{
		ProcessInvencibilityCounter((float)delta);

		RegenerateStamina((float)delta);

		if (GlobalPosition.Y < MinY)
		{
			Reset();
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_hookedArrow != null && (_hookedArrow as Arrow)!.HookIsShooterPulled())
		{
			PulledByHook((float)delta);
			return;
		}


		var direction = Vector3.Zero;

		if (Input.IsActionPressed("move_right"))
			direction.X += 1.0f;
		if (Input.IsActionPressed("move_left"))
			direction.X -= 1.0f;
		if (Input.IsActionPressed("move_up"))
			direction.Z -= 1.0f;
		if (Input.IsActionPressed("move_down"))
			direction.Z += 1.0f;

		if (direction != Vector3.Zero)
		{
			direction = direction.Normalized();
			GetNode<Node3D>("Pivot").Basis = Basis.LookingAt(direction);
		}

		direction = direction.Rotated(Vector3.Up, Rotation.Y);

		_targetVelocity.X = direction.X * GetCurrentSpeed();
		_targetVelocity.Z = direction.Z * GetCurrentSpeed();

		if (!IsOnFloor())
		{
			_targetVelocity.Y -= FallAcceleration * (float)delta;
		}

		Velocity = _targetVelocity;
		MoveAndSlide();
	}

	public override void _Input(InputEvent e)
	{
		if (e is InputEventMouseMotion eventMouseMotion)
		{
			Vector2 move = eventMouseMotion.ScreenRelative;
			// Rotates the player left and right
			RotateY(Mathf.DegToRad(-move.X * Camera.Sensitivity));
		}

		if (e.IsActionPressed("crouch_toggle"))		ToggleCrouch();
		if (e.IsActionPressed("jump"))				Jump();
		if (e.IsActionPressed("fire"))				PullArrowBack(ArrowType.Normal);
		else if (e.IsActionReleased("fire"))		FireArrow();
		if (e.IsActionPressed("fire_2"))			PullArrowBack(ArrowType.Hook);
		else if (e.IsActionReleased("fire_2"))		FireArrow();
	}

	public void TakeDamage(int damage)
	{
		if (_invincibilityCountdown > 0) return;

		Health -= damage;

		if (Health <= 0)
		{
			Reset();
		}
		else
		{
			Resources.HUD.SetHealth((float)Health / _maxHealth);
			_invincibilityCountdown = InvincibilityTime;
		}
	}

	/// <param name="value">The value to be consumed</param>
	/// <returns>If the operation is allowed</returns>
	public bool ConsumeStamina(float value)
	{
		if (Stamina - value < 0)
		{
			return false;
		}

		Stamina -= value;
		_staminaRegenCountdown = StaminaRegenDelay;
		Resources.HUD.SetStamina(Stamina / _maxStamina);

		return true;
	}

	public void RegenerateStamina(float delta)
	{
		if (_staminaRegenCountdown > 0)
		{
			_staminaRegenCountdown -= delta;
			return;
		}
		else if (Stamina < _maxStamina)
		{
			Stamina += StaminaRegenRate * delta;
			if (Stamina > _maxStamina) Stamina = _maxStamina;
			Resources.HUD.SetStamina(Stamina / _maxStamina);
		}
	}

	public void PickUpGold(int amount)
	{
		_gold += amount;
		Resources.HUD.SetGoldAmount(_gold);
	}

	public void ArrowHooked(Node3D arrow)
	{
		if (_hookedArrow != null)
		{
			LeaveHookedArrow();
		}

		_hookedArrow = arrow;
	}

	public bool IsInvincible()
	{
		return _invincibilityCountdown > 0;
	}

	public float GetHeight()
	{
		CollisionShape3D collisionShape = GetNode<CollisionShape3D>("CollisionShape3D");
		CylinderShape3D? cylinder = collisionShape.Shape as CylinderShape3D;
		return cylinder!.Height;
	}

	private float GetCurrentSpeed()
	{
		return _isCrouching ? SpeedCrouched : Speed;
	}

	private void ToggleCrouch()
	{
		_isCrouching = !_isCrouching;

		if (_isCrouching)
		{
			Scale = new Vector3(1, 0.5f, 1);
		}
		else
		{
			Scale = new Vector3(1, 1, 1);
		}
	}

	private void Jump()
	{
		if (_hookedArrow != null)
		{
			LeaveHookedArrow();
			_targetVelocity.Y = JumpHeightHooked;
			return;
		}


		if (IsOnFloor())
		{
			_targetVelocity.Y = JumpHeight;
		}
	}

	/// <summary>
	/// Prepares an arrow to be fired
	/// </summary>
	private void PullArrowBack(ArrowType type)
	{
		if (_pulledBackArrow != null)
		{
			// TODO play error sound
			return;
		}

		if (type == ArrowType.Hook && _hookedArrow != null)
		{
			LeaveHookedArrow();
			return;
		}

		if (type == ArrowType.Hook && !ConsumeStamina(StaminaCostHook))
		{
			// TODO play error sound
			return;
		}

		Node node = Resources.Arrow.Instantiate();
		Node3D? node3d = node as Node3D;

		Vector3 spawnPos = node3d!.Basis.X.Normalized() * 0.3f +
								node3d!.Basis.Z.Normalized() * -0.4f +
								node3d!.Basis.Y.Normalized() * 1.4f;
		node3d!.GlobalPosition = spawnPos;


		Arrow? arrow = node3d as Arrow;
		arrow!.SetType(type);
		arrow!.SetShooter(this);


		AddChild(node3d);
		_pulledBackArrow = node3d;
	}

	/// <summary>
	/// Fires an arrow that's pulled back
	/// </summary>
	private void FireArrow()
	{
		if (_pulledBackArrow == null) return;

		Vector3 pos = _pulledBackArrow.GlobalPosition;
		RemoveChild(_pulledBackArrow);
		GetParent().AddChild(_pulledBackArrow);
		_pulledBackArrow.GlobalPosition = pos;

		Vector3 target = AimingAt();
		_pulledBackArrow.LookAt(target);

		{
			Arrow? a = _pulledBackArrow as Arrow;
			a!.Fire();
		}

		_pulledBackArrow = null;
	}

	/// <returns>The point in the world the player's aiming at</returns>
	private Vector3 AimingAt()
	{
		Vector3 rayOrigin = Resources.Camera.GlobalTransform.Origin;
		Vector3 rayDirection = Resources.Camera.GlobalTransform.Basis.Z.Normalized() * -1;

		PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;
		PhysicsRayQueryParameters3D rayParams = new()
		{
			From = rayOrigin,
			To = rayOrigin + rayDirection * 1000.0f,
			CollideWithBodies = true,
			CollideWithAreas = true
		};

		var rayResult = spaceState.IntersectRay(rayParams);

		Vector3 targetPoint;

		if (rayResult.Count > 0)
		{
			targetPoint = (Vector3)rayResult["position"];
		}
		else
		{
			// Ray didn't hit anything, set target point far away along the ray direction
			targetPoint = rayOrigin + rayDirection * 1000.0f;
		}

		return targetPoint;
	}

	private void PulledByHook(float delta)
	{
		if (_hookedArrow!.GlobalPosition.DistanceTo(GlobalPosition) < 1.5f)
		{
			LeaveHookedArrow();
			return;
		}

		var direction = (_hookedArrow as Arrow)!.HookGetPullDirection();
		Velocity = direction * HookSpeed * (float)delta;
		MoveAndSlide();
	}

	private void LeaveHookedArrow()
	{
		(_hookedArrow as Arrow)!.DetachShooter();
		_hookedArrow = null;
		Velocity = Vector3.Zero;
		_targetVelocity = Vector3.Zero;
	}

	private void Reset()
	{
		GlobalPosition = _startingPos;
		Health = _maxHealth;
		GlobalRotation = _startingRot;
		Resources.HUD.SetHealth(1);
		Resources.Camera.Reset();
		_bow.Visible = true;
		_invincibilityCountdown = -1;
	}

	private void ProcessInvencibilityCounter(float delta)
	{
		if (_invincibilityCountdown <= 0) return;

		_invincibilityCountdown -= delta;

		// Blink effect
		_bow.Visible = _invincibilityCountdown % 0.2f > 0.1f;

		if (_invincibilityCountdown <= 0)
		{
			_invincibilityCountdown = -1;

			// Stop blinking
			_bow.Visible = true;
		}
	}
}
