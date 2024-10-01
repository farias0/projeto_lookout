using Godot;
using System;
using projeto_lookout.libs;

public partial class Explosion : Node3D
{
	[Export]
	public float Radius = 5;
	[Export]
	public float Duration = 2.5f;
	//[Export]
	//public int PlayerDamage = 0;
	[Export]
	public int EnemyDamage = 100;


	private Area3D _area;
	private ExplosionAudio _audio;
	private float _durationCountdown = -1;

	public override void _Ready()
	{
		_durationCountdown = Duration;

		_area = GetNode<Area3D>("Area3D");
		_area.Connect("body_entered", new(this, nameof(OnBodyEntered)));
		_area.Connect("area_entered", new(this, nameof(OnBodyEntered)));
		_area.Monitoring = true;
		(_area.GetNode<CollisionShape3D>("CollisionShape3D").Shape as SphereShape3D).Radius = Radius;

		_audio = GetNode<ExplosionAudio>("AudioStreamPlayer3D");
		_audio.PlayExplosion();

		// Particles
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
			//player.TakeDamage(PlayerDamage);
		}
		else if (body is Enemy enemy && LineOfSight(enemy, enemy.GetCentralPoint()))
		{
			enemy.TakeDamage(GlobalPosition, EnemyDamage);
		}
		else if (body is Npc npc && LineOfSight(npc, npc.GetCentralPoint()))
		{
			npc.TakeDamage(GlobalPosition, EnemyDamage);
		}
		else if (body is BreakableWall wall && LineOfSight(wall, wall.GetCentralPoint()))
		{
			wall.Break(GlobalPosition);
		}
		else if (body is Button button && LineOfSight(button, button.GlobalPosition))
		{
			button.Press();
		}
	}

	private bool LineOfSight(Node3D entity, Vector3 point)
	{
		// Offset it a little off the ground
		var origin = GlobalPosition + (Basis.Y * 0.3f);

		var result = Raycast.CastRay(GetWorld3D(), origin, point);
		var collider = result.ContainsKey("collider") ? (Node3D)result["collider"] : null;
		return collider != null && (collider == entity || collider.GetParent() == entity);
	}
}
