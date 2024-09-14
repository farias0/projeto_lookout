using Godot;
using System;

public abstract partial class PickUp : RigidBody3D
{
	private readonly float HookSpeed = 3000;

	private Area3D _pickupArea;
	private Node3D _hookedArrow;


	public abstract void OnPlayerPickup(Player player);


	public override void _Ready()
	{
		ContactMonitor = true;
		MaxContactsReported = 1;

		_pickupArea = GetNode<Area3D>("Area3D");
		_pickupArea.Monitoring = true;
		_pickupArea.Connect("body_entered", new Callable(this, nameof(OnAreaEntered)));
	}

	public override void _IntegrateForces(PhysicsDirectBodyState3D state)
	{
		if (_hookedArrow != null)
		{
			PulledByHook(state);
		}
	}

	public void ArrowHooked(Node3D arrow)
	{
		Sleeping = false;
		_hookedArrow = arrow;
	}

	private void PulledByHook(PhysicsDirectBodyState3D state) {

		var direction = (_hookedArrow as Arrow).HookGetPullDirection();

		if (state.LinearVelocity.Length() < HookSpeed)
		{
			state.LinearVelocity += direction * state.Step * 100;
		}
		else
		{
			state.LinearVelocity = direction * HookSpeed;
		}
	}

	private void OnAreaEntered(Node body)
	{
		if (body is Player player)
		{
			OnPlayerPickup(player);

			if (_hookedArrow != null)
					(_hookedArrow as Arrow).DetachPickup();

			CallDeferred("queue_free");
		}
	}
}
