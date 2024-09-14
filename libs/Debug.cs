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
		/// <returns>The line instance.</returns>
		public static MeshInstance3D DrawLine3D(Node parent, Vector3 pointA, Vector3 pointB)
		{
			var lineInstance =  Draw.Line3D(parent, pointA, pointB, new Color(0, 1, 0));

			DebugLog($"Drew 3D line from {pointA} to {pointB}.");

			return lineInstance;
		}

		/// <summary>
		/// Draws a sphere in 3D space.
		/// </summary>
		/// <returns>The sphere instance.</returns>
		public static MeshInstance3D DrawSphere(Node parent, Vector3 center)
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

			return sphereInstance;
		}
	}
}
