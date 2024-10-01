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
	[Export(PropertyHint.Range, "-80, 24")]
	private float FiredRocket
	{
		get => _firedRocket.Volume;
		set => SetSoundVolume(_firedRocket, value);
	}

	private readonly Sound _tensing = new();
	private readonly Sound _fired = new();
	private readonly Sound _firedRocket = new();

	public override void _Ready()
	{
		_tensing.LoadStream("res://bow-n-arrow/bow/audio/tensing.wav");
		_fired.LoadStream("res://bow-n-arrow/bow/audio/fired.wav");
		_firedRocket.LoadStream("res://bow-n-arrow/bow/audio/fired_rocket.wav");
	}

	public void PlayTensing()
	{
		PlaySound(_tensing);
	}

	public void CancelTensing()
	{
		if (CurrentSound == _tensing)
			StopSound();
	}

	public void PlayFired()
	{
		PlaySound(_fired);
	}

	public void PlayFiredRocket()
	{
		PlaySound(_firedRocket);
	}

}
