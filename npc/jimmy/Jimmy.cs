using Godot;
using System;

public partial class Jimmy : Npc
{
	private static readonly Dialogue _alertDialogue =
		new("res://npc/jimmy/dialogue/alert_dialogue.tres");
	
	private static readonly Dialogue _proposalDialogue =
		new("res://npc/jimmy/dialogue/proposal_dialogue.tres");


	private bool _hasGivenAlert = false;


	public override void _Process(double delta)
	{
		base._Process(delta);

		if (SeesPlayer && State != NpcState.InDialogue && !_hasGivenAlert)
		{
			StartGivingAlert();
		}
	}

	public override void InteractWith(Node3D entity)
	{
		base.InteractWith(entity);

		if (State != NpcState.InDialogue)
		{
			if (!_hasGivenAlert)
				StartGivingAlert();
			else
				StartProposalDialogue();
		}
	}

	private void StartGivingAlert()
	{
		_hasGivenAlert = true;
		StartDialogue(_alertDialogue);
		NpcAudio.PlayHey();
	}

	private void StartProposalDialogue()
	{
		StartDialogue(_proposalDialogue);
	}
}
