using Godot;
using System;

public partial class Jimmy : Npc
{
	private readonly Dialogue _alertDialogue = new(new string[] {
		"Ei! O que voce ta fazendo? Esse lugar ta cheio de guardas.",
		"Venha falar comigo, eu tenho um plano pra tirar a gente daqui."
	});
	private readonly Dialogue _proposalDialogue = new(new string[] {
		"Voce tambem esta fugindo dos guardas, ne?",
		"Notei pelo seu jeito de andar.",
		"Escuta, eu posso tirar a gente daqui. So preciso de Y 700.",
		"Se voce conseguir esse dinheiro, venha falar comigo, combinado?"
	});


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
