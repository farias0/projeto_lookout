using Godot;
using System;

public partial class Merchant : Npc
{
	private static readonly Dialogue AnouncementDialogue =
		new("res://npc/merchant/dialogue/anouncement_dialogue.tres");
	
	private static readonly Trade Trade =
		GD.Load<Trade>("res://npc/merchant/trade.tres");


	private bool _hasMadeAnouncement = false;


	public override void _Ready()
	{
		base._Ready();
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		if (SeesPlayer && State != NpcState.InDialogue && !_hasMadeAnouncement)
		{
			MakeAnouncement();
		}
	}

	public override void InteractWith(Node3D entity)
	{
		base.InteractWith(entity);

		if (State != NpcState.InDialogue)
		{
			Sell();
		}
	}

	private void MakeAnouncement()
	{
		_hasMadeAnouncement = true;
		StartDialogue(AnouncementDialogue);
		NpcAudio.PlayHey();
	}

	private void Sell()
	{
		Trade.StartTrade(this);
	}
}
