using Godot;
using System;

public partial class PlayerAudio : SoundPlayer
{
	[ExportGroup("Mixer")]	
	[Export(PropertyHint.Range, "-80, 24")]
	private float FootstepsSlow {
		get => _footstepsSlow.Volume;
		set => SetSoundVolume(_footstepsSlow, value);
	}
	[Export(PropertyHint.Range, "-80, 24")]
	private float FootstepsFast
	{
		get => _footstepsFast.Volume;
		set => SetSoundVolume(_footstepsFast, value);
	}
	[Export(PropertyHint.Range, "-80, 24")]
	private float Jump
	{
		get => _jump.Volume;
		set => SetSoundVolume(_jump, value);
	}
	[Export(PropertyHint.Range, "-80, 24")]
	private float GotHit
	{
		get => _gotHit.Volume;
		set => SetSoundVolume(_gotHit, value);
	}


	private readonly Sound _footstepsSlow = new();
	private readonly Sound _footstepsFast = new();
	private readonly Sound _jump = new();
	private readonly Sound _gotHit = new();


	public override void _Ready()
	{
		_footstepsSlow.LoadStream("res://player/audio/footsteps_slow.wav");
		_footstepsSlow.IsContinuous = true;

		_footstepsFast.LoadStream("res://player/audio/footsteps_fast.wav");
		_footstepsFast.IsContinuous = true;

		_jump.LoadStream("res://player/audio/jump.wav");

		_gotHit.LoadStream("res://player/audio/got_hit.wav");
	}


	public void PlayCrouchedWalk()
	{
		PlaySound(_footstepsSlow);
	}

	public void PlayWalk()
	{
		PlaySound(_footstepsFast);
	}

	public void StopMoving()
	{
		StopContinuousSound();
	}

	public void PlayJump()
	{
		PlaySound(_jump);
	}
	
	public void PlayGotHit()
	{
		PlaySound(_gotHit);
	}
}

