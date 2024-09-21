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
	/// Like GetRect(), but compensates for the rotation of the item.
	/// Presumes the rotations are always at 90 degree intervals.
	/// </summary>
	private Rect2 GetRotatedRect()
	{
		Rect2 rect = new()
		{
			Position = GlobalPosition
		};
		var rot = Item.RotationDegrees; // Attention: Uses the Item rotation


		if (rot == 0 || rot == 180)
		{
			rect.Size = Size * Scale;
		}
		else if (rot == 90 || rot == 270)
		{
			rect.Size = new Vector2(Size.Y, Size.X) * Scale;
		}
		else
			throw new InvalidOperationException($"Invalid rotation {rot}.");

		Debug.Log($"{Name} rect: {rect}");
		return rect;
	}
}
