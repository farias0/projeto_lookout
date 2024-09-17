using Godot;
using System;
using projeto_lookout.libs;

public partial class StaminaPotion : PickUp
{
	public static readonly int FillAmount = 100;

	public override void OnPlayerPickup(Player player)
	{
		player.PickUpStaminaPotion();
	}
}
