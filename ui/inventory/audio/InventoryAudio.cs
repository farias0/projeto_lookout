using Godot;
using System;

public partial class InventoryAudio : SoundPlayer
{
	[ExportGroup("Mixer")]
	[Export(PropertyHint.Range, "-80, 24")]
	private float ItemSelected
	{
		get => _itemSelected.Volume;
		set => SetSoundVolume(_itemSelected, value);
	}
	[Export(PropertyHint.Range, "-80, 24")]
	private float ItemRotated
	{
		get => _itemRotated.Volume;
		set => SetSoundVolume(_itemRotated, value);
	}
	[Export(PropertyHint.Range, "-80, 24")]
	private float ItemSetInPlace
	{
		get => _itemSetInPlace.Volume;
		set => SetSoundVolume(_itemSetInPlace, value);
	}
	//[Export(PropertyHint.Range, "-80, 24")]
	//private float OperationCancelled
	//{
	//	get => _operationCancelled.Volume;
	//	set => SetSoundVolume(_operationCancelled, value);
	//}
	[Export(PropertyHint.Range, "-80, 24")]
	private float InventoryFull
	{
		get => _inventoryFull.Volume;
		set => SetSoundVolume(_inventoryFull, value);
	}
	[Export(PropertyHint.Range, "-80, 24")]
	private float CollectPotion
	{
		get => _collectPotion.Volume;
		set => SetSoundVolume(_collectPotion, value);
	}
	[Export(PropertyHint.Range, "-80, 24, or_less")]
	private float CollectGold
	{
		get => _collectGold.Volume;
		set => SetSoundVolume(_collectGold, value);
	}


	//private readonly Sound _inventoryOpened = new();
	private readonly Sound _itemSelected = new();
	private readonly Sound _itemRotated = new();
	private readonly Sound _itemSetInPlace = new();
	//private readonly Sound _itemDropped = new();
	// private readonly Sound _operationCancelled = new();
	private readonly Sound _inventoryFull = new();
	private readonly Sound _collectPotion = new();
	private readonly Sound _collectGold = new();


	public override void _Ready()
	{
		_itemSelected.LoadStream("res://ui/inventory/audio/item_selected.wav");

		_itemRotated.LoadStream("res://ui/inventory/audio/item_rotated.wav");

		_itemSetInPlace.LoadStream("res://ui/inventory/audio/item_set_in_place.wav");

		//_itemDropped.LoadStream("res://ui/inventory/audio/item_dropped.wav");

		// _operationCancelled.LoadStream("res://ui/inventory/audio/operation_cancelled.wav");

		_inventoryFull.LoadStream("res://ui/inventory/audio/inventory_full.wav");

		_collectPotion.LoadStream("res://ui/inventory/audio/collect_potion.wav");

		_collectGold.LoadStream("res://ui/inventory/audio/collect_gold.wav");
	}

	public void PlayItemSelected()
	{
		PlaySound(_itemSelected);
	}

	public void PlayItemRotated()
	{
		PlaySound(_itemRotated);
	}

	public void PlayItemSetInPlace()
	{
		PlaySound(_itemSetInPlace);
	}

	public void PlayOperationCancelled()
	{
		//PlaySound(_operationCancelled);
	}

	public void PlayInventoryFull()
	{
		PlaySound(_inventoryFull);
	}

	public void PlayCollectPotion()
	{
		PlaySound(_collectPotion);
	}

	public void PlayCollectGold()
	{
		PlaySound(_collectGold);
	}
}
