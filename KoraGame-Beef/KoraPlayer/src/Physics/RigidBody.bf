using Box3d;
using internal KoraPlayer;
using static KoraPlayer.Physics.Extensions;

namespace KoraPlayer.Physics;

public enum FreezeConstraint: uint32
{
	X = 1 << 0,
	Y = 1 << 1,
	Z = 1 << 2,
}

public sealed class RigidBody : Component
{
	// Private
	private b3WorldId worldId;
	private b3BodyId bodyId;

	// Serialize
	[SerializeField("IsKinematic")]
	private bool isKinematic = false;
	[SerializeField("FreezePosition")]
	private FreezeConstraint freezePosition = 0;
	[SerializeField("FreezeRotation")]
	private FreezeConstraint freezeRotation = 0;

	// Properties
	internal b3BodyId BodyId => bodyId;

	public float3 Position
	{
		get => b3Body_GetPosition(bodyId).ToKora();
	}

	public quaternion Rotation
	{
		get => b3Body_GetRotation(bodyId).ToKora();
	}

	public bool IsKinematic
	{
		get => isKinematic;
		set
		{
			isKinematic = value;
			b3Body_SetType(bodyId, isKinematic == false
				? .b3_dynamicBody
				: .b3_kinematicBody);
		}
	}

	public FreezeConstraint FreezePosition
	{
		get => freezePosition;
		set
		{
			freezePosition = value;
			b3Body_SetMotionLocks(bodyId, GetMotionLocks(freezePosition, freezePosition));
		}
	}

	public FreezeConstraint FreezeRotation
	{
		get => freezeRotation;
		set
		{
			freezeRotation = value;
			b3Body_SetMotionLocks(bodyId, GetMotionLocks(freezePosition, freezePosition));
		}
	}

	// Constructor
	public this()
	{
		worldId = Game.Instance.World.WorldId;
	}
	
	// Methods
	protected override void OnCreate()
	{
		CreateBody();
	}

	protected override void OnDestroy()
	{
		DestroyBody();
	}

	protected override void OnEnable()
	{
		// Enable the body
		b3Body_Enable(bodyId);
	}

	protected override void OnDisable()
	{
		// Disable the body
		b3Body_Disable(bodyId);
	}

	private void CreateBody()
	{
		// Check for already created
		if(b3Body_IsValid(bodyId) == true)
			return;

		// Create def
		b3BodyDef def = .
		{
			type = isKinematic == false
				? .b3_dynamicBody
				: .b3_kinematicBody,
			position = default,
			rotation = default,
			linearVelocity = default,
			angularVelocity = default,
			linearDamping = 0.01f,
			angularDamping = 0.05f,
			gravityScale = 1f,
			sleepThreshold = 0.05f,
			name = null,
			userData = Native,
			motionLocks = GetMotionLocks(freezePosition, freezeRotation),
			enableSleep = true,
			isAwake = false,
			isBullet = false,
			isEnabled = false,				// Will be enabled via OnEnable
			allowFastRotation = false,
			enableContactRecycling = true,
		};

		// Try to create
		bodyId = b3CreateBody(worldId, &def);

		// Check for valid
		if(b3Body_IsValid(bodyId) == false)
		{
			Debug.LogError("Physics body is not valid", .Physics);
			return;
		}
	}

	private void DestroyBody()
	{
		// Check for created
		if(b3Body_IsValid(bodyId) == true)
		{
			b3DestroyBody(bodyId);
			bodyId = default;
		}
	}

	private static b3MotionLocks GetMotionLocks(FreezeConstraint position, FreezeConstraint rotation)
	{
		b3MotionLocks locks = default;

		// Position
		if((position & .X) != 0) locks.linearX = true;
		if((position & .Y) != 0) locks.linearY = true;
		if((position & .Z) != 0) locks.linearZ = true;

		// Rotation
		if((rotation & .X) != 0) locks.angularX = true;
		if((rotation & .Y) != 0) locks.angularY = true;
		if((rotation & .Z) != 0) locks.angularZ = true;

		return locks;
	}
}