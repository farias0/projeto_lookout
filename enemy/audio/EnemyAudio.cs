using Godot;
using System;

public partial class EnemyAudio : SoundPlayer3D
{
	[ExportGroup("Mixer")]
	[Export(PropertyHint.Range, "-80, 24, or_less")]
	private float Alert
	{
		get => _alert.Volume;
		set => SetSoundVolume(_alert, value);
	}
	[Export(PropertyHint.Range, "-80, 24, or_less")]
	private float Chasing
	{
		get => _chasing.Volume;
		set => SetSoundVolume(_chasing, value);
	}
	[Export(PropertyHint.Range, "-80, 24, or_less")]
	private float GotHit
	{
		get => _gotHit.Volume;
		set => SetSoundVolume(_gotHit, value);
	}

	private readonly Sound _alert = new();
	private readonly Sound _chasing = new();
	private readonly Sound _gotHit = new();

	public override void _Ready()
	{
		_alert.LoadStream("res://enemy/audio/alert.wav");

		_chasing.LoadStream("res://enemy/audio/chasing.wav");

		_gotHit.LoadStream("res://enemy/audio/got_hit.wav");
	}

	public void PlayAlert()
	{
		PlaySound(_alert);
	}

	public void PlayChasing()
	{
		PlaySound(_chasing);
	}

	public void PlayGotHit()
	{
		PlaySound(_gotHit);
	}
}
