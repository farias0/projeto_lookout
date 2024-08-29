using Godot;
using System;
using System.Diagnostics;

public partial class Arrow : Node3D
{
	[Export]
	public float Speed { get; set; } = 15;

	private float _lifeTime = 5;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Position -= Transform.Basis.X * Speed * (float)delta;

		_lifeTime -= (float) delta;
		if (_lifeTime <= 0)
		{
			Destroy();
		}
	}

	private void Destroy()
	{
		GetParent().QueueFree();
	}
}
