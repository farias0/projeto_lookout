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

	private readonly Sound _fired = new();
	private readonly Sound _flying = new();

	public override void _Ready()
	{
		
	}

	public void PlayFired()
	{
		//
	}
}
