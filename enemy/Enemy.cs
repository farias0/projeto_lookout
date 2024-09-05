using Godot;
using projeto_lookout.libs;
using System;

public partial class Enemy : Node3D
{
	[Export]
	public int Health { get; set; } = 100;


    private MeshInstance3D _mesh;
    
    private float _tookDamageCountdown = -1;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        _mesh = GetChild<Node3D>(0).GetChild<MeshInstance3D>(0);

        if (_mesh == null)
        {
            Debug.LogError("Couldn't find enemy's mesh.");
        }
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        ProcessDamageCountdown((float)delta);
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
}
