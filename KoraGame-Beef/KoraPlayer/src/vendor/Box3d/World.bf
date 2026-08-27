using System;

namespace Box3d;

static
{
	typealias b3FrictionCallback = function float(float frictionA, uint64 userMaterialIdA, float frictionB, uint64 userMaterialIdB);
	typealias b3RestitutionCallback = function float(float restitutionA, uint64 userMaterialIdA, float restitutionB, uint64 userMaterialIdB);

	typealias b3TaskCallback = function void(void* taskContext);
	typealias b3EnqueueTaskCallback = function void*(b3TaskCallback* task, void* taskContext, void* userContext, char8* taskName);
	typealias b3FinishTaskCallback = function void(void* userTask, void* userContext);
	typealias b3CreateDebugShapeCallback = function void*(b3DebugShape* debugShape, void* userContext);
	typealias b3DestroyDebugShapeCallback = function void(void* userShape, void* userContext);

	[CRepr]
	struct b3DebugShape
	{
		public b3ShapeId shapeId;
		public b3ShapeType type;
		public void* data;
	}

	[CRepr]
	struct b3Capacity
	{
		public int32 staticShapeCount;
		public int32 dynamicShapeCount;
		public int32 staticBodyCount;
		public int32 dynamicBodyCount;
		public int32 contactCount;
	} 

	[CRepr]
	struct b3WorldDef
	{
		// Public
		public b3Vec3 gravity;
		public float restitutionThreshold;
		public float hitEventThreshold;
		public float contactHertz;
		public float contactDampingRatio;
		public float contactSpeed;
		public float maximumLinearSpeed;
		public b3FrictionCallback* frictionCallback;
		public b3RestitutionCallback* restitutionCallback;
		public bool enableSleep;
		public bool enableContinuous;
		public uint32 workerCount;
		public b3EnqueueTaskCallback* enqueueTask;
		public b3FinishTaskCallback* finishTask;
		public void* userTaskContext;
		public void* userData;
		public b3CreateDebugShapeCallback* createDebugShape;
		public b3DestroyDebugShapeCallback* destroyDebugShape;
		public void* userDebugShapeContext;
		public b3Capacity capacity;
		public int32 internalValue;
	}

	[CRepr]
	struct b3WorldId
	{
		uint16 index1;
		uint16 generation;
	}

	[Import(Box3dLib), CLink]
	public static extern b3WorldId b3CreateWorld(b3WorldDef* def);

	[Import(Box3dLib), CLink]
	public static extern void b3DestroyWorld(b3WorldId world);

	[Import(Box3dLib), CLink]
	public static extern bool b3World_IsValid(b3WorldId world);

	[Import(Box3dLib), CLink]
	public static extern void b3World_Step(b3WorldId world, float timeStep, int32 subStepCount);
}