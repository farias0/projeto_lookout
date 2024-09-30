using Godot;
using System;
using projeto_lookout.libs;

public partial class Explosion : Node3D
{
	[Export]
	public float Radius = 5;
	[Export]
	public float Duration = 2.5f;
	[Export]
	public int PlayerDamage = 60;
	[Export]
	public int EnemyDamage = 100;


	private Area3D _area;
	private float _durationCountdown = -1;

	public override void _Ready()
	{
		_durationCountdown = Duration;

		_area = GetNode<Area3D>("Area3D");
		_area.Connect("body_entered", new(this, nameof(OnBodyEntered)));
		_area.Connect("area_entered", new(this, nameof(OnBodyEntered)));
		_area.Monitoring = true;
		(_area.GetNode<CollisionShape3D>("CollisionShape3D").Shape as SphereShape3D).Radius = Radius;

		// Particles
		// Play sound effect
		// Screen shake?
	}

	public override void _Process(double delta)
	{
		_durationCountdown -= (float)delta;
		if (_durationCountdown <= 0)
		{
			QueueFree();
		}

		// Blink effect
		GetNode<MeshInstance3D>("MeshInstance3D").Visible = _durationCountdown % 0.2f > 0.1f;
	}

	private void OnBodyEntered(Node body)
	{
		if (body is Player player && LineOfSight(player, player.GlobalPosition))
		{
			player.TakeDamage(PlayerDamage);
		}
		else if (body is Enemy enemy && LineOfSight(enemy, enemy.GetCentralPoint()))
		{
			enemy.TakeDamage(GlobalPosition, EnemyDamage);
		}
		else if (body is Npc npc && LineOfSight(npc, npc.GetCentralPoint()))
		{
			npc.TakeDamage(GlobalPosition, EnemyDamage);
		}
		// TODO buttons
		// TODO walls
	}

	private bool LineOfSight(Node3D entity, Vector3 point)
	{
		var result = Raycast.CastRay(GetWorld3D(), GlobalPosition, point);
		return result.ContainsKey("collider") && (Node)result["collider"] == entity;
	}
}
