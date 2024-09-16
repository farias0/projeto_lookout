using Godot;
using System;

public partial class BowAudio : SoundPlayer3D
{
	[ExportGroup("Mixer")]
	[Export(PropertyHint.Range, "-80, 24")]
	private float Tensing
	{
		get => _tensing.Volume;
		set => SetSoundVolume(_tensing, value);
	}
	[Export(PropertyHint.Range, "-80, 24")]
	private float Fired
	{
		get => _fired.Volume;
		set => SetSoundVolume(_fired, value);
	}

	private readonly Sound _tensing = new();
	private readonly Sound _fired = new();

	public override void _Ready()
	{
		_tensing.LoadStream("res://bow-n-arrow/bow/audio/tensing.wav");
		_fired.LoadStream("res://bow-n-arrow/bow/audio/fired.wav");
	}

	public void PlayTensing()
	{
		PlaySound(_tensing);
	}

	public void PlayFired()
	{
		PlaySound(_fired);
	}
}
