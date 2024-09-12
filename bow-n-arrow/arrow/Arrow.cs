using Godot;
using System;
using projeto_lookout.libs;

public enum ArrowType
{
    Normal,
    Hook
}

public partial class Arrow : Node3D
{
    private const int Damage = 40;

    public enum State
    {
        PulledBack,
        Flying,
        Hit,
        Hooked,
    }

    [Export]
    public float Speed { get; set; } = 45;

	private const float LifeTime = 5;
    private readonly Color ColorNormal = new(0f, 1f, 0f);
    private readonly Color ColorHook = new(0.54f, 0.26f, 0.07f);

    private float _lifeTime = LifeTime;
	private RigidBody3D _rigidBody;
	private State _state = State.PulledBack;
    private ArrowType _type = ArrowType.Normal;
    private Player _player;
    private Node3D _hookLine;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		_rigidBody = GetChild<RigidBody3D>(0);
        _rigidBody.ContactMonitor = true; // Necessary for detecting collision from the RigidBody3D
        _rigidBody.MaxContactsReported = 1;
        _rigidBody.Connect("body_entered", new Callable(this, nameof(OnBodyEntered)));
        _rigidBody.Freeze = true;

        SetType(_type);
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
        else if (_state == State.Hooked)
        {
            DrawHookLine();
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_state == State.Flying)
        {
            MoveArrow((float)delta);
        }
    }

    private void PaintSolidColor(Color color)
    {
        var material = new StandardMaterial3D
        {
            AlbedoColor = color
        };
        _rigidBody.GetNode<MeshInstance3D>("MeshNode/Arrow").MaterialOverride = material;
    }

    public void SetType(ArrowType type)
    {
        _type = type;

        if (_rigidBody == null) return;

        switch (_type)
        {
            case ArrowType.Normal:
                PaintSolidColor(ColorNormal);
                break;
            case ArrowType.Hook:
                PaintSolidColor(ColorHook);
                break;
        }
    }

    public void SetPlayer(Player player)
    {
        _player = player;
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

        _lifeTime = LifeTime;
        _rigidBody.Freeze = true;
        Reparent(body);

        if (_type == ArrowType.Normal)
        {
            _state = State.Hit;
            if (body is Enemy enemy)
            {
                var player = GetParent() as Node3D;
                enemy.TakeDamage(player.GlobalPosition, Damage);
            }
        }
        else if (_type == ArrowType.Hook)
        {
            _state = State.Hooked;
            _player.ArrowHooked(this);
        }
    }

    private void DrawHookLine()
    {
        _hookLine?.QueueFree();
        _hookLine = Draw.Line3D(_player.GetParent(), GlobalPosition, _player.GlobalPosition, ColorHook);
    }

	public void Destroy()
	{
        _hookLine?.QueueFree();
        QueueFree();
	}
}
