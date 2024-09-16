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
	private Sound _currentSound;

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
			if (IsPlaying() && (Stream == sound.Stream || !_currentSound.IsContinuous))
			{
				return;
			}
		}

		_currentSound = sound;
		Stream = sound.Stream;
		SetVolume(sound.Volume);
		Play();
	}

	public void StopContinuousSound()
	{
		ContinuousSound = null;
		if (_currentSound != null && _currentSound.IsContinuous)
			Stop();
	}

	public void SetSoundVolume(Sound sound, float value)
	{
		sound.Volume = value;
		if (_currentSound == sound) SetVolume(value);
	}

	private void SetVolume(float volume)
	{
		VolumeDb = volume;
	}
}

public partial class SoundPlayer3D : AudioStreamPlayer3D
{
	private Sound _currentSound;

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
			if (IsPlaying() && (Stream == sound.Stream || !_currentSound.IsContinuous))
			{
				return;
			}
		}

		_currentSound = sound;
		Stream = sound.Stream;
		SetVolume(sound.Volume);
		Play();
	}

	public void StopSound()
	{
		_currentSound = null;
		ContinuousSound = null;
		Stream = null;
		Stop();
	}

	public void StopContinuousSound()
	{
		ContinuousSound = null;
		if (_currentSound != null && _currentSound.IsContinuous)
			Stop();
	}

	public void SetSoundVolume(Sound sound, float value)
	{
		sound.Volume = value;
		if (_currentSound == sound) SetVolume(value);
	}

	private void SetVolume(float volume)
	{
		VolumeDb = volume;
	}
}
