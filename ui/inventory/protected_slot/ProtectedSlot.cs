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
		_item = item;

		if (_itemIcon != null)
		{
			RemoveChild(_itemIcon);
		}

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
}
