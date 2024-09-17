using Godot;
using System;
using projeto_lookout.libs;

public partial class HealthPotion : PickUp
{
	public override void OnPlayerPickup(Player player)
	{
		player.PickUpHealthPotion();
	}
}
