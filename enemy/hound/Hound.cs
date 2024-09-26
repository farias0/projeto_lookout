using Godot;
using System;

public partial class Hound : Enemy
{
	[Export]
	public override int Health { get; set; } = 50;
	[Export]
	public override int MeleeDamage { get; set; } = 60;
	[Export]
	public override float VisionDistance { get; set; } = 40;
	[Export]
	public override float PlayerConfirmDistance { get; set; } = 20;
	[ExportGroup("Speed")]
	[Export]
	public override float SpeedPatrolling { get; set; } = 4f;
	[Export]
	public override float SpeedSearching { get; set; } = 7f;
	[Export]
	public override float SpeedChasing { get; set; } = 17f;


	public override void _Ready()
	{
		base._Ready();

		Type = EnemyType.Melee;
		SyncEnemyType();
	}

	protected override float GetHeight()
	{
		CollisionShape3D collisionShape = GetNode<CollisionShape3D>("CollisionShape3D");
		BoxShape3D box = collisionShape.Shape as BoxShape3D;
		return box.Size.Y;
	}

	protected override void ChangeMeshMaterial(StandardMaterial3D material)
	{
		// So the Enemy class doesn't override the material
		// TODO fix this
	}
}
