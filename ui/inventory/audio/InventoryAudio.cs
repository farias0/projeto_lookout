using Godot;
using System;

public partial class InventoryAudio : SoundPlayer
{
	[ExportGroup("Mixer")]
	[Export(PropertyHint.Range, "-80, 24")]
	private float OperationCancelled
	{
		get => _operationCancelled.Volume;
		set => SetSoundVolume(_operationCancelled, value);
	}

	//private readonly Sound _inventoryOpened = new();
	//private readonly Sound _itemSelected = new();
	//private readonly Sound _itemRotated = new();
	//private readonly Sound _itemSetInPlace = new();
	//private readonly Sound _itemDropped = new();
	private readonly Sound _operationCancelled = new();
	//private readonly Sound _inventoryFull = new();

	public override void _Ready()
	{
		// _itemSelected.LoadStream("res://ui/inventory/audio/item_selected.wav");

		//_itemRotated.LoadStream("res://ui/inventory/audio/item_rotated.wav");

		//_itemSetInPlace.LoadStream("res://ui/inventory/audio/item_set_in_place.wav");

		//_itemDropped.LoadStream("res://ui/inventory/audio/item_dropped.wav");

		_operationCancelled.LoadStream("res://ui/inventory/audio/operation_cancelled.wav");

		//_inventoryFull.LoadStream("res://ui/inventory/audio/inventory_full.wav");
	}

	public void PlayOperationCancelled()
	{
		PlaySound(_operationCancelled);
	}
}
