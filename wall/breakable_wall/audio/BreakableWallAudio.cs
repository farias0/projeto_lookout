using Godot;
using System;

public partial class BreakableWallAudio : SoundPlayer3D
{
	[ExportGroup("Mixer")]
	[Export(PropertyHint.Range, "-80, 24")]
	private float Exploded
	{
		get => _exploded.Volume;
		set => SetSoundVolume(_exploded, value);
	}

	private readonly Sound _exploded = new();

	public override void _Ready()
	{
		_exploded.LoadStream("res://wall/breakable_wall/audio/exploded.wav");
	}

	public void PlayExploded()
	{
		PlaySound(_exploded);
	}
}
