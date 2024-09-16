using Godot;
using System;

public partial class ArrowAudio : SoundPlayer3D
{
	[ExportGroup("Mixer")]
	[Export(PropertyHint.Range, "-80, 24")]
	private float Fired
	{
		get => _fired.Volume;
		set => SetSoundVolume(_fired, value);
	}
	[Export(PropertyHint.Range, "-80, 24")]
	private float Flying
	{
		get => _flying.Volume;
		set => SetSoundVolume(_flying, value);
	}
	[Export(PropertyHint.Range, "-80, 24")]
	private float Hit
	{
		get => _hit.Volume;
		set => SetSoundVolume(_hit, value);
	}

	private readonly Sound _fired = new();
	private readonly Sound _flying = new();
	private readonly Sound _hit = new();

	public override void _Ready()
	{
		_flying.IsContinuous = true;
	}

	public void PlayFired()
	{
		//
	}

	public void PlayFlying()
	{
		//
	}

	public void PlayHit()
	{
		//
	}
}
