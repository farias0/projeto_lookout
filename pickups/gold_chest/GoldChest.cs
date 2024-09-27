using Godot;
using System;

public partial class GoldChest : PickUp
{
	[Export]
	public int GoldAmount { get; set; } = 100;

	public override void OnPlayerPickup(Player player)
	{
		base.OnPlayerPickup(player);

		player.PickUpGold(GoldAmount);
	}
}
