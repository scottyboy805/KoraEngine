using System;

namespace Box3d;

[CRepr]
static
{
	struct b3SurfaceMaterial
	{
		public float friction;
		public float restitution;
		public float rollingResistance;
		public b3Vec3 tangentVelocity;
		public uint64 userMaterialId;
		public uint32 customColor;
	}

	struct b3Filter
	{
		public uint64 categoryBits;
		public uint64 maskBits;
		public int32 groupIndex;
	}

	enum b3ShapeType
	{
		b3_capsuleShape,
		b3_compoundShape,
		b3_heightShape,
		b3_hullShape,
		b3_meshShape,
		b3_sphereShape,
		b3_shapeTypeCount
	} 

	struct b3ShapeDef
	{
		public char8* name;
		public void* userData;
		public b3SurfaceMaterial* materials;
		public int32 materialCount;
		public b3SurfaceMaterial baseMaterial;
		public float density;
		public float explosionScale;
		public b3Filter filter;
		public bool enableCustomFiltering;
		public bool isSensor;
		public bool enableSensorEvents;
		public bool enableContactEvents;
		public bool enableHitEvents;
		public bool enablePreSolveEvents;
		public bool invokeContactCreation;
		public bool updateBodyMass;
		public int32 internalValue;
	}

	struct b3ShapeId
	{
		int32 index1;
		uint16 world0;
		uint16 generation;
	}

	struct b3Sphere
	{
		public b3Vec3 center;
		public float radius;
	}

	struct b3Capsule
	{
		public b3Vec3 center1;
		public b3Vec3 center2;
		public float radius;
	}


	[Import(Box3dLib), CLink]
	public static extern b3ShapeId b3CreateSphereShape(b3BodyId bodyId, b3ShapeDef* def, b3Sphere* sphere);

	[Import(Box3dLib), CLink]
	public static extern b3ShapeId b3CreateCapsuleShape(b3BodyId bodyId, b3ShapeDef* def, b3Capsule* capsule);

	[Import(Box3dLib), CLink]
	public static extern void b3DestroyShape(b3ShapeId shapeId);

	[Import(Box3dLib), CLink]
	public static extern bool b3Shape_IsValid(b3ShapeId shapeId);

	[Import(Box3dLib), CLink]
	public static extern void b3Shape_SetSphere(b3ShapeId shapeId, b3Sphere* sphere);
}