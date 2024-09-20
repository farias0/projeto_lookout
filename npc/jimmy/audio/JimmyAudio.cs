using Godot;
using System;

public partial class JimmyAudio : NpcAudio
{
	[ExportGroup("Mixer")]
	[Export(PropertyHint.Range, "-80, 24")]
	private float Laugh
	{
		get => _laugh.Volume;
		set => SetSoundVolume(_laugh, value);
	}

	private readonly Sound _laugh = new();

	public override void _Ready()
	{
		base._Ready();

		_laugh.LoadStream("res://npc/jimmy/audio/laugh.wav");
	}

	public void PlayLaugh()
	{
		PlaySound(_laugh);
	}
}
