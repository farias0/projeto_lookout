using Godot;
using System;

/*
 *		This secondary SoundPlayer is used for special effects,
 *		instead of the player character's natural sounds.
 *		This is so those sounds can play without being interrupted by the player's.
 */

public partial class EffectsAudio : SoundPlayer
{
	[ExportGroup("Mixer")]
	[Export(PropertyHint.Range, "-80, 24")]
	private float Heal
	{
		get => _fillStamina.Volume;
		set => SetSoundVolume(_fillStamina, value);
	}
	[Export(PropertyHint.Range, "-80, 24")]
	private float Slide
	{
		get => _slide.Volume;
		set => SetSoundVolume(_slide, value);
	}
	[Export(PropertyHint.Range, "-80, 24")]
	private float FillStamina
	{
		get => _heal.Volume;
		set => SetSoundVolume(_heal, value);
	}
	[Export(PropertyHint.Range, "-80, 24, or_less")]
	private float CollectGold
	{
		get => _collectGold.Volume;
		set => SetSoundVolume(_collectGold, value);
	}

	private readonly Sound _heal = new();
	private readonly Sound _slide = new();
	private readonly Sound _fillStamina = new();
	private readonly Sound _collectGold = new();

	public override void _Ready()
	{
		_heal.LoadStream("res://player/effects_audio/heal.wav");

		_slide.LoadStream("res://player/effects_audio/slide.wav");

		_fillStamina.LoadStream("res://player/effects_audio/fill_stamina.wav");

		_collectGold.LoadStream("res://player/effects_audio/collect_gold.wav");
	}

	public void PlayHeal()
	{
		PlaySound(_heal);
	}

	public void PlaySlide()
	{
		PlaySound(_slide);
	}

	public void CancelSlide()
	{
		if (CurrentSound == _slide)
			StopSound();
	}

	public void PlayFillStamina()
	{
		PlaySound(_fillStamina);
	}

	public void PlayCollectGold()
	{
		PlaySound(_collectGold);
	}
}
