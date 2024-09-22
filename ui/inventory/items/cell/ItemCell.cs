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


	public override void _Input(InputEvent @event)
	{
		if (!Resources.Instance.Inventory.IsEnabled()) return;

		if (Type == CellType.InventoryCell)
		{
			// Inventory cells don't have any interactivity
			return;
		}


		if (@event is InputEventMouseButton mouseEvent)
		{
			if (GetRotatedRect().HasPoint(mouseEvent.Position))
			{
				Item.OnClick(mouseEvent);
			}
		}
		else if (@event is InputEventMouseMotion motionEvent)
		{
			Item.OnMove(motionEvent);
		}
	}

	/// <summary>
	/// This cell's collision rectangle, considering the item's rotation.
	/// Presumes the rotations are always at 90 degree intervals.
	/// </summary>
	private Rect2 GetRotatedRect()
	{
		Vector2 pos = GlobalPosition;
		var rot = Item.RotationDegrees;

		if (rot == 0)
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
			throw new InvalidOperationException($"Invalid rotation {rot}.");

		return new Rect2(pos, Size * Scale);
	}
}
