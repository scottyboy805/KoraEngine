using Box3d;
using static KoraPlayer.Physics.Extensions;

namespace KoraPlayer.Physics;

public sealed class World
{
	// Private
	private b3WorldId worldId;

	// Properties
	internal b3WorldId WorldId => worldId;

	// Constructor
	public this()
	{
		Initialize();
	}

	public ~this()
	{
		b3DestroyWorld(worldId);
		worldId = default;
	}

	// Methods
	private void Initialize()
	{
		// Create def
		b3WorldDef worldDef = .
		{
			gravity = float3(0, -9.81f, 0).ToBoxVec3(),
			restitutionThreshold = 1f,
			hitEventThreshold = 1f,
			contactHertz = 60,
			contactDampingRatio = 1f,
			contactSpeed = 1f,
			maximumLinearSpeed = 200f,
			enableSleep = true,
			enableContinuous = true,
			workerCount = 1,
		};
		
		// Create the world
		worldId = b3CreateWorld(&worldDef);

		// Check for valid
		if(b3World_IsValid(worldId) == false)
		{
			Debug.LogError("Physics world is not valid", .Physics);
			return;
		}
	}
}