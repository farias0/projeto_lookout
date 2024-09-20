#nullable enable

using Godot;
using projeto_lookout.libs;

public partial class Player : CharacterBody3D
{
	[ExportGroup("Speed")]
	[Export]
	public int SpeedWalking { get; set; } = 12;
	[Export]
	public int SpeedCrouched { get; set; } = 6;
	[Export]
	public int SpeedSliding { get; set; } = 17;
	[ExportGroup("")]
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
	[Export]
	public float SlideDuration { get; set; } = 0.7f;
	[ExportGroup("Stamina")]
	[Export]
	public float Stamina { get; set; } = 100;
	[Export]
	public float StaminaRegenRate { get; set; } = 3.5f;
	[Export]
	public float StaminaRegenDelay { get; set; } = 2.0f;
	[Export]
	public float StaminaCostHook { get; set; } = 40;
	[ExportGroup("")]
	[Export]
	public float InvincibilityTime { get; set; } = 2.0f;
	[ExportGroup("Sound")]
	[Export]
	public float DistanceHearWalking { get; set; } = 12.0f;
	[Export]
	public float DistanceHearCrouched { get; set; } = 1.5f;
	[ExportGroup("")]
	[Export]
	public float ArrowLoadTime { get; set; } = 0.6f; // How long for the arrow to load so it can be shot
	[Export]
	public float InteractDistance { get; set; } = 3.0f; // The distance to initiate an interaction by pressing the Interact button


	private const float MinY = -70;
	private const float HeightCrouched = 0.7f; // In scale
	private const float HeightSliding = 0.3f;
	private const float AirCrouchSlideTolerance = 0.15f; // How long before touching the floor the player can crouch to slide


	private Vector3 _targetVelocity = Vector3.Zero;
	private bool _isCrouching = false;
	private Node3D? _pulledBackArrow;
	private Vector3 _startingPos;
	private Node3D? _hookedArrow;
	private int _maxHealth;
	private float _invincibilityCountdown;
	private Vector3 _startingRot;
	private Node3D _bow = new();
	private int _gold;
	private float _maxStamina;
	private float _staminaRegenCountdown;
	private PlayerAudio? _audio;
	private EffectsAudio? _effectsAudio;
	private BowAudio? _bowAudio;
	private PullerAudio? _pullerAudio;
	private int _healthPotionCount = 0;
	private int _staminaPotionCount = 0;
	private float _slideCountdown = 0;
	private float _arrowLoadCountdown = -1;
	private float _airCrouchSlideCountdown = -1;
	private bool _wasOnFloorLastFrame;

	// Debug
	private bool _staminaEnabled = true;


	public override void _Ready()
	{
		Resources.Instance.Player = this;

		_audio = GetNode<PlayerAudio>("AudioStreamPlayer");
		_effectsAudio = GetNode<EffectsAudio>("EffectsAudioStreamPlayer");

		_bow = Resources.Instance.Camera.GetNode<Node3D>("Bow");
		_bowAudio = _bow.GetNode<AudioStreamPlayer3D>("AudioStreamPlayer3D")! as BowAudio;

		var puller = Resources.Instance.Camera.GetNode<Node3D>("Puller");
		_pullerAudio = puller.GetNode<AudioStreamPlayer3D>("AudioStreamPlayer3D")! as PullerAudio;

		_startingPos = GlobalPosition;
		_startingRot = GlobalRotation;
		_maxHealth = Health;
		_maxStamina = Stamina;

		Resources.Instance.HUD.SetGoldAmount(_gold);
	}

	public override void _Process(double delta)
	{
		ProcessInvencibilityCounter((float)delta);
		if (_arrowLoadCountdown > 0) _arrowLoadCountdown -= (float)delta;
		if (_airCrouchSlideCountdown > 0) _airCrouchSlideCountdown -= (float)delta;

		RegenerateStamina((float)delta);

		if (GlobalPosition.Y < MinY)
		{
			Reset();
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		var isOnFloor = IsOnFloor();

		if (_hookedArrow != null)
		{
			_pullerAudio!.PlayPullIn();
			if ((_hookedArrow as Arrow)!.HookIsShooterPulled())
			{
				// Pulled by hook

				PulledByHook((float)delta);
				return;
			}
		}
		else _pullerAudio!.StopSound();


		if (_slideCountdown > 0)
		{
			// Sliding

			if (_slideCountdown == SlideDuration)
			{
				_audio!.StopContinuousSound();
				_effectsAudio!.PlaySlide();

				_targetVelocity = _targetVelocity.Normalized() * GetCurrentSpeed();
				GetNode<Node3D>("Pivot").Basis = Basis.LookingAt(_targetVelocity);

			}
			_slideCountdown -= (float)delta;
		}
		else
		{
			// Moving normally

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

			if (direction != Vector3.Zero && GetCurrentSpeed() == SpeedCrouched && IsOnFloor())
				_audio!.PlayCrouchedWalk();
			else if (direction != Vector3.Zero && GetCurrentSpeed() == SpeedWalking && IsOnFloor())
				_audio!.PlayWalk();
			else _audio!.StopMoving();

			direction = direction.Rotated(Vector3.Up, Rotation.Y);

			_targetVelocity.X = direction.X * GetCurrentSpeed();
			_targetVelocity.Z = direction.Z * GetCurrentSpeed();
		}

		if (IsOnFloor())
		{
			var playerVel = new Vector2(_targetVelocity.X, _targetVelocity.Z);
			if (!_wasOnFloorLastFrame && _airCrouchSlideCountdown > 0 && playerVel.Length() > 10)
			{
				Slide();
			}
		}
		else
		{
			// Gravity
			_targetVelocity.Y -= FallAcceleration * (float)delta;
		}


		Velocity = _targetVelocity;
		MoveAndSlide();

		SyncHeightWithState();

		_wasOnFloorLastFrame = isOnFloor;
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
		if (e.IsActionPressed("item_1"))			UseHealthPotion();
		if (e.IsActionPressed("item_2"))			UseStaminaPotion();
		if (e.IsActionPressed("interact"))			Interact();

		// Debug
		if (e.IsActionPressed("toggle_stamina"))	ToggleStamina();
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
			SyncHealthHUD();
			_invincibilityCountdown = InvincibilityTime;
		}

		_audio!.PlayGotHit();
	}

