using Godot;
using System;
using projeto_lookout.libs;

public partial class Inventory : CanvasLayer
{
	[Export]
	public InventoryItem[] Items { get; set; }


	private ColorRect _bg;


	public bool IsEnabled()
	{
		return Visible;
	}

	public void Enable()
	{
		Visible = true;
		Input.MouseMode = Input.MouseModeEnum.Visible;
	}

	public void Disable()
	{
		Visible = false;
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	public override void _Ready()
	{
		Resources.Instance.Inventory = this;

		_bg = GetNode<ColorRect>("BG");

		Disable();
	}

	public override void _Input(InputEvent e)
	{
		if (e.IsActionPressed("toggle_inventory"))
			ToggleEnabled();


		if (IsEnabled())
		{
			//
		}
	}

	public void ToggleEnabled()
	{
		if (IsEnabled()) Disable();
		else Enable();
	}
}
