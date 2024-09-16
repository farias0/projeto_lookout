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
	[ExportGroup("Mixer")]
	[Export(PropertyHint.Range, "-80, 24, or_less")]
	private float CollectGold
	{
		get => _collectGold.Volume;
		set => SetSoundVolume(_collectGold, value);
	}


	private readonly Sound _footstepsSlow = new();
	private readonly Sound _footstepsFast = new();
	private readonly Sound _jump = new();
	private readonly Sound _collectGold = new();


	public override void _Ready()
	{
		_footstepsSlow.LoadStream("res://player/audio/footsteps_slow.wav");
		_footstepsSlow.IsContinuous = true;

		_footstepsFast.LoadStream("res://player/audio/footsteps_fast.wav");
		_footstepsFast.IsContinuous = true;

		_jump.LoadStream("res://player/audio/jump.wav");

		_collectGold.LoadStream("res://player/audio/collect_gold.wav");
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

	public void PlayCollectGold()
	{
		PlaySound(_collectGold);
	}
}

