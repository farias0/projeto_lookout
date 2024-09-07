#nullable enable

using System.Threading.Tasks;
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

    private Vector3 _targetVelocity = Vector3.Zero;
	private bool _isCrouching = false;
	private Node3D? _arrow;


    public override void _Ready()
    {
        Resources.PlayerRef = this;
    }

    public override void _PhysicsProcess(double delta)
	{
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
		if (e.IsActionPressed("fire"))				PullArrowBack();
		else if (e.IsActionReleased("fire"))		FireArrow();
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
	private void PullArrowBack()
	{
		if (_arrow != null)
        {
            Debug.LogError("Tried to pull back arrow, but an arrow already exists.");
            return;
        }

		Node node = Resources.Arrow.Instantiate();
        Node3D? arrow = node as Node3D;

        Vector3 spawnPos = arrow!.GlobalPosition +
								arrow!.Basis.X.Normalized() * 0.3f +
                                arrow!.Basis.Z.Normalized() * -0.4f +
                                arrow!.Basis.Y.Normalized() * 1.4f;
		arrow!.GlobalPosition = spawnPos;


        AddChild(arrow);
        _arrow = arrow;
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
            Arrow? a = _arrow.GetChild<Node3D>(0) as Arrow;
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
}
