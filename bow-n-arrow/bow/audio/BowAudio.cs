using Godot;
using System;

public partial class BowAudio : SoundPlayer3D
{
	[ExportGroup("Mixer")]
	[Export(PropertyHint.Range, "-80, 24")]
	private float HookPullIn
	{
		get => _hookPullIn.Volume;
		set => SetSoundVolume(_hookPullIn, value);
	}

	private readonly Sound _hookPullIn = new();

	public override void _Ready()
	{
		_hookPullIn.LoadStream("res://bow-n-arrow/bow/audio/hook_pulled_in.wav");
		_hookPullIn.IsContinuous = true;
	}

	public void PlayHookPullIn()
	{
		PlaySound(_hookPullIn);
	}
}
