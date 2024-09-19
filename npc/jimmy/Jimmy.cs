using Godot;
using System;
using projeto_lookout.libs;

public partial class Jimmy : Npc
{
	private enum State
	{
		Idle,
		GivingAlert
	}


	private const float AlertLength = 3f;


	private State _state;
	private bool _hasGivenAlert = false;
	private float _alertCountdown = -1;


	public override void _Process(double delta)
	{
		base._Process(delta);

		if (SeesPlayer && !_hasGivenAlert)
			StartGivingAlert();
	}

	public override void InteractWith(Node3D entity)
	{
		base.InteractWith(entity);
		// TODO dialogue
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
		_alertCountdown = AlertLength;
		NpcAudio.PlayHey();
		Debug.Log("Hey! You there! Come here!");
	}

	private void KeepGivingAlert(float delta)
	{
		_alertCountdown -= delta;
		TurnTarget = LastKnownPlayerPos;

		if (_alertCountdown <= 0)
		{
			StartPatrolling();
		}
	}
}
