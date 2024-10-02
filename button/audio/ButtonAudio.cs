using Godot;
using System;

public partial class ButtonAudio : SoundPlayer3D
{
	[ExportGroup("Mixer")]
	[Export(PropertyHint.Range, "-80, 24")]
	private float Pressed
	{
		get => _pressed.Volume;
		set => SetSoundVolume(_pressed, value);
	}

	private readonly Sound _pressed = new();

	public override void _Ready()
	{
		_pressed.LoadStream("res://button/audio/pressed.wav");
	}

	public void PlayPressed()
	{
		PlaySound(_pressed);
	}
}
