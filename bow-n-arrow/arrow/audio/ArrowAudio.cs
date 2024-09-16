using Godot;
using System;

public partial class ArrowAudio : SoundPlayer3D
{
	[ExportGroup("Mixer")]
	[Export(PropertyHint.Range, "-80, 24")]
	private float BowTensing
	{
		get => _bowTensing.Volume;
		set => SetSoundVolume(_bowTensing, value);
	}
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

	private readonly Sound _bowTensing = new();
	private readonly Sound _fired = new();
	private readonly Sound _flying = new();

	public override void _Ready()
	{
		_bowTensing.LoadStream("res://bow-n-arrow/arrow/audio/bow_tensing.wav");
	}

	public void PlayPulledBack()
	{
		PlaySound(_bowTensing);
	}

	public void PlayFired()
	{
		StopSound();
	}
}
