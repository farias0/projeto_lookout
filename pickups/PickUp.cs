using Godot;
using System;
using projeto_lookout.libs;

public partial class PickUp : RigidBody3D
{
	[Export(PropertyHint.File)]
	public string InventoryItem // It's a string to avoid circular dependency
	{
		get => _inventoryItem.ResourcePath;
		set => _inventoryItem = GD.Load<PackedScene>(value);
	}


	private readonly float HookSpeed = 3000;

	private Area3D _pickupArea;
	private Node3D _hookedArrow;
	private PackedScene _inventoryItem;


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

	public virtual void OnPlayerPickup(Player player)
	{
		_hookedArrow?.QueueFree();


		if (_inventoryItem == null)
		{
			CallDeferred("queue_free");
		}
		else
		{
			if (Resources.Instance.Inventory.AddNewItem(_inventoryItem))
			{
				CallDeferred("queue_free");
			}
			else
			{
				Debug.Log($"Inventory rejected item {Name}.");
				// Keep existing
			}
		}
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
		}
	}
}
