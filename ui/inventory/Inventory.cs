using Godot;
using System;

public partial class Inventory : CanvasLayer
{
	[Export]
	public InventoryItem[] Items { get; set; }
}
