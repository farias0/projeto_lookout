using Godot;

namespace projeto_lookout.libs
{
    public static class Resources
    {
        public static CharacterBody3D Player { get; set; }
        public static HUD HUD { get; set; }


        public static readonly PackedScene Arrow =
            GD.Load<PackedScene>("res://bow-n-arrow/arrow/arrow.tscn");

        public static readonly StandardMaterial3D ArrowNormalMaterial =
            GD.Load<StandardMaterial3D>("res://bow-n-arrow/arrow/arrow_normal_material.tres");

        public static readonly StandardMaterial3D ArrowHookMaterial =
            GD.Load<StandardMaterial3D>("res://bow-n-arrow/arrow/arrow_hook_material.tres");
    }
}
