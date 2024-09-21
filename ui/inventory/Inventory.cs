using Godot;
using System;
using projeto_lookout.libs;

public partial class Inventory : Control
{
	[Export]
	public InventoryItem[] Items { get; set; }


	private static readonly PackedScene CellScene = (PackedScene)GD.Load("res://ui/inventory/items/item_cell.tscn");
	private const int Rows = 5;
	private const int Columns = 7;


	private ColorRect _panel;
	private Control _grid;


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

		_panel = GetNode<ColorRect>("Panel");
		_grid = _panel.GetNode<Control>("Grid");


		PopulateCells();


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

	private void PopulateCells()
	{
		for (int i = 0; i < Rows; i++)
		{
			for (int j = 0; j < Columns; j++)
			{
				TextureRect cell = (TextureRect)CellScene.Instantiate();
				cell.Position = new Vector2(j * cell.Size.X, i * cell.Size.Y);
				_grid.AddChild(cell);
			}
		}
	}
}
