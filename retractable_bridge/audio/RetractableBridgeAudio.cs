using Godot;
using System;

public partial class RetractableBridgeAudio : SoundPlayer3D
{
	[ExportGroup("Mixer")]
	[Export(PropertyHint.Range, "-80, 24")]
	private float Activate
	{
		get => _activate.Volume;
		set => _activate.Volume = value;
	}

	private readonly Sound _activate = new();

	public override void _Ready()
	{
		_activate.LoadStream("res://retractable_bridge/audio/activate.wav");
		_activate.IsContinuous = false;
	}

	public void PlayActivate()
	{
		PlaySound(_activate);
	}

	public void StopActivate()
	{
		if (CurrentSound == _activate)
			StopSound();
	}
}
