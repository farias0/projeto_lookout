using Godot;
using System;

public partial class PlayerAudio : AudioStreamPlayer
{
	private AudioStream _footstepsSlow;
	private AudioStream _footstepsFast;
	private AudioStream _jump;

	private AudioStream _movementSound; // They are played looped and can be interrupted

	public override void _Ready()
	{
		_footstepsSlow = GD.Load<AudioStream>("res://player/audio/footsteps_slow.wav");
		_footstepsFast = GD.Load<AudioStream>("res://player/audio/footsteps_fast.wav");
		_jump = GD.Load<AudioStream>("res://player/audio/jump.wav");
	}

	public override void _Process(double delta)
	{
		// Loop
		if (!IsPlaying() && _movementSound != null)
		{
			Stream = _movementSound;
			Play();
		}
	}

	public void PlayCrouchedWalk()
	{
		PlayMovementSound(_footstepsSlow);
	}

	public void PlayWalk()
	{
		PlayMovementSound(_footstepsFast);
	}

	public void StopMoving()
	{
		if (IsMovementSound(Stream))
		{
			Stop();
		}
		_movementSound = null;
	}

	public void PlayJump()
	{
		PlayOneOffSound(_jump);
	}

	private bool IsMovementSound(AudioStream stream)
	{
		return (stream == _footstepsSlow ||
				stream == _footstepsFast);
	}

	private void PlayMovementSound(AudioStream stream)
	{
		if (!IsPlaying() || (Stream != stream && IsMovementSound(Stream))) {
			Stream = stream;
			Play();
		}
		_movementSound = stream;
	}

	private void PlayOneOffSound(AudioStream stream)
	{
		Stream = _jump;
		Play();
	}
}

