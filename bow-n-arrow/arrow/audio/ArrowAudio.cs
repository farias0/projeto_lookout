using Godot;
using System;

public partial class ArrowAudio : SoundPlayer3D
{
	[ExportGroup("Mixer")]
	[Export(PropertyHint.Range, "-80, 24")]
	private float Flying
	{
		get => _flying.Volume;
		set => SetSoundVolume(_flying, value);
	}
	[Export(PropertyHint.Range, "-80, 24")]
	private float Hit
	{
		get => _hit.Volume;
		set => SetSoundVolume(_hit, value);
	}
	[Export(PropertyHint.Range, "-80, 24")]
	private float FlyingRocket
	{
		get => _flyingRocket.Volume;
		set => SetSoundVolume(_flyingRocket, value);
	}


	private readonly Sound _flying = new();
	private readonly Sound _flyingRocket = new();
	private readonly Sound _hit = new();


	public override void _Ready()
	{
		//_flying.LoadStream("res://bow-n-arrow/arrow/audio/flying.wav");
		_flying.IsContinuous = true;

		_flyingRocket.LoadStream("res://bow-n-arrow/arrow/audio/flying_rocket.wav");
		_flyingRocket.IsContinuous = true;
	}

	public void PlayFlying()
	{
		// PlaySound(_flying);
	}

	public void PlayFlyingRocket()
	{
		PlaySound(_flyingRocket);
	}

	public void PlayHit()
	{
		StopSound(); // TODO
	}
}
