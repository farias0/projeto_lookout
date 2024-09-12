using Godot;
using System;

namespace projeto_lookout.libs
{
    public partial class Draw : Node
    {

        /// <summary>
        /// Draws a line in 3D space.
        /// </summary>
        /// <returns>The line instance.</returns>
        public static MeshInstance3D Line3D(Node parent, Vector3 pointA, Vector3 pointB, Color color)
        {
            MeshInstance3D lineInstance;
            {
                Mesh mesh;
                {
                    SurfaceTool surfaceTool = new();
                    surfaceTool.Begin(Mesh.PrimitiveType.Lines);
                    surfaceTool.SetColor(color);
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

            return lineInstance;
        }
    }
}
