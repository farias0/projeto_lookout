using Godot;
using System;

public partial class PatrolPoint : Node3D
{
    public Vector3 Pos { get; set; } = Vector3.Zero;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		Pos = GlobalPosition;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
