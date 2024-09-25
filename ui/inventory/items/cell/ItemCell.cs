using Godot;
using projeto_lookout.libs;
using System;


public enum CellType
{
	ItemCell,       // Belongs to an item, used to drag and drop
	InventoryCell   // Belongs to the inventory grid, used to store items
}

public partial class ItemCell : TextureButton
{
	public CellType Type { get; set; }
	public InventoryItem Item { get; set; }


	private Square _debugSquare;


	public override void _Input(InputEvent @event)
	{
		if (!Resources.Instance.Inventory.IsEnabled()) return;

		if (Type == CellType.InventoryCell)
		{
			// Inventory cells don't have any interactivity
			return;
		}

		if (@event is InputEventMouseMotion motionEvent)
		{
				Item.OnMove(motionEvent);
			}
		}

	public override void _GuiInput(InputEvent @event)
	{
		if (!Resources.Instance.Inventory.IsEnabled()) return;

		if (Type == CellType.InventoryCell)
		{
			// Inventory cells don't have any interactivity
			return;
		}


		if (@event is InputEventMouseButton mouseEvent)
		{
			Item.OnClick(this, mouseEvent);
		}
	}

	public override void _Process(double a)
	{
		// if (Type != CellType.ItemCell) return; 

		if (Inventory.DebugCellSquareEnabled) UpdateDebugSquare();
		else
		{
			_debugSquare?.QueueFree();
			_debugSquare = null;
		}
	}

	/// <summary>
	/// This cell's GlobalPosition, considering the item's rotation.
	/// Presumes the rotations are always at 90 degree intervals.
	/// </summary>
	public Vector2 GetPos()
	{
		Vector2 pos = GlobalPosition;

		if (Item != null)
		{
			var rot = Item.RotationDegrees;

			if (rot == 0 || rot == 360)
			{
				//
			}
			else if (rot == 90)
			{
				pos.X -= Size.Y * Scale.Y;
			}
			else if (rot == 180)
			{
				pos.X -= Size.X * Scale.X;
				pos.Y -= Size.Y * Scale.Y;
			}
			else if (rot == 270)
			{
				pos.Y -= Size.Y * Scale.Y;
			}
			else
				throw new InvalidOperationException($"Invalid Item rotation {rot}.");
		}

		return pos;
	}

	/// <summary>
	/// This cell's collision rectangle
	/// </summary>
	public Rect2 GetCollisionRect()
	{
		return new Rect2(GetPos(), Size * Scale);
	}

	private void UpdateDebugSquare()
	{
		if (Resources.Instance.Inventory == null || !Resources.Instance.Inventory.IsNodeReady())
			return;

		_debugSquare ??= new Square();
		_debugSquare?.QueueFree();
		_debugSquare.DrawSquare(Resources.Instance.Inventory, GetCollisionRect());
	}
}
