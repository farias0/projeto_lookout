using Godot;
using System;

public partial class PullerAudio : SoundPlayer3D
{
	[ExportGroup("Mixer")]
	[Export(PropertyHint.Range, "-80, 24, or_less")]
	private float PullIn
	{
		get => _pullIn.Volume;
		set => SetSoundVolume(_pullIn, value);
	}

	private readonly Sound _pullIn = new();

	public override void _Ready()
	{
		_pullIn.LoadStream("res://player/puller/pull_in.wav");
		_pullIn.IsContinuous = true;
	}

	public void PlayPullIn()
	{
		PlaySound(_pullIn);
	}
}
