using Godot;

namespace projeto_lookout.libs
{
	public static class Resources
	{
		public static Player Player { get; set; }
		public static HUD HUD { get; set; }
		public static Camera Camera { get; set; }


		//      Arrow

		public static readonly PackedScene Arrow =
			GD.Load<PackedScene>("res://bow-n-arrow/arrow/arrow.tscn");

		public static readonly StandardMaterial3D ArrowNormalMaterial =
			GD.Load<StandardMaterial3D>("res://bow-n-arrow/arrow/arrow_normal_material.tres");

		public static readonly StandardMaterial3D ArrowHookMaterial =
			GD.Load<StandardMaterial3D>("res://bow-n-arrow/arrow/arrow_hook_material.tres");



		//      Enemy

		public static readonly StandardMaterial3D EnemyMeleeMaterial =
			GD.Load<StandardMaterial3D>("res://enemy/enemy_melee_material.tres");

		public static readonly StandardMaterial3D EnemyRangedMaterial =
			GD.Load<StandardMaterial3D>("res://enemy/enemy_ranged_material.tres");
	}
}
