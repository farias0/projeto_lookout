#nullable enable

using Godot;
using projeto_lookout.libs;
using System.Diagnostics;

public partial class Player : CharacterBody3D
{
	[ExportGroup("Speed")]
	[Export]
	public int SpeedWalking { get; set; } = 12;
	[Export]
	public int SpeedCrouched { get; set; } = 6;
	[Export]
	public int SpeedSliding { get; set; } = 17;
	[Export]
	public int SpeedSlideMin { get; set; } = 10;
	[ExportGroup("Movement")]
	[Export]
	public float Acceleration = 60.0f;
	[Export]
	public float Deceleration = 40.0f;
	[Export]
	public int FallAcceleration { get; set; } = 40;
	[Export]
	public int JumpHeight { get; set; } = 14;
	[Export]
	public int JumpHeightHooked { get; set; } = 19;
	[Export]
	public float SlideDuration { get; set; } = 0.7f;
	[Export]
	public float RocketPushback { get; set; } = 30;
	[Export]
	public float DashImpulse { get; set; } = 55;
	[ExportGroup("")]
	[Export]
	public float HookSpeed { get; set; } = 3000;
	[Export]
	public int Health { get; set; } = 100;
	[ExportGroup("Stamina")]
	[Export]
	public float Stamina { get; set; } = 100;
	[Export]
	public float StaminaRegenRate { get; set; } = 3.5f;
	[Export]
	public float StaminaRegenDelay { get; set; } = 2.0f;
	[Export]
	public float StaminaCostHook { get; set; } = 40;
	[Export]
	public float StaminaCostDash { get; set; } = 20;
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
	private float _maxStamina;
	private float _staminaRegenCountdown;
	private PlayerAudio? _audio;
	private EffectsAudio? _effectsAudio;
	private BowAudio? _bowAudio;
	private PullerAudio? _pullerAudio;
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
	}

	public override void _Process(double delta)
	{
		ProcessInvencibilityCounter((float)delta);
		if (_arrowLoadCountdown > 0) _arrowLoadCountdown -= (float)delta;
		if (_airCrouchSlideCountdown > 0) _airCrouchSlideCountdown -= (float)delta;

		RegenerateStamina((float)delta);

		if (GlobalPosition.Y < MinY)
		{
			DieAndReset();
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
			if (_slideCountdown <= 0)
			{
				// Finish sliding
				SetCrouching(true);
			}
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
				direction = direction.Rotated(Vector3.Up, Rotation.Y);

				_targetVelocity.X = Mathf.MoveToward(_targetVelocity.X, direction.X * GetCurrentSpeed(), Acceleration * (float)delta);
				_targetVelocity.Z = Mathf.MoveToward(_targetVelocity.Z, direction.Z * GetCurrentSpeed(), Acceleration * (float)delta);

				GetNode<Node3D>("Pivot").Basis = Basis.LookingAt(direction);
			}
			else
			{
				// Deaccelerate
				_targetVelocity.X = Mathf.MoveToward(_targetVelocity.X, 0, Deceleration * (float)delta);
				_targetVelocity.Z = Mathf.MoveToward(_targetVelocity.Z, 0, Deceleration * (float)delta);
			}

			if (direction != Vector3.Zero && GetCurrentSpeed() == SpeedCrouched && IsOnFloor())
				_audio!.PlayCrouchedWalk();
			else if (direction != Vector3.Zero && GetCurrentSpeed() == SpeedWalking && IsOnFloor())
				_audio!.PlayWalk();
			else _audio!.StopMoving();
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
		var inventory = Resources.Instance.Inventory;


		if (e is InputEventMouseMotion eventMouseMotion && IsMouseInputEnabled())
		{
			Vector2 move = eventMouseMotion.ScreenRelative;
			// Rotates the player left and right
			RotateY(Mathf.DegToRad(-move.X * Camera.Sensitivity));
		}

		if (e.IsActionPressed("crouch_toggle"))		ToggleCrouch();
		if (e.IsActionPressed("jump"))				Jump();
		if (e.IsActionPressed("item_1"))			UseHealthPotion();
		if (e.IsActionPressed("item_2"))			UseStaminaPotion();
		if (e.IsActionPressed("activate_boot"))		ActivateBoot();			
		if (e.IsActionPressed("interact"))			Interact();
		if (IsMouseInputEnabled())
		{
			if (e.IsActionPressed("fire")) PullArrowBack(ArrowType.Normal);
			else if (e.IsActionReleased("fire")) FireArrow();
			if (e.IsActionPressed("fire_2") && inventory.BowItemEquipped != BowItemType.None)
			{
				// Use Bow Item
				PullArrowBack(inventory.BowItemEquipped switch
				{
					BowItemType.Hook => ArrowType.Hook,
					BowItemType.Rocket => ArrowType.Rocket,
					_ => throw new System.NotImplementedException()
				});
			}
			else if (e.IsActionReleased("fire_2")) FireArrow();
		}

		// Debug
		if (e.IsActionPressed("toggle_stamina"))	ToggleStamina();
	}

	public static bool IsMouseInputEnabled()
	{
		return !Resources.Instance.Inventory.IsEnabled() &&
				Resources.Instance.OngroingTrade == null;
	}

	public static int GetGoldAmount() => Resources.Instance.Inventory.Gold;

	public void TakeDamage(int damage)
	{
		if (_invincibilityCountdown > 0) return;

		Health -= damage;

		if (Health <= 0)
		{
			DieAndReset();
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

	public bool SubtractGold(int amount)
	{
		return Resources.Instance.Inventory.SubtractGold(amount);
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

	public void SpawnItem(PackedScene itemScene)
	{
		if (itemScene == null) return;

		var item = itemScene.Instantiate() as Node3D;
		item!.GlobalPosition = GlobalPosition + (-Basis.Z * 4);
		GetParent().AddChild(item);
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

		SetCrouching(!_isCrouching);

		var playerVel = new Vector2(_targetVelocity.X, _targetVelocity.Z);
		if (_isCrouching && IsOnFloor() && playerVel.Length() > 10)
		{
			Slide();
		}
	}

	private void SetCrouching(bool isCrouching)
	{
		_isCrouching = isCrouching;
	}

	private void Slide()
	{
		var playerVel = new Vector2(_targetVelocity.X, _targetVelocity.Z);
		if (playerVel.Length() < SpeedSlideMin) return;

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
		SetCrouching(false);
	}

	private void UseHealthPotion()
	{
		if (Health == _maxHealth)
		{
			return;
		}

		if (!Resources.Instance.Inventory.SpendHealthPotion())
		{
			return;
		}

		Health += HealthPotion.HealAmount;
		if (Health > _maxHealth) Health = _maxHealth;
		SyncHealthHUD();

		_effectsAudio!.PlayHeal();
	}

	private void UseStaminaPotion()
	{
		if (Stamina == _maxStamina)
		{
			return;
		}

		if (!Resources.Instance.Inventory.SpendStaminaPotion())
		{
			return;
		}

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
		// TODO rocket check

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

		var arrow = _pulledBackArrow as Arrow;

		if (arrow!.GetType() == ArrowType.Hook)
			ConsumeStamina(StaminaCostHook);
		else if (arrow!.GetType() == ArrowType.Rocket)
		{
			ApplyRocketPushback();
		}

		Vector3 pos = _pulledBackArrow.GlobalPosition;
		_pulledBackArrow.Reparent(GetParent());
		_pulledBackArrow.GlobalPosition = pos;

		Vector3 target = AimingAt();
		_pulledBackArrow.LookAt(target);

		arrow!.Fire();

		if (arrow!.GetType() == ArrowType.Rocket)
			_bowAudio!.PlayFiredRocket();
		else
			_bowAudio!.PlayFired();


		_pulledBackArrow = null;
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
		_hookedArrow!.QueueFree();
		_hookedArrow = null;
		_targetVelocity = Velocity;
	}

	private void CancelSlide()
	{
		if (_slideCountdown > 0)
		{
			_slideCountdown = -1;
			_effectsAudio!.CancelSlide();
		}
	}

	private void ApplyRocketPushback()
	{
		float cameraRot = Resources.Instance.Camera.Rotation.X;
		Vector3 direction = Basis.Z.Normalized().Rotated(Basis.X.Normalized(), cameraRot);
		_targetVelocity += direction * RocketPushback;
	}

	private void ActivateBoot()
	{
		var item = Resources.Instance.Inventory.BootsItemEquipped;

		switch (item)
		{
			case BootsItemType.None:
				// TODO play error sound
				return;
			case BootsItemType.Dash:
				Dash();
				return;
			default:
				throw new System.NotImplementedException();
		}
	}

	private void Dash()
	{
		if (Velocity == Vector3.Zero) return;

		if (ConsumeStamina(StaminaCostDash))
		{
			if (_hookedArrow != null) LeaveHookedArrow();

			// Omnidirectional (OP)
			// _targetVelocity = Velocity.Normalized() * DashImpulse;

			// Horizontal only (nerfed)
			var direction2D = (new Vector2(Velocity.X, Velocity.Z)).Normalized();
			var direction = new Vector3(direction2D.X, 0, direction2D.Y);
			_targetVelocity = direction * DashImpulse;

				_effectsAudio!.PlayDash();
		}
		else {
			// TODO play error sound
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

	private void DieAndReset()
	{
		GlobalPosition = _startingPos;
		Health = _maxHealth;
		Stamina = _maxStamina;
		GlobalRotation = _startingRot;
		Resources.Instance.HUD.SetHealth(1);
		Resources.Instance.HUD.SetStamina(1);
		Resources.Instance.Camera.Reset();
		Resources.Instance.Inventory.LoseAllUnprotectedItems();
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

	private void ToggleStamina()
	{
		_staminaEnabled = !_staminaEnabled;
		Resources.Instance.HUD.SetStaminaBarVisible(_staminaEnabled);
	}
}
