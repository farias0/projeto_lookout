using Godot;
using projeto_lookout.libs;
using System;

public partial class Enemy : Node3D
{
	[Export]
	public int Health { get; set; } = 100;


	private float _tookDamageCountdown = -1;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        if (_tookDamageCountdown > 0)
        {
            _tookDamageCountdown -= (float)delta;
            BlinkDamage();
            if (_tookDamageCountdown <= 0)
            {
                _tookDamageCountdown = -1;
            }
        }
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
            Debug.Log("Enemy died."); // TODO die
        }
    }

    private void BlinkDamage()
    {

    }
}
