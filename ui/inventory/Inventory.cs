using Godot;
using System;
using projeto_lookout.libs;
using System.Collections.Generic;
using System.Linq;


// To be able to export it to Godot's Inspector
using ListInventoryItem = Godot.Collections.Array<InventoryItem>;

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
	public int ProtectedSlotsCount
	{
		get => _protectedSlots.Count;
		set => SetProtectedSlotsCount(value);
	}
	[Export]
	public ListInventoryItem HeldItems // Sadly, modifying this list via inspector at runtime doesnt work. Something to do with serialization of Inventoryitems. TODO fix this
	{
		get => GetHeldItems();
		set => SetHeldItems(value);
	}

	public ListInventoryItem ProtectedItems
	{
		get => new(_protectedSlots.Select(slot => slot.Item).ToList());
	}
	public int HealthPotionCount {
		get => HeldItems.Count(item => item.ID == "health_potion");
	}
	public int StaminaPotionCount
	{
		get => HeldItems.Count(item => item.ID == "stamina_potion");
	}
	public static bool DebugCellSquareEnabled { get; private set; } = false;
	public InventoryAudio Audio;
	public BowItemType BowItemEquipped
	{
		get => _bowSlot.Item?.BowItem ?? BowItemType.None;
	}


	private static readonly PackedScene CellScene = 
		(PackedScene)GD.Load("res://ui/inventory/items/cell/item_cell.tscn");
	private static readonly PackedScene ProtectedSlotScene = 
		(PackedScene)GD.Load("res://ui/inventory/protected_slot/protected_slot.tscn");


	private int _columns = 7;
	private int _rows = 5;
	private ItemCell[] _cells = Array.Empty<ItemCell>();
	private ColorRect _panel;
	private Control _grid;
	private List<ItemCell> _draggingItemCells = new(); // Keeps trach of which cells are occupied by an item that's being dragged
	private ColorRect _dropArea;
	private List<ProtectedSlot> _protectedSlots = new();
	private Control _protectedSlotsArea;
	private BowSlot _bowSlot;


	// Workaround for only adding the items to the grid after the cells are created
	private ListInventoryItem _delayedInitializationItems = null;



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

		Audio.PlayItemSelected();
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
		Audio.PlayOperationCancelled();
	}

	public override void _Ready()
	{
		Resources.Instance.Inventory = this;

		_panel = GetNode<ColorRect>("Panel");
		_grid = _panel.GetNode<Control>("Grid");
		_dropArea = _panel.GetNode<ColorRect>("DropArea");
		_protectedSlotsArea = _panel.GetNode<Control>("ProtectedSlotsArea");
		_bowSlot = _panel.GetNode<BowSlot>("BowSlot");
		Audio = GetNode<InventoryAudio>("AudioStreamPlayer");


		CreateCells();
		SetProtectedSlotsCount(ProtectedSlotsCount); // Call it again so it can created the slot Nodes


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

		if (e.IsActionPressed("debug_toggle_cell_squares"))
			DebugCellSquareEnabled = !DebugCellSquareEnabled;


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

	public bool AddNewItem(PackedScene scene)
	{
		var item = scene.Instantiate() as InventoryItem;
		AddChild(item);
		var success = AddItemToGrid(item);

		if (success)
		{
			HeldItems.Add(item);

			var player = Resources.Instance.Player;
			if (item.ID == "health_potion")
			{
				player.PickUpHealthPotion();
				Resources.Instance.HUD.SetHealthPotionAmount(HealthPotionCount);
			}
			else if (item.ID == "stamina_potion")
			{
				player.PickUpStaminaPotion();
				Resources.Instance.HUD.SetStaminaPotionAmount(StaminaPotionCount);
			}
		}
		else
		{
			Audio.PlayInventoryFull();
			item.QueueFree();
		}

		return success;
	}

	/// <summary>
	/// Use a Health Potion from the inventory
	/// </summary>
	/// <returns>If it there was a potion to be used</returns>
	public bool SpendHealthPotion()
	{
		var item = HeldItems.Where(item => item.ID == "health_potion").FirstOrDefault();
		if (item == null) return false;
		RemoveItem(item);
		Resources.Instance.HUD.SetHealthPotionAmount(HealthPotionCount);
		return true;
	}

	/// <summary>
	/// Use a Stamina Potion from the inventory
	/// </summary>
	/// <returns>If it there was a potion to be used</returns>
	public bool SpendStaminaPotion()
	{
		var item = HeldItems.Where(item => item.ID == "stamina_potion").FirstOrDefault();
		if (item == null) return false;
		RemoveItem(item);
		Resources.Instance.HUD.SetStaminaPotionAmount(StaminaPotionCount);
		return true;
	}

	/// <summary>
	/// Removes all unprotected items from the inventory
	/// </summary>
	public void LoseAllUnprotectedItems()
	{
		var pItems = ProtectedItems;

		foreach (var item in HeldItems)
		{
			if (!pItems.Contains(item))
				RemoveItem(item);
		}

		RefreshHUD();
	}

	private bool AddItemToGrid(InventoryItem item)
	{
		var success = false;

		// Find an empty cell
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

		return success;
	}

	private void RemoveItem(InventoryItem item)
	{
		foreach (var cell in _cells)
		{
			if (cell.Item == item) cell.Item = null;
		}

		RemoveItemFromProtected(item);
		if (item == _bowSlot.Item) _bowSlot.Item = null;

		item.QueueFree();

		return;
	}

	private void RemoveItemFromProtected(InventoryItem item)
	{
		foreach (var slot in _protectedSlots)
		{
			if (slot.Item == item) slot.Item = null;
		}
	}

	private void UpdateGridSize()
	{
		ListInventoryItem heldItems = HeldItems;

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

	private ListInventoryItem GetHeldItems()
	{
		if (_delayedInitializationItems != null)
			return _delayedInitializationItems;
		else
			return new ListInventoryItem( 
				_cells.Select(cell => cell.Item).Distinct().Where(item => item != null).ToList());
	}

	private void SetHeldItems(ListInventoryItem items)
	{
		if (!IsNodeReady())
		{
			_delayedInitializationItems = items;
			return;
		}


		foreach (var item in items)
		{
			if (!AddItemToGrid(item))
				throw new InvalidOperationException($"Could not add item {item.Name} to the inventory.");
		}

		RefreshHUD();
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
			// Check against drop area
			if (itemCell.GetCollisionRect().Intersects(_dropArea.GetGlobalRect()))
			{
				DropItemToWorld(item);
				_draggingItemCells.Clear();
				return true;
			}

			// Check against protected item slots
			foreach (var pSlot in _protectedSlots)
			{
				if (itemCell.GetCollisionRect().Intersects(pSlot.GetGlobalRect()))
				{
					RemoveItemFromProtected(item);
					pSlot.SetItem(item);
					StopDraggingItem(item);
					return true;
				}
			}
			// Check against bow slot
			if (item.IsBowItem && itemCell.GetCollisionRect().Intersects(_bowSlot.GetGlobalRect()))
			{
				_bowSlot.SetItem(item);
				StopDraggingItem(item);
				RefreshHUD();
				return true;
			}

			// Check against grid cells
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

		if (IsEnabled())
			Audio.PlayItemSetInPlace();

		return true;
	}


	private void DropItemToWorld(InventoryItem item)
	{
		Resources.Instance.Player.SpawnItem(item.SpawnsItem);
		RemoveItem(item);
		RefreshHUD();
	}

	/// <summary>
	/// Recreates the protected slots
	/// </summary>
	/// <param name="count">The number of slots</param>
	private void SetProtectedSlotsCount(int count)
	{
		_protectedSlots = new();
		for (int i = 0; i < count; i++)
		{
			ProtectedSlot slot = (ProtectedSlot)ProtectedSlotScene.Instantiate();
			slot.Position = new Vector2(0, i * (slot.GetRect().Size.Y + 40));
			_protectedSlotsArea?.AddChild(slot);
			_protectedSlots.Add(slot);
		}
	}

	private void StopDraggingItem(InventoryItem item)
	{
		if (_draggingItemCells.Count == 0)
			throw new InvalidOperationException("Not dragging item.");

		_draggingItemCells.ForEach(cell =>
		{
			if (cell.Item != null)
				throw new InvalidOperationException("Cell already occupied");

			cell.Item = item;
		});
		_draggingItemCells.Clear();

		item.ResetDraggingPosition();
	}

	private void RefreshHUD() {
		Resources.Instance.HUD.SetHealthPotionAmount(HealthPotionCount);
		Resources.Instance.HUD.SetStaminaPotionAmount(StaminaPotionCount);
		Resources.Instance.HUD.SetBowItemIcon(_bowSlot.Item?.TextureNormal);
	}
}
