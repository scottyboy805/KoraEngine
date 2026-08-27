using Box3d;
using internal KoraPlayer;
using static KoraPlayer.Physics.Extensions;

namespace KoraPlayer.Physics;

public sealed class SphereCollider : Collider
{
	// Private
	private b3ShapeId sphereShapeId;

	[SerializeField("Radius")]
	private float radius = 0.5f;

	// Properties
	public float Radius
	{
		get => radius;
		set
		{
			radius = value;

			// Create the sphere info
			b3Sphere sphere = .
			{
				center = GameObject.WorldTransform.TransformPoint(float3.Zero).ToBoxVec3(),
				radius = radius,
			};

			// Update the existing shape
			b3Shape_SetSphere(sphereShapeId, &sphere);
		}
	}

	// Methods
	protected override b3ShapeId CreateShape(b3BodyId bodyId, ref b3ShapeDef shapeDef)
	{
		// Create the sphere info
		b3Sphere sphere = .
		{
			center = GameObject.WorldTransform.TransformPoint(float3.Zero).ToBoxVec3(),
			radius = radius,
		};

		// Create the shape attached to body
		sphereShapeId = b3CreateSphereShape(bodyId, &shapeDef, &sphere);
		return sphereShapeId;
	}
}