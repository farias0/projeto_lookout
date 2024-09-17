using Godot;
using System;

public partial class Sound
{
	public AudioStream Stream { get; set; } = new();
	public float Volume { get; set; } = 1;
	public bool IsContinuous { get; set; } = false;

	public void LoadStream(string filename)
	{
		Stream = GD.Load<AudioStream>(filename);
	}
}

// TODO Deduplicate SoundPlayers

public partial class SoundPlayer : AudioStreamPlayer
{
	public Sound CurrentSound;

	public Sound ContinuousSound;

	public override void _Process(double delta)
	{
		// Loop
		if (!IsPlaying() && ContinuousSound != null)
		{
			PlaySound(ContinuousSound);
		}
	}

	public void PlaySound(Sound sound)
	{
		if (sound.IsContinuous)
		{
			ContinuousSound = sound;
			if (IsPlaying() && (Stream == sound.Stream || !CurrentSound.IsContinuous))
			{
				return;
			}
		}

		CurrentSound = sound;
		Stream = sound.Stream;
		SetVolume(sound.Volume);
		Play();
	}

	public void StopSound()
	{
		CurrentSound = null;
		ContinuousSound = null;
		Stream = null;
		Stop();
	}

	public void StopContinuousSound()
	{
		ContinuousSound = null;
		if (CurrentSound != null && CurrentSound.IsContinuous)
			Stop();
	}

	public void SetSoundVolume(Sound sound, float value)
	{
		sound.Volume = value;
		if (CurrentSound == sound) SetVolume(value);
	}

	private void SetVolume(float volume)
	{
		VolumeDb = volume;
	}
}

public partial class SoundPlayer3D : AudioStreamPlayer3D
{
	public Sound CurrentSound;

	public Sound ContinuousSound;

	public override void _Process(double delta)
	{
		// Loop
		if (!IsPlaying() && ContinuousSound != null)
		{
			PlaySound(ContinuousSound);
		}
	}

	public void PlaySound(Sound sound)
	{
		if (sound.IsContinuous)
		{
			ContinuousSound = sound;
			if (IsPlaying() && (Stream == sound.Stream || !CurrentSound.IsContinuous))
			{
				return;
			}
		}

		CurrentSound = sound;
		Stream = sound.Stream;
		SetVolume(sound.Volume);
		Play();
	}

	public void StopSound()
	{
		CurrentSound = null;
		ContinuousSound = null;
		Stream = null;
		Stop();
	}

	public void StopContinuousSound()
	{
		ContinuousSound = null;
		if (CurrentSound != null && CurrentSound.IsContinuous)
			Stop();
	}

	public void SetSoundVolume(Sound sound, float value)
	{
		sound.Volume = value;
		if (CurrentSound == sound) SetVolume(value);
	}

	private void SetVolume(float volume)
	{
		VolumeDb = volume;
	}
}
