using Godot;

namespace projeto_lookout.libs
{
	public partial class Resources : Node
	{
		public static Resources Instance { get; private set; }

		public Player Player { get; set; }
		public HUD HUD { get; set; }
		public Camera Camera { get; set; }
		public Subtitles Subtitles { get; set; }
		public Inventory Inventory { get; set; }


		//      Arrow

		public readonly PackedScene Arrow =
			GD.Load<PackedScene>("res://bow-n-arrow/arrow/arrow.tscn");

		public readonly StandardMaterial3D ArrowNormalMaterial =
			GD.Load<StandardMaterial3D>("res://bow-n-arrow/arrow/arrow_normal_material.tres");

		public readonly StandardMaterial3D ArrowHookMaterial =
			GD.Load<StandardMaterial3D>("res://bow-n-arrow/arrow/arrow_hook_material.tres");



		public override void _Ready()
		{
			Instance = this;
		}
	}
}
