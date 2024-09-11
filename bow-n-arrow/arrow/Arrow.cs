using Godot;
using System;
using projeto_lookout.libs;

public partial class Arrow : Node3D
{
    private const int Damage = 40;

    public enum State
    {
        PulledBack,
        Flying,
        Hit
    }

    [Export]
    public float Speed { get; set; } = 45;

	private const float LifeTime = 5;

    private float _lifeTime = LifeTime;
	private RigidBody3D _rigidBody;
	private State _state = State.PulledBack;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		_rigidBody = GetNode<RigidBody3D>("RigidBody3D");
        _rigidBody.ContactMonitor = true; // Necessary for detecting collision from the RigidBody3D
        _rigidBody.MaxContactsReported = 1;
        _rigidBody.Connect("body_entered", new Callable(this, nameof(OnBodyEntered)));
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
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_state == State.Flying)
        {
            MoveArrow((float)delta);
        }
    }

    public void Fire()
    {
        if (_state != State.PulledBack)
        {
            Debug.LogError("Arrow fired, but it's not pulled back.");
            return;
        }

        //_rigidBody.ApplyCentralImpulse(Transform.Basis.X * Speed);

        _state = State.Flying;
    }


    private void MoveArrow(float delta)
    {
        Position -= Transform.Basis.X * Speed * delta;
    }


	private void OnBodyEntered(Node body)
    {
        if (_state != State.Flying) return;
        if (body is Player) return;

        Debug.Log("Arrow collided with " + body.Name + " pos: " + GlobalPosition);

        _state = State.Hit;
        _lifeTime = LifeTime;

        if (body.GetParent() is Enemy enemy)
        {
            GetParent().Reparent(body.GetParent());
            enemy.TakeDamage(Damage);
        }
    }

	private void Destroy()
	{
		GetParent().QueueFree();
	}
}
