using Godot;
using System;
using projeto_lookout.libs;
using System.Collections.Generic;

public class InventoryCell
{
	public TextureRect Cell { get; set; }
	public InventoryItem HeldItem { get; set; }

}

public partial class Inventory : Control
{
	[Export]
	public InventoryItem[] Items { get; set; }


	private static readonly PackedScene CellScene = (PackedScene)GD.Load("res://ui/inventory/items/item_cell.tscn");
	private const int Rows = 5;
	private const int Columns = 7;


	private ColorRect _panel;
	private Control _grid;
	private List<InventoryCell> _cells = new();


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

	/// <summary>
	/// Attempts to drag an item from the inventory based on its new
	/// position, and snaps it to the new cells if possible.
	/// </summary>
	/// <param name="item">Item that's been dragged</param>
	/// <returns>If the item was successfully dragged</returns>
	public bool AttemptItemDrag(InventoryItem item)
	{
		// Checks if it's in list
		bool inList = false;
		foreach (var i in Items) if (i == item) inList = true;
		if (!inList) throw new InvalidOperationException("Item not in inventory.");


		// Looks for the attempted slots
		bool foundSlot = false;
		Vector2 offset = Vector2.Zero;
		foreach (var itemCell in item.Cells)
		{
			foreach (var cell in _cells)
			{
				var dist = itemCell.GetGlobalPosition().DistanceTo(cell.Cell.GetGlobalPosition());
				if (dist < cell.Cell.Size.X / 2)
				{
					foundSlot = true;
					offset = cell.Cell.GetGlobalPosition() - itemCell.GetGlobalPosition();
				}
			}
		}
		if (!foundSlot) return false;


		// Checks if every cell has a slot
		foreach (var itemCell in item.Cells)
		{
			bool found = false;
			foreach (var cell in _cells)
			{
				var dist = itemCell.GetGlobalPosition().DistanceTo(cell.Cell.GetGlobalPosition());
				if (dist < cell.Cell.Size.X / 2)
				{
					found = true;
				}
			}
			if (!found) return false;
		}

		// Snaps to the new position
		item.Position += offset;

		return true;
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
				_cells.Add(new InventoryCell()
				{
					Cell = cell,
					HeldItem = null
				});
			}
		}
	}
}
