using Godot;
using System;
using projeto_lookout.libs;
using System.Collections.Generic;
using System.Linq;


public partial class Inventory : Control
{
	[Export]
	public int Columns { 
		get => _columns; 
		set
		{
			_columns = value;
			UpdateGridSize();
		}
	}
	[Export]
	public int Rows
	{
		get => _rows;
		set
		{
			_rows = value;
			UpdateGridSize();
		}
	}
	[Export]
	public InventoryItem[] HeldItems // Sadly, modifying this list via inspector at runtime doesnt work. Something to do with serialization of Inventoryitems. TODO fix this
	{
		get => GetHeldItems();
		set => SetHeldItems(value);
	}


	private static readonly PackedScene CellScene = (PackedScene)GD.Load("res://ui/inventory/items/cell/item_cell.tscn");


	private int _columns = 7;
	private int _rows = 5;
	private ItemCell[] _cells = Array.Empty<ItemCell>();
	private ColorRect _panel;
	private Control _grid;
	private List<ItemCell> _draggingItemCells = new(); // Keeps trach of which cells are occupied by an item that's being dragged


	// Workaround for only adding the items to the grid after the cells are created
	private InventoryItem[] _delayedInitializationItems = null;



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

	public void StartDraggingItem(InventoryItem item)
	{
		if (_draggingItemCells.Count != 0)
			throw new InvalidOperationException("Cells for a dragging item present");

		_draggingItemCells.Clear();

		foreach (var cell in _cells)
		{
			if (cell.Item == item)
			{
				cell.Item = null;
				_draggingItemCells.Add(cell);
			}
		}

		if (_draggingItemCells.Count == 0)
			throw new InvalidOperationException("Couldn't find item in inventory");
	}

	public void CancelDraggingItem(InventoryItem item)
	{
		foreach (var cell in _draggingItemCells)
		{
			if (cell.Item != null)
				throw new InvalidOperationException($"Cell {cell.Name} already occupied");

			cell.Item = item;
		}

		_draggingItemCells.Clear();
	}

	public override void _Ready()
	{
		Resources.Instance.Inventory = this;

		_panel = GetNode<ColorRect>("Panel");
		_grid = _panel.GetNode<Control>("Grid");


		CreateCells();


		if (_delayedInitializationItems != null)
		{
			HeldItems = _delayedInitializationItems;
			_delayedInitializationItems = null;
		}


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

	private void UpdateGridSize()
	{
		InventoryItem[] heldItems = HeldItems;

		if (IsNodeReady())
		{
			DestroyCells();
			CreateCells();
		}

		HeldItems = heldItems;
	}

	private void CreateCells()
	{
		_cells = new ItemCell[Columns * Rows];

		for (int i = 0; i < Rows; i++)
		{
			for (int j = 0; j < Columns; j++)
			{
				ItemCell cell = (ItemCell)CellScene.Instantiate();
				cell.Type = CellType.InventoryCell;
				cell.Position = new Vector2(j * cell.Size.X, i * cell.Size.Y);
				_grid.AddChild(cell);
				_cells[(i * Columns) + j] = cell;
			}
		}
	}

	private void DestroyCells()
	{
		foreach (var cell in _cells)
		{
			cell.QueueFree();
		}
		_cells = Array.Empty<ItemCell>();
	}

	private InventoryItem[] GetHeldItems()
	{
		if (_delayedInitializationItems != null)
			return _delayedInitializationItems;
		else
			return _cells.Select(cell => cell.Item).Distinct().Where(item => item != null).ToArray();
	}

	private void SetHeldItems(InventoryItem[] items)
	{
		if (!IsNodeReady())
		{
			_delayedInitializationItems = items;
			return;
		}


		foreach (var item in items)
		{
			var success = false;

			// Find an emtpy cell
			foreach (var cell in _cells)
			{
				if (cell.Item != null) continue;

				// Attempt drag
				var pos = item.GlobalPosition;
				item.Position = cell.GetGlobalPosition();
				
				if (AttemptItemDrag(item)) // TODO AttemptItemDrag is for mouse! Fix this hack
				{
					success = true;
					break;
				}
				else item.Position = pos; // Fail
			}

			if (!success)
				throw new InvalidOperationException($"Could not add item {item.Name} to the inventory.");
		}
	}

	/// <summary>
	/// Attempts to drag an item from the inventory based on its new
	/// position, and snaps it to the grid if successful.
	/// </summary>
	/// <param name="item">Item that's been dragged</param>
	/// <returns>If the item was successfully dragged</returns>
	public bool AttemptItemDrag(InventoryItem item)
	{

		// Looks for the attempted slots
		bool foundSlot = false;
		Vector2 offset = Vector2.Zero;
		foreach (var itemCell in item.Cells)
		{
			foreach (var gridCell in _cells)
			{
				var dist = itemCell.GetPos().DistanceTo(gridCell.GetPos());
				if (dist <= gridCell.Size.X / 2)
				{
					foundSlot = true;
					offset = gridCell.GetPos() - itemCell.GetPos();
					break;
				}
				if (foundSlot) break;
			}
		}
		if (!foundSlot) return false;


		var desiredCells = new List<ItemCell>();

		// Checks if every cell has the corresponding slot available
		foreach (var itemCell in item.Cells)
		{
			bool found = false;
			foreach (var gridCell in _cells)
			{
				var dist = itemCell.GetPos().DistanceTo(gridCell.GetPos());
				if (dist <= gridCell.Size.X / 2 && gridCell.Item == null)
				{
					found = true;
					desiredCells.Add(gridCell);
					break;
				}
			}
			if (!found) return false;
		}


		//		Move is successfull!!

		_draggingItemCells.Clear();

		// Marks new cells as occupied
		foreach (var cell in desiredCells)
		{
			cell.Item = item;
		}

		// Snaps to the new position
		item.Position += offset;

		return true;
	}
}
