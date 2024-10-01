using Godot;
using System;
using projeto_lookout.libs;

public partial class Button : Area3D
{
	[Export]
	public float ButtonFinalZ { get; set; } = -0.35f;
	[Export]
	public float ButtonSlideSpeed { get; set; } = 0.1f;

	private Node3D _button;
	private bool _isSliding = false;

	public override void _Ready()
	{
		_button = GetNode<Node3D>("Button");
	}

	public override void _Process(double delta)
	{
		ProcessSlide((float)delta);
	}

	public void Press()
	{
		_isSliding = true;
		Debug.Log("Button pressed.");
	}

	private void ProcessSlide(float delta)
	{
		if (!_isSliding) return;

		_button.Position -= new Vector3(0, 0, ButtonSlideSpeed * delta);

		if (_button.Position.Z <= ButtonFinalZ)
			_isSliding = false;
	}
}
