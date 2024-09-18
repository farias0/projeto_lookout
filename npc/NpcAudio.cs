using Godot;
using System;

public partial class NpcAudio : SoundPlayer3D
{
	[ExportGroup("Mixer")]
	[Export(PropertyHint.Range, "-80, 24")]
	private float GotHit
	{
		get => _gotHit.Volume;
		set => SetSoundVolume(_gotHit, value);
	}
	[ExportGroup("Mixer")]
	[Export(PropertyHint.Range, "-80, 24")]
	private float Hey
	{
		get => _hey.Volume;
		set => SetSoundVolume(_hey, value);
	}

	private readonly Sound _gotHit = new();
	private readonly Sound _hey = new();

	public override void _Ready()
	{
		_gotHit.LoadStream("res://npc/audio/got_hit.wav");

		_hey.LoadStream("res://npc/audio/hey.wav");
	}

	public void PlayGotHit()
	{
		PlaySound(_gotHit);
	}

	public void PlayHey()
	{
		PlaySound(_hey);
	}
}
