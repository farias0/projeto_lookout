using Godot;
using System;
using System.Diagnostics;

namespace projeto_lookout.libs
{
    public class Debug
    {

        public static void Log (string message)
        {
            message = $" [*] {message}";

            // For Godot's editor
            GD.Print(message);

            // For Visual Studio's output
            Debugger.Log(2, "inf", message + '\n');
        }

        public static void LogError(string message) => Log($"[ERROR] {message}");

        private static void DebugLog(string message) => Log($"[Debug] {message}");

        /// <summary>
        /// Draws a line in 3D space.
        /// </summary>
        public static void Draw3DLine(Node parent, Vector3 pointA, Vector3 pointB)
        {
            MeshInstance3D lineInstance;
            {
                Mesh mesh;
                {
                    SurfaceTool surfaceTool = new();
                    surfaceTool.Begin(Mesh.PrimitiveType.Lines);
                    surfaceTool.SetColor(new Color(0, 1, 0));
                    surfaceTool.AddVertex(pointA);
                    surfaceTool.AddVertex(pointB);
                    mesh = surfaceTool.Commit();
                }

                lineInstance = new()
                {
                    Mesh = mesh
                };
            }

            StandardMaterial3D material = new()
            {
                VertexColorUseAsAlbedo = true
            };

            lineInstance.MaterialOverride = material;

            parent.AddChild(lineInstance);

            DebugLog($"Drew 3D line from {pointA} to {pointB}.");
        }

        public static void DrawSphere(Node parent, Vector3 center)
        {
            MeshInstance3D sphereInstance = new()
            {
                Mesh = new SphereMesh()
                {
                    Radius = 0.3f
                },

                MaterialOverride = new StandardMaterial3D()
                {
                    VertexColorUseAsAlbedo = true,
                    AlbedoColor = new Color(1, 0, 0)
                },

                GlobalPosition = center,
            };

            parent.AddChild(sphereInstance);

            DebugLog($"Drew 3D sphere at {center}.");
        }
    }
}
