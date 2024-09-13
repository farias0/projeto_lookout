using Godot;
using System;
using projeto_lookout.libs;

public partial class Camera : Camera3D
{
	public const float Sensitivity = 0.07f;

	private Vector3 _startingRotation;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Resources.Camera = this;
		_startingRotation = Rotation;
        Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion eventMouseMotion)
		{

			Vector2 move = eventMouseMotion.ScreenRelative;

			// Pitches camera up and down
			RotateX(Mathf.DegToRad(-move.Y * Sensitivity));

			Input.WarpMouse(	// Center mouse
				GetViewport().GetVisibleRect().Size / 2f);
		}
	}

	public void Reset()
	{
		Rotation = _startingRotation;
	}
}
