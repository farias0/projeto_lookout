using Godot;
using Godot.Collections;

namespace projeto_lookout.libs
{
	public partial class Raycast
	{
		public static Dictionary CastRay(World3D world, Vector3 origin, Vector3 target)
		{
			var rayParams = new PhysicsRayQueryParameters3D
			{
				From = origin,
				To = target,
				CollideWithBodies = true,
				CollideWithAreas = true
			};
			return world.DirectSpaceState.IntersectRay(rayParams);
		}

		public static Dictionary CastRayInDirection(World3D world, Vector3 origin, Vector3 direction, float distance)
		{
			Vector3 target = origin + (direction.Normalized() * distance);
			return CastRay(world, origin, target);
		}
	}
}
