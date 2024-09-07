using Godot;
using projeto_lookout.libs;
using System;

public partial class Enemy : Node3D
{
    [Export]
	public int Health { get; set; } = 100;
    [Export]
    public float Speed { get; set; } = 1.8f;
    [Export]
    public float VisionDistance { get; set; } = 20;
    [Export]
    public float VisionAngle { get; set; } = 55;


    private CharacterBody3D _player;
    private MeshInstance3D _mesh;
    private NavigationAgent3D _navAgent;
    
    private Vector2 _targetPosition;
    private float _tookDamageCountdown = -1;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        _player = Resources.PlayerRef;
        if (_player == null)
        {
            Debug.LogError("Enemy couldn't find player.");
        }

        _mesh = FindChild("Node0").GetChild<MeshInstance3D>(0);
        if (_mesh == null)
        {
            Debug.LogError("Couldn't find enemy's mesh.");
        }

        _navAgent = FindChild("NavigationAgent3D") as NavigationAgent3D;
        if (_navAgent == null)
        {
            Debug.LogError("Couldn't find enemy's navigation agent.");
        }


        SetTarget(_player.GlobalPosition);
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        ProcessDamageCountdown((float)delta);

        if (SeesPlayer())
        {
            FollowPlayer();
        }
        else
        {
            StopInPlace();
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        MoveTowardsTarget((float)delta);
    }

    public void TakeDamage(int damage)
	{
        if (_tookDamageCountdown > 0)
        {
            return;
        }

        if (Health <= 0)
        {
            Debug.LogError("Dead enemy took damage.");
            return;
        }

        Health -= damage;
		_tookDamageCountdown = 2;
		Debug.Log($"Enemy took damage. Health: {Health}");

        if (Health <= 0)
        {
            Die();
        }
    }

    private void ProcessDamageCountdown(float delta)
    {
        if (_tookDamageCountdown <= 0) return;

        _tookDamageCountdown -= delta;

        // Blink effect
        _mesh.Visible = _tookDamageCountdown % 0.2f > 0.1f;

        if (_tookDamageCountdown <= 0)
        {
            _tookDamageCountdown = -1;

            // Stop blinking
            _mesh.Visible = true;
        }
    }

    private void Die()
    {
        QueueFree();
    }

    private void SetTarget(Vector3 pos)
    {
        _navAgent.TargetPosition = pos;
    }

    private void MoveTowardsTarget(float delta)
    {
        Vector3 pos = GlobalPosition;
        Vector3 targetPos = _navAgent.GetNextPathPosition();

        if (pos.DistanceTo(targetPos) < 1.0f)
        {
            return;
        }

        TurnTowards(targetPos);

        pos.Y = 0;
        targetPos.Y = 0;
        GlobalPosition += (targetPos - pos).Normalized() * Speed * delta;
    }

    private void TurnTowards(Vector3 direction)
    {
        direction.Y = GlobalPosition.Y;
        LookAt(direction);
    }

    private void StopInPlace()
    {
        SetTarget(GlobalPosition);
    }

    private void FollowPlayer()
    {
        SetTarget(_player.GlobalPosition);
    }

    private bool SeesPlayer()
    {
        {   // Is within distance
            float distance = GlobalPosition.DistanceTo(_player.GlobalPosition);
            if (distance > VisionDistance)
            {
                return false;
            }
        }

        {   // Is within the vision cone
            Vector3 direction = (_player.GlobalPosition - GlobalPosition).Normalized();
            float angle = GlobalTransform.Basis.Z.AngleTo(direction * -1);
            if (angle > Mathf.DegToRad(VisionAngle))
            {
                return false;
            }
        }

        { // Has no obstacles between them
            Vector3 heightOffset = new(0, 1.3f, 0);
            Vector3 origin = GlobalPosition + heightOffset;
            Vector3 target = _player.GlobalPosition + heightOffset;

            var rayParams = new PhysicsRayQueryParameters3D
            {
                From = origin,
                To = target,
                CollideWithBodies = true,
                CollideWithAreas = true
            };
            var rayResult = GetWorld3D().DirectSpaceState.IntersectRay(rayParams);

            if ((Node)rayResult["collider"] != _player)
            {
                return false;
            }
        }

        return true;
    }
}
