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

		/// <summary>
		/// Draws a line in 2D space.
		/// </summary>
		/// <returns>The line instance.</returns>
		public static Line2D Line2D(Node parent, Vector2 pointA, Vector2 pointB, Color color, float width = 1f)
		{
			Line2D line = new()
			{
				Points = new Vector2[] { pointA, pointB },
				Width = width,
				DefaultColor = color
			};

			parent.AddChild(line);

			return line;
		}
	}

	public class Square
	{
		public readonly Line2D[] Lines = new Line2D[4];

		public void DrawSquare(Node parent, Rect2 points)
		{
			Lines[0] = Draw.Line2D(parent, points.Position, new Vector2(points.Position.X + points.Size.X, points.Position.Y), Colors.White);
			Lines[1] = Draw.Line2D(parent, new Vector2(points.Position.X + points.Size.X, points.Position.Y), points.End, Colors.White);
			Lines[2] = Draw.Line2D(parent, points.End, new Vector2(points.Position.X, points.Position.Y + points.Size.Y), Colors.White);
			Lines[3] = Draw.Line2D(parent, new Vector2(points.Position.X, points.Position.Y + points.Size.Y), points.Position, Colors.White);
		}

		public void QueueFree()
		{
			foreach (var line in Lines)
			{
				line?.QueueFree();
			}
		}
	}
}
