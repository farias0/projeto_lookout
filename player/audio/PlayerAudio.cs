using Godot;
using System;

public partial class PlayerAudio : AudioStreamPlayer
{
	private AudioStream _footstepsSlow;
	private AudioStream _footstepsFast;

	public override void _Ready()
	{
		_footstepsSlow = GD.Load<AudioStream>("res://player/audio/footsteps_slow.wav");
		_footstepsFast = GD.Load<AudioStream>("res://player/audio/footsteps_fast.wav");
	}

	public override void _Process(double delta)
	{
		if (Stream != null && !IsPlaying())
		{
			Play();
		}
	}

	public void PlayCrouchedWalk()
	{
		if (Stream != _footstepsSlow) {
			Stream = _footstepsSlow;
			Play();
		}
	}

	public void PlayWalk()
	{
		if (Stream != _footstepsFast)
		{
			Stream = _footstepsFast;
			Play();
		}
	}

	public void StopMoving()
	{
		if (Stream == _footstepsSlow ||
			Stream == _footstepsFast)
		{
			Stream = null;
			Stop();
		}
	}
}

