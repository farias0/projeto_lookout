using Godot;
using System;
using projeto_lookout.libs;

public partial class GoldChest : PickUp
{
	[Export]
	public int GoldAmount { get; set; } = 100;

	public override void OnPlayerPickup(Player player)
	{
		base.OnPlayerPickup(player);

		Resources.Instance.Inventory.PickUpGold(GoldAmount);
	}
}
