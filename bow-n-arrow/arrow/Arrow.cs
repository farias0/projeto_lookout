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
	private State state = State.PulledBack;


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
        if (state == State.Flying || state == State.Hit)
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
        if (state == State.Flying)
        {
            MoveArrow((float)delta);
        }
    }

    public void Fire()
    {
        if (state != State.PulledBack)
        {
            Debug.LogError("Arrow fired, but it's not pulled back.");
            return;
        }

        state = State.Flying;
    }


    private void MoveArrow(float delta)
    {
        Position -= Transform.Basis.X * Speed * delta;
    }


	private void OnCollision(Node body)
    {
		if (body is Player) return;
        if (state == State.PulledBack) return;

        state = State.Hit;
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
