using System;
using Box3d;

namespace KoraPlayer.Physics;

static class Extensions
{
	// Methods
	[Inline]
	public static b3Vec3 ToBoxVec3(this in float3 val)
	{
		float3 local = val;
		return *(b3Vec3*)&local;
	}

	[Inline]
	public static b3Pos ToBoxPos(this in float3 val)
	{
		return .
		{
			x = val.x,
			y = val.y,
			z = val.z,
		};
	}

	[Inline]
	public static b3Quat ToBoxQuat(this in quaternion val)
	{
		quaternion local = val;
		return *(b3Quat*)&local;
	}



	[Inline]
	public static float3 ToKora(this in b3Vec3 val)
	{
		b3Vec3 local = val;
		return *(float3*)&local;
	}

	[Inline]
	public static float3 ToKora(this in b3Pos val)
	{
		return .
		{
		  	x = (float)val.x,
			y = (float)val.y,
			z = (float)val.z,
		};
	}

	[Inline]
	public static quaternion ToKora(this in b3Quat val)
	{
		b3Quat local = val;
		return *(quaternion*)&local;
	}
}