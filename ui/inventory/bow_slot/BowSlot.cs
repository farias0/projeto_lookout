using Godot;
using System;
using projeto_lookout.libs;

public partial class BowSlot : TextureButton
{
	public InventoryItem Item
	{
		get => _item;
		set => SetItem(value);
	}

	private InventoryItem _item;
	private TextureRect _itemIcon;


	public void SetItem(InventoryItem item)
	{
		if (item != null && !item.IsBowItem)
			throw new InvalidOperationException("Only bow items can be set in the bow slot.");

		ClearItem();

		_item = item;

		if (_item != null)
		{
			_itemIcon = new()
			{
				Texture = _item.TextureNormal,
				ExpandMode = TextureRect.ExpandModeEnum.FitWidthProportional,
				Size = Size
			};
			AddChild(_itemIcon);
			// Resources.Instance.Inventory.Audio.PlayItemSetInPlace();
		}
	}

	public override void _GuiInput(InputEvent @event)
	{
		if (!Resources.Instance.Inventory.IsEnabled()) return;


		if (@event is InputEventMouseButton mouseEvent)
		{
			if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
			{
				ClearItem();
			}
		}
	}

	private void ClearItem()
	{
		if (_itemIcon != null)
		{
			RemoveChild(_itemIcon);
			_itemIcon = null;
			Resources.Instance.Inventory.Audio.PlayOperationCancelled();
		}

		_item = null;
	}
}
