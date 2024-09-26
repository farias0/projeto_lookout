using Godot;
using System;

public partial class HoundAudio : EnemyAudio
{
	public override void _Ready()
	{
		base._Ready();

		AlertSound.LoadStream("res://enemy/hound/audio/alert.wav");

		ChasingSound.LoadStream("res://enemy/hound/audio/chasing.wav");

		GotHitSound.LoadStream("res://enemy/hound/audio/got_hit.wav");
	}
}
