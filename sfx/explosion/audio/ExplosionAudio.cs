using Godot;
using System;

public partial class ExplosionAudio : SoundPlayer3D
{
	[ExportGroup("Mixer")]
	[Export(PropertyHint.Range, "-80, 24")]
	private float Explosion
	{
		get => _explosion.Volume;
		set => SetSoundVolume(_explosion, value);
	}

	private readonly Sound _explosion = new();

	public override void _Ready()
	{
		_explosion.LoadStream("res://sfx/explosion/audio/explosion.wav");
	}

	public void PlayExplosion()
	{
		PlaySound(_explosion);
	}
}
