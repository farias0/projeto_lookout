using Godot;
using System;

public partial class ArrowAudio : AudioStreamPlayer3D
{
	private AudioStream _bowTensing;
	private AudioStream _fired;
	private AudioStream _flying;

	public override void _Ready()
	{
		_bowTensing = GD.Load<AudioStream>("res://bow-n-arrow/arrow/audio/bow_tensing.wav");
	}

	public void PlayPulledBack()
	{
		PlayOneOffSound(_bowTensing);
	}

	public void PlayFired()
	{
		PlayOneOffSound(_fired);
	}

	private void PlayOneOffSound(AudioStream stream)
	{
		Stream = stream;
		Play();
	}
}
