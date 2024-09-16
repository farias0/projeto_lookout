using Godot;
using System;

public partial class EnemyAudio : SoundPlayer3D
{
	[ExportGroup("Mixer")]
	[Export(PropertyHint.Range, "-80, 24, or_less")]
	private float GotHit
	{
		get => _gotHit.Volume;
		set => SetSoundVolume(_gotHit, value);
	}

	private readonly Sound _gotHit = new();

	public override void _Ready()
	{
		_gotHit.LoadStream("res://enemy/audio/got_hit.wav");
	}

	public void PlayGotHit()
	{
		PlaySound(_gotHit);
	}
}
