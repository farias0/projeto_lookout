using Godot;
using System;
using projeto_lookout.libs;

public partial class Jimmy : Npc
{
	private static readonly Dialogue _alertDialogue =
		new("res://npc/jimmy/dialogue/alert_dialogue.tres");
	
	private static readonly Dialogue _proposalDialogue =
		new("res://npc/jimmy/dialogue/proposal_dialogue.tres");

	private static readonly Dialogue _proposalConfirmationDialogue =
		new("res://npc/jimmy/dialogue/proposal_confirmation_dialogue.tres");

	private static readonly Dialogue _proposalConfirmedDialogue =
		new("res://npc/jimmy/dialogue/proposal_confirmed_dialogue.tres");


	private const int GoldAsked = 400;

	private JimmyAudio _audio;

	private bool _hasGivenAlert = false;
	private bool _hasMadeProposal = false;
	private bool _hasAcceptedProposal = false;
	private bool _isExecutingPlan = false;


	public override void _Ready()
	{
		base._Ready();

		_audio = GetNode<JimmyAudio>("AudioStreamPlayer3D");
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		if (SeesPlayer && State != NpcState.InDialogue && !_hasGivenAlert)
		{
			GiveAlert();
		}
		else if (_isExecutingPlan)
		{
			if (State == NpcState.InDialogue)
				ContinuePlan();
			else
				_isExecutingPlan = false; // Canceled it
		}
	}

	public override void InteractWith(Node3D entity)
	{
		base.InteractWith(entity);

		if (State != NpcState.InDialogue)
		{
			if (!_hasGivenAlert)
				GiveAlert();
			else if (!_hasMadeProposal)
				MakeProposal();
			else if (!_hasAcceptedProposal)
				AcceptProposal();
			else
				StartPlan();
		}
	}

	private void GiveAlert()
	{
		_hasGivenAlert = true;
		StartDialogue(_alertDialogue);
		NpcAudio.PlayHey();
	}

	private void MakeProposal()
	{
		_hasMadeProposal = true;
		StartDialogue(_proposalDialogue);
	}

	private void AcceptProposal()
	{
		if (Resources.Instance.Player.GetGoldAmount() >= GoldAsked)
		{
			_hasAcceptedProposal = true;
			StartDialogue(_proposalConfirmationDialogue);

		}
		else MakeProposal();
	}

	private void StartPlan()
	{
		if (Resources.Instance.Player.GetGoldAmount() >= GoldAsked)
		{
			StartDialogue(_proposalConfirmedDialogue);
			_isExecutingPlan = true;
		}
		else
		{
			_hasAcceptedProposal = false;
			MakeProposal();
		}
	}

	private void ContinuePlan()
	{
		var player = Resources.Instance.Player;
		bool playerTurnedAway = false;

		var playerForward = player.GetGlobalTransform().Basis.Z;
		var directionToEnemy = (GlobalTransform.Origin - player.GlobalTransform.Origin).Normalized();
		playerTurnedAway = -1 + Mathf.Abs(playerForward.Dot(directionToEnemy)) < -0.7;

		if (playerTurnedAway)
		{
			player.SubtractGold(player.GetGoldAmount());

			PullArrowBack();
			FireArrowAt(LastKnownPlayerPos);

			// _audio.PlayLaugh(); // TODO find a way to play it while disappearing with Jimmy

			_proposalConfirmedDialogue.Stop();
			QueueFree();
		}
	}
}
