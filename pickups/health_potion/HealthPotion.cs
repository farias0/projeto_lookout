using Godot;
using System;
using projeto_lookout.libs;

public partial class HealthPotion : PickUp
{
	public static readonly int HealAmount = 60;

	public override void OnPlayerPickup(Player player)
	{
		player.PickUpHealthPotion();
	}
}
