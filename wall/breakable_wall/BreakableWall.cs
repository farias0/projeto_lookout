using Godot;
using System;

public partial class BreakableWall : Area3D
{
	[Export]
	public int DurationAfterExplosion { get; set; } = 5;
	[Export]
	public float ChunkExplosionForce { get; set; } = 20;


	private float _durationCountdown = -1;
	private BoxShape3D _collisionShape;


	public override void _Ready()
	{
		_collisionShape = GetNode<CollisionShape3D>("CollisionShape3D").Shape as BoxShape3D;

		SetChunksEnabled(false);
	}

	public override void _Process(double delta)
	{
		if (_durationCountdown > 0)
		{
			_durationCountdown -= (float)delta;
			if (_durationCountdown <= 0)
			{
				QueueFree();
			}
		}
	}

	public Vector3 GetCentralPoint()
	{
		return GlobalPosition + (Basis.Y * _collisionShape.Size.Y * Scale.Y * 0.5f);
	}

	public void Break(Vector3 explosionSource)
	{
		_durationCountdown = DurationAfterExplosion;
		SetChunksEnabled(true);
		ExplodeChunks((GlobalPosition - explosionSource).Normalized());
		// TODO play sound
	}

	private void SetChunksEnabled(bool enabled)
	{
		foreach (var child in GetChildren())
		{
			if (child is RigidBody3D rb)
			{
				rb.Freeze = !enabled;
			}
		}
	}

	private void ExplodeChunks(Vector3 direction)
	{
		foreach (var child in GetChildren())
		{
			if (child is RigidBody3D rb)
			{
				rb.ApplyImpulse(direction * ChunkExplosionForce);
			}
		}
	}
}
