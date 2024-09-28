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


	public void SetItem(InventoryItem item)
	{
		_item = item;

		if (_item == null)
		{
			// TODO clear image
		}
		else
		{
			// TODO add item image
		}

		Debug.Log($"Setting item {item.Name} to protected.");
	}
}
