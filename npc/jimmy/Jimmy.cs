using Godot;
using System;

public partial class Jimmy : Npc
{
	private enum State
	{
		Idle,
		GivingAlert
	}


	private const float AlertLength = 3f;


	private readonly Dialogue _alertDialogue = new(new string[] {
		"Ei! O que voce ta fazendo? Esse lugar ta cheio de guardas.",
		"Venha falar comigo, eu tenho um plano pra tirar a gente daqui."
	});

	private State _state;
	private bool _hasGivenAlert = false;


	public override void _Process(double delta)
	{
		base._Process(delta);

		if (SeesPlayer && !_hasGivenAlert)
			StartGivingAlert();
	}

	public override void InteractWith(Node3D entity)
	{
		if (_state == State.GivingAlert)
		{
			_alertDialogue.NextLine();
		}
	}

	public override void KeepInteracting(float delta)
	{
		switch (_state)
		{
			case State.GivingAlert:
				KeepGivingAlert(delta);
				break;
			case State.Idle:
				StartPatrolling();
				break;
		}
	}

	private void StartGivingAlert()
	{
		StartInteracting();
		_hasGivenAlert = true;
		_state = State.GivingAlert;
		NpcAudio.PlayHey();
		_alertDialogue.Start();
	}

	private void KeepGivingAlert(float delta)
	{
		TurnTarget = LastKnownPlayerPos;
		_alertDialogue.Process(delta);

		if (_alertDialogue.IsFinished)
		{
			StartPatrolling();
		}
	}
}
