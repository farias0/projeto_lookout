using Godot;
using projeto_lookout.libs;

public partial class Sell : Resource
{
	[Export(PropertyHint.File)]
	public string InventoryItem // Resources can't export types derived from Node
	{
		get => _inventoryItem.ResourcePath;
		set { if (value != "") _inventoryItem = GD.Load<PackedScene>(value); }
	}
	[Export]
	public Texture2D Icon { get; set; }
	[Export]
	public string ItemName { get; set; }
	[Export]
	public int Price { get; set; }

	public bool IsSold { get; private set; } = false;

	private PackedScene _inventoryItem;

	
	public bool SellItem()
	{
		if (IsSold)
		{
			// TODO play error sound
			return false;
		}

		var inventory = Resources.Instance.Inventory;
		var result = inventory.SubtractGold(Price);

		if (result)
		{
			inventory.AddNewItem(_inventoryItem);
			// TODO play sold sound
		}
		else
		{
			Debug.Log($"{ItemName}: Not enough funds.");
			// TODO play error sound
		}

		return result;
	}
}
