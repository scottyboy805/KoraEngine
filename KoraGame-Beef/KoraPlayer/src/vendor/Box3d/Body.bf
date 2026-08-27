using System;

namespace Box3d;

static
{
	[CRepr]
	enum b3BodyType
	{
		b3_staticBody = 0,
		b3_kinematicBody = 1,
		b3_dynamicBody = 2,
		b3_bodyTypeCount,
	}

	[CRepr]
	struct b3BodyDef
	{
		public b3BodyType type;
		public b3Pos position;
		public b3Quat rotation;
		public b3Vec3 linearVelocity;
		public b3Vec3 angularVelocity;
		public float linearDamping;
		public float angularDamping;
		public float gravityScale;
		public float sleepThreshold;
		public char8* name;
		public void* userData;
		public b3MotionLocks motionLocks;
		public bool enableSleep;
		public bool isAwake;
		public bool isBullet;
		public bool isEnabled;
		public bool allowFastRotation;
		public bool enableContactRecycling;
		public int32 internalValue;
	}

	[CRepr]
	struct b3BodyId
	{
		int32 index1;
		uint16 world0;
		uint16 generation;
	}

	[Import(Box3dLib), CLink]
	public static extern b3BodyId b3CreateBody(b3WorldId worldId, b3BodyDef* def);

	[Import(Box3dLib), CLink]
	public static extern void b3DestroyBody(b3BodyId bodyId);

	[Import(Box3dLib), CLink]
	public static extern bool b3Body_IsValid(b3BodyId bodyId);

	[Import(Box3dLib), CLink]
	public static extern b3BodyType b3Body_GetType(b3BodyId bodyId);

	[Import(Box3dLib), CLink]
	public static extern void b3Body_SetType(b3BodyId bodyId, b3BodyType type);

	[Import(Box3dLib), CLink]
	public static extern b3Pos b3Body_GetPosition(b3BodyId bodyId);

	[Import(Box3dLib), CLink]
	public static extern b3Quat b3Body_GetRotation(b3BodyId bodyId);

	[Import(Box3dLib), CLink]
	public static extern void b3Body_Enable(b3BodyId bodyId);

	[Import(Box3dLib), CLink]
	public static extern void b3Body_Disable(b3BodyId bodyId);

	[Import(Box3dLib), CLink]
	public static extern void b3Body_SetMotionLocks(b3BodyId bodyId, b3MotionLocks locks);
}