using Godot;
using System;
using projeto_lookout.libs;

public partial class ProtectedSlot : TextureButton
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
