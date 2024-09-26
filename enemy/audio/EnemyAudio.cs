using Godot;
using System;

public partial class EnemyAudio : SoundPlayer3D
{
	[ExportGroup("Mixer")]
	[Export(PropertyHint.Range, "-80, 24, or_less")]
	private float Alert
	{
		get => AlertSound.Volume;
		set => SetSoundVolume(AlertSound, value);
	}
	[Export(PropertyHint.Range, "-80, 24, or_less")]
	private float Chasing
	{
		get => ChasingSound.Volume;
		set => SetSoundVolume(ChasingSound, value);
	}
	[Export(PropertyHint.Range, "-80, 24, or_less")]
	private float GotHit
	{
		get => GotHitSound.Volume;
		set => SetSoundVolume(GotHitSound, value);
	}

	protected readonly Sound AlertSound = new();
	protected readonly Sound ChasingSound = new();
	protected readonly Sound GotHitSound = new();

	public override void _Ready()
	{
		AlertSound.LoadStream("res://enemy/audio/alert.wav");

		ChasingSound.LoadStream("res://enemy/audio/chasing.wav");

		GotHitSound.LoadStream("res://enemy/audio/got_hit.wav");
	}

	public void PlayAlert()
	{
		PlaySound(AlertSound);
	}

	public void PlayChasing()
	{
		PlaySound(ChasingSound);
	}

	public void PlayGotHit()
	{
		PlaySound(GotHitSound);
	}
}
