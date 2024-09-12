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
    private readonly Color Color = new(0f, 1f, 0f);

    private float _lifeTime = LifeTime;
	private RigidBody3D _rigidBody;
	private State _state = State.PulledBack;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		_rigidBody = GetChild<RigidBody3D>(0);
        _rigidBody.ContactMonitor = true; // Necessary for detecting collision from the RigidBody3D
        _rigidBody.MaxContactsReported = 1;
        _rigidBody.Connect("body_entered", new Callable(this, nameof(OnBodyEntered)));
        _rigidBody.Freeze = true;

        PaintSolidColor();
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

    private void PaintSolidColor()
    {
        var material = new StandardMaterial3D
        {
            AlbedoColor = Color
        };
        _rigidBody.GetNode<MeshInstance3D>("MeshNode/Arrow").MaterialOverride = material;
    }

    public void Fire()
    {
        if (_state != State.PulledBack)
        {
            Debug.LogError("Arrow fired, but it's not pulled back.");
            return;
        }

        _state = State.Flying;
        _rigidBody.Freeze = false;
    }


    private void MoveArrow(float delta)
    {
        Position -= Transform.Basis.Z * Speed * delta;
    }


	private void OnBodyEntered(Node body)
    {
        Debug.Log($"Arrow hit {body.Name}");

        if (_state != State.Flying) return;
        if (body is Player) return;

        _state = State.Hit;
        _lifeTime = LifeTime;

        if (body is Enemy enemy)
        {
            Reparent(body);
            var player = GetParent() as Node3D;
            enemy.TakeDamage(player.GlobalPosition, Damage);
        }
    }

	private void Destroy()
	{
		QueueFree();
	}
}
