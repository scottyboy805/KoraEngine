using System;

namespace Box3d;

static
{
	public const String Box3dLib = "box3d.dll";

	[CRepr]
	public struct b3Pos
	{
		public double x, y, z;
	}

	[CRepr]
	public struct b3Vec3
	{
		public float x, y, z;
	}

	[CRepr]
	public struct b3Quat
	{
		public b3Vec3 v;
		public float s;
	}

	[CRepr]
	struct b3MotionLocks
	{
		public bool linearX;
		public bool linearY;
		public bool linearZ;
		public bool angularX;
		public bool angularY;
		public bool angularZ;
	}
}