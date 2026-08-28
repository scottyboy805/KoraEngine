using Box3d;
using internal KoraPlayer;
using static KoraPlayer.Physics.Extensions;

namespace KoraPlayer.Physics;

public abstract class Collider : Component
{
	// Private
	private b3WorldId worldId;
	private b3BodyId bodyId;
	private b3ShapeId shapeId;
	private bool isBodyOwned;

	[SerializeField("IsTrigger")]
	private bool isTrigger = false;

	// Constructor
	protected this()
	{
		worldId = Game.Instance.World.WorldId;
	}

	// Methods
	protected override void OnCreate()
	{
		GetOrCreateBody();
	}

	protected override void OnDestroy()
	{
		DestroyShape();
		DestroyBody();
	}

	protected override void OnEnable()
	{
		// Enable the body
		if(isBodyOwned == true)
			b3Body_Enable(bodyId);
	}

	protected override void OnDisable()
	{
		// Disable the body
		if(isBodyOwned == true)
			b3Body_Disable(bodyId);
	}

	protected abstract b3ShapeId CreateShape(b3BodyId body, ref b3ShapeDef shapeDef);

	private void GetOrCreateBody()
	{
		// Check for already created
		if(b3Body_IsValid(bodyId) == true)
			return;

		// Check for parent rigid body
		RigidBody parentBody = GameObject.GetComponentInParent<RigidBody>(true);

		// Check for any
		if(parentBody != null)
		{
			// Use parent body
			bodyId = parentBody.BodyId;
			isBodyOwned = false;
		}
		// Create static body
		else
		{
			// Create def
			b3BodyDef bodyDef = .
			{
				type = .b3_staticBody,
				position = default,
				rotation = default,
				linearVelocity = default,
				angularVelocity = default,
				linearDamping = 0.01f,
				angularDamping = 0.05f,
				gravityScale = 1f,
				sleepThreshold = 0.05f,
				name = null,
				userData = null,
				motionLocks = default,
				enableSleep = true,
				isAwake = false,
				isBullet = false,
				isEnabled = false,				// Will be enabled via OnEnable
				allowFastRotation = false,
				enableContactRecycling = true,
			};

			// Try to create
			bodyId = b3CreateBody(worldId, &bodyDef);

			// Check for valid
			if(b3Body_IsValid(bodyId) == false)
			{
				Debug.LogError("Physics body is not valid for static collider", .Physics);
				return;
			}

			isBodyOwned = true;
		}

		// Create shape def
		b3ShapeDef shapeDef = .
		{
			name = Name.ToScopeCStr!(),
			userData = Native,
			isSensor = isTrigger,
		};

		// Create the shape
		shapeId = CreateShape(bodyId, ref shapeDef);
	}

	private void DestroyBody()
	{
		// Check for created
		if(isBodyOwned == true && b3Body_IsValid(bodyId) == true)
		{
			b3DestroyBody(bodyId);
			bodyId = default;
		}
	}

	private void DestroyShape()
	{
		if(b3Shape_IsValid(shapeId) == true)
		{
			b3DestroyShape(shapeId);
			shapeId = default;
		}
	}
}