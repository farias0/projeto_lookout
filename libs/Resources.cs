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


		public readonly PackedScene Arrow =
			GD.Load<PackedScene>("res://bow-n-arrow/arrow/arrow.tscn");

		public readonly PackedScene Explosion =
			GD.Load<PackedScene>("res://explosion/explosion.tscn");


		public override void _Ready()
		{
			Instance = this;
		}
	}
}
