using Godot;
using System;
using System.Diagnostics;

public partial class Arrow : Node3D
{
	[Export]
	public float Speed { get; set; } = 15;

	private const float LifeTime = 5;

    private float _lifeTime = LifeTime;
	private RigidBody3D _rigidBody;
	private bool _hasHitATarget = false;
    private Vector3? _hitPoint;
    private Vector3? _rotation;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		_rigidBody = GetNode<RigidBody3D>("RigidBody3D");
        _rigidBody.ContactMonitor = true; // Necessary for detecting collision from the RigidBody3D
        _rigidBody.MaxContactsReported = 1;
        _rigidBody.Connect("body_entered", new Callable(this, nameof(OnCollision)));
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        _lifeTime -= (float)delta;
        if (_lifeTime <= 0)
        {
            Destroy();
            return;
        }

        if (_hasHitATarget) StayInPlace();
        else                MoveArrow((float)delta);
    }


    private void MoveArrow(float delta)
    {
        Position -= Transform.Basis.X * Speed * delta;
    }

    // Necessary because the arrow in a child node of a moving entity
    private void StayInPlace()
    {
        GlobalPosition = _hitPoint.Value;
        GlobalRotation = _rotation.Value;
        _rigidBody.LinearVelocity = Vector3.Zero;
    }

	private void OnCollision(Node body)
    {
		if (body is Player) return;

        _hasHitATarget = true;
        _hitPoint = GlobalPosition;
        _rotation = GlobalRotation;
        _lifeTime = LifeTime;
    }

	private void Destroy()
	{
		GetParent().QueueFree();
	}
}
