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
    public int FallAcceleration { get; set; } = 75;
	[Export]
	public int JumpHeight { get; set; } = 20;
	[Export]
	public float HookSpeed { get; set; } = 3000;


    private const float MinY = -70;


    private Vector3 _targetVelocity = Vector3.Zero;
	private bool _isCrouching = false;
	private Node3D? _arrow;
	private static Vector3 _startingPos;
	private static Node3D? _hookedArrow;


    public override void _Ready()
    {
        Resources.PlayerRef = this;
        _startingPos = GlobalPosition;
    }

    public override void _Process(double delta)
	{
        if (GlobalPosition.Y < MinY)
        {
			Reset();
        }
    }

    public override void _PhysicsProcess(double delta)
	{
		if (_hookedArrow != null)
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

	public void ArrowHooked(Node3D arrow)
    {
        if (_hookedArrow != null)
        {
            (arrow as Arrow)!.Destroy();
        }
		else
		{
            _hookedArrow = arrow;
        }
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
		if (_arrow != null)
        {
			// TODO play error sound
            return;
        }

		if (type == ArrowType.Hook && _hookedArrow != null)
        {
            (_hookedArrow as Arrow)!.Destroy();
            _hookedArrow = null;
            return;
        }

        Node node = Resources.Arrow.Instantiate();
        Node3D? node3d = node as Node3D;

        Vector3 spawnPos = node3d!.GlobalPosition +
								node3d!.Basis.X.Normalized() * 0.3f +
                                node3d!.Basis.Z.Normalized() * -0.4f +
                                node3d!.Basis.Y.Normalized() * 1.4f;
		node3d!.GlobalPosition = spawnPos;


		Arrow? arrow = node3d as Arrow;
		arrow!.SetType(type);
		arrow!.SetPlayer(this);


        AddChild(node3d);
        _arrow = node3d;
    }

	/// <summary>
	/// Fires an arrow that's pulled back
	/// </summary>
	private void FireArrow()
	{
		if (_arrow == null) return;

		Vector3 pos = _arrow.GlobalPosition;
        RemoveChild(_arrow);
		GetParent().AddChild(_arrow);
        _arrow.GlobalPosition = pos;

        Vector3 target = AimingAt();
        _arrow.LookAt(target);

		{
            Arrow? a = _arrow as Arrow;
            a!.Fire();
        }

        _arrow = null;
    }

	/// <returns>The point in the world the player's aiming at</returns>
	private Vector3 AimingAt()
	{
        Camera3D camera = GetNode<Camera3D>("Camera3D");

        Vector3 rayOrigin = camera.GlobalTransform.Origin;
        Vector3 rayDirection = camera.GlobalTransform.Basis.Z.Normalized() * -1;

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
        if (_hookedArrow!.GlobalPosition.DistanceTo(GlobalPosition) < 2.5f)
        {
            (_hookedArrow as Arrow)!.Destroy();
            _hookedArrow = null;
            return;
        }

        var direction = (_hookedArrow.GlobalPosition - GlobalPosition).Normalized();
        Velocity = direction * HookSpeed * (float)delta;
        MoveAndSlide();
    }

	private void Reset()
    {
        GlobalPosition = _startingPos;
    }
}
