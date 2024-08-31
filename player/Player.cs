using System.Threading.Tasks;
using Godot;

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
	private PackedScene _arrow = GD.Load<PackedScene>("res://bow-n-arrow/arrow/arrow.tscn"); 

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
		if (e.IsActionPressed("fire"))				FireArrow();
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

	private void FireArrow()
	{
		Node instance = _arrow.Instantiate();
		GetParent().AddChild(instance);

		var node = instance as Node3D;

		Vector3 spawnPosition = GlobalTransform.Origin +
									GlobalTransform.Basis.Y * 1.6f +
									GlobalTransform.Basis.Z * -0.9f;
		node.GlobalTransform = new Transform3D(GlobalTransform.Basis, spawnPosition);

		Vector3 arrowDirection = -GlobalTransform.Basis.Z;
		var rb = node as RigidBody3D;
		rb?.ApplyCentralImpulse(arrowDirection * 10.0f);
	}
}