	/// <param name="value">The value to be consumed</param>
	/// <returns>If the operation is allowed</returns>
	public bool ConsumeStamina(float value)
	{
		if (!_staminaEnabled) return true;

		if (Stamina - value < 0)
		{
			return false;
		}

		Stamina -= value;
		_staminaRegenCountdown = StaminaRegenDelay;
		Resources.Instance.HUD.SetStamina(Stamina / _maxStamina);

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
			Resources.Instance.HUD.SetStamina(Stamina / _maxStamina);
		}
	}

	public int GetGoldAmount() => _gold;

	public void PickUpGold(int amount)
	{
		_gold += amount;
		Resources.Instance.HUD.SetGoldAmount(_gold);
		_effectsAudio!.PlayCollectGold();
	}

	public bool SubtractGold(int amount)
	{
		if (amount < _gold) return false;
		_gold -= amount;
		Resources.Instance.HUD.SetGoldAmount(_gold);
		// _effectsAudio!.PlaySubtractGold();
		return true;
	}

	public void PickUpHealthPotion()
	{
		_healthPotionCount++;
		Resources.Instance.HUD.SetHealthPotionAmount(_healthPotionCount);
		_effectsAudio!.PlayCollectPotion();
	}
	public void PickUpStaminaPotion()
	{
		_staminaPotionCount++;
		Resources.Instance.HUD.SetStaminaPotionAmount(_staminaPotionCount);
		_effectsAudio!.PlayCollectPotion();
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

	/// <summary>
	/// If someone at that point can hear the player this frame
	/// </summary>
	public bool HearsPlayerThisFrame(Vector3 point)
	{
		// Not moving
		if (_targetVelocity.X == 0 && _targetVelocity.Z == 0) return false;

		var dist = GlobalPosition.DistanceTo(point);
		if (_isCrouching && dist <= DistanceHearCrouched) return true;
		if (dist <= DistanceHearWalking) return true;

		return false;
	}

	private float GetCurrentSpeed()
	{
		return _slideCountdown > 0 ? SpeedSliding :
				(_isCrouching ? SpeedCrouched : SpeedWalking);
	}

	private void ToggleCrouch()
	{
		if (!IsOnFloor())
		{
			// Attemps air slide
			_airCrouchSlideCountdown = AirCrouchSlideTolerance;
			return;
		}

		CancelSlide();

		_isCrouching = !_isCrouching;

		var playerVel = new Vector2(_targetVelocity.X, _targetVelocity.Z);
		if (_isCrouching && IsOnFloor() && playerVel.Length() > 10)
		{
			Slide();
		}
	}

	private void Slide()
	{
		_slideCountdown = SlideDuration;
	}

	private void Jump()
	{
		if (_hookedArrow != null)
		{
			LeaveHookedArrow();
			_targetVelocity.Y = JumpHeightHooked;
			_audio!.PlayJump();
			return;
		}

		if (!IsOnFloor()) return;

		CancelSlide();
		_targetVelocity.Y = JumpHeight;
		_audio!.PlayJump();
	}

	private void UseHealthPotion()
	{
		if (_healthPotionCount < 1)
		{
			return;
		}
		if (Health == _maxHealth)
		{
			return;
		}

		_healthPotionCount--;
		SyncHealthPotionHUD();

		Health += HealthPotion.HealAmount;
		if (Health > _maxHealth) Health = _maxHealth;
		SyncHealthHUD();

		_effectsAudio!.PlayHeal();
	}

	private void UseStaminaPotion()
	{
		if (_staminaPotionCount < 1)
		{
			return;
		}
		if (Stamina == _maxStamina)
		{
			return;
		}

		_staminaPotionCount--;
		SyncStaminaPotionHUD();

		Stamina += StaminaPotion.FillAmount;
		if (Stamina > _maxStamina) Stamina = _maxStamina;
		SyncStaminaHUD();

		_effectsAudio!.PlayFillStamina();
	}

	private void Interact()
	{
		var rayOrigin = GlobalPosition;
		rayOrigin.Y += GetHeight() * 0.5f;
		var rayResult = Raycast.CastRayInDirection(GetWorld3D(), rayOrigin, Basis.Z.Normalized() * -1, InteractDistance);
		
		if (rayResult.ContainsKey("collider"))
		{
			var collider = (Node)rayResult["collider"] as Node3D;
			// TODO pick up pickups
			if (collider is Npc npc)
			{
				npc.InteractWith(this);
			}
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

		if (type == ArrowType.Hook && Stamina < StaminaCostHook && _staminaEnabled)
		{
			// TODO play error sound
			return;
		}

		Node node = Resources.Instance.Arrow.Instantiate();
		Node3D? node3d = node as Node3D;

		Vector3 spawnPos = node3d!.Basis.X.Normalized() * 0.3f +
								node3d!.Basis.Z.Normalized() * -0.4f +
								node3d!.Basis.Y.Normalized() * -0.3f;
		node3d!.GlobalPosition = spawnPos;


		Arrow? arrow = node3d as Arrow;
		arrow!.SetType(type);
		arrow!.SetShooter(this);


		Resources.Instance.Camera.AddChild(node3d);
		_pulledBackArrow = node3d;

		_arrowLoadCountdown = ArrowLoadTime;

		_bowAudio!.PlayTensing();
	}

	/// <summary>
	/// Fires an arrow that's pulled back
	/// </summary>
	private void FireArrow()
	{
		if (_pulledBackArrow == null) return;

		if (_arrowLoadCountdown > 0)
		{
			// Didn't pull it back enough
			_pulledBackArrow.QueueFree();
			_pulledBackArrow = null;
			_bowAudio!.CancelTensing();
			_arrowLoadCountdown = -1;
			return;
		}
		_arrowLoadCountdown = -1;

		if ((_pulledBackArrow as Arrow)!.GetType() == ArrowType.Hook)
			ConsumeStamina(StaminaCostHook);

		Vector3 pos = _pulledBackArrow.GlobalPosition;
		_pulledBackArrow.Reparent(GetParent());
		_pulledBackArrow.GlobalPosition = pos;

		Vector3 target = AimingAt();
		_pulledBackArrow.LookAt(target);

		{
			Arrow? a = _pulledBackArrow as Arrow;
			a!.Fire();
		}

		_pulledBackArrow = null;

		_bowAudio!.PlayFired();
	}

	/// <returns>The point in the world the player's aiming at</returns>
	private Vector3 AimingAt()
	{
		Vector3 targetPoint;

		Vector3 rayOrigin = Resources.Instance.Camera.GlobalTransform.Origin;
		Vector3 rayDirection = Resources.Instance.Camera.GlobalTransform.Basis.Z.Normalized() * -1;
		var rayResult = Raycast.CastRayInDirection(GetWorld3D(), rayOrigin, rayDirection, 1000);

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

		_audio!.StopMoving();
	}

	private void LeaveHookedArrow()
	{
		(_hookedArrow as Arrow)!.DetachShooter();
		_hookedArrow = null;
		Velocity = Vector3.Zero;
		_targetVelocity = Vector3.Zero;
	}

	private void CancelSlide()
	{
		if (_slideCountdown > 0)
		{
			_slideCountdown = -1;
			_effectsAudio!.CancelSlide();
		}
	}

	// TODO get rid of this workaround
	private void SyncHeightWithState()
	{
		float height = 1;
		if (_slideCountdown > 0) height = HeightSliding;
		else if (_isCrouching) height = HeightCrouched;

		if (height != Scale.Y)
		{
			Scale = new Vector3(1, height, 1);
		}
	}

	private void Reset()
	{
		GlobalPosition = _startingPos;
		Health = _maxHealth;
		Stamina = _maxStamina;
		GlobalRotation = _startingRot;
		Resources.Instance.HUD.SetHealth(1);
		Resources.Instance.HUD.SetStamina(1);
		Resources.Instance.Camera.Reset();
		_bow.Visible = true;
		_invincibilityCountdown = -1;
		_staminaRegenCountdown = -1;
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

	private void SyncHealthHUD()
	{
		Resources.Instance.HUD.SetHealth((float)Health / _maxHealth);
	}

	private void SyncStaminaHUD()
	{
		Resources.Instance.HUD.SetStamina((float)Stamina / _maxStamina);
	}

	private void SyncHealthPotionHUD()
	{
		Resources.Instance.HUD.SetHealthPotionAmount(_healthPotionCount);
	}

	private void SyncStaminaPotionHUD()
	{
		Resources.Instance.HUD.SetStaminaPotionAmount(_staminaPotionCount);
	}

	private void ToggleStamina()
	{
		_staminaEnabled = !_staminaEnabled;
		Resources.Instance.HUD.SetStaminaBarVisible(_staminaEnabled);
	}
}
