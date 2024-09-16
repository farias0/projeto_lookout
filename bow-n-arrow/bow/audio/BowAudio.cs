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

	private readonly Sound _tensing = new();

	public override void _Ready()
	{
		_tensing.LoadStream("res://bow-n-arrow/bow/audio/tensing.wav");
	}

	public void PlayTensing()
	{
		PlaySound(_tensing);
	}

	public void CancelTensing()
	{
		StopSound();
	}
}
