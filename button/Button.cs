using Godot;
using System;
using projeto_lookout.libs;


public interface IButtonActionable
{
	void ButtonActivate();
}

public partial class Button : Area3D
{
	[Export]
	public float ButtonFinalZ { get; set; } = -0.35f;
	[Export]
	public float ButtonSlideSpeed { get; set; } = 0.1f;
	[Export]
	public Node3D ActivatesEntity
	{
		get => _activatesEntity as Node3D;
		set
		{
			if (value is not IButtonActionable)
				throw new Exception("Entity must implement IButtonActionable");

			_activatesEntity = value as IButtonActionable;
		}
	}


	private Node3D _button;
	private bool _isSliding = false;
	private IButtonActionable _activatesEntity;


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
		_activatesEntity?.ButtonActivate();
	}

	private void ProcessSlide(float delta)
	{
		if (!_isSliding) return;

		_button.Position -= new Vector3(0, 0, ButtonSlideSpeed * delta);

		if (_button.Position.Z <= ButtonFinalZ)
			_isSliding = false;
	}
}
