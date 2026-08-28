namespace KoraPlayer;

public struct Transform
{
	// Private
	private matrix4? parentMatrix;

	[SerializeField("Position")]
	private float3 position;
	[SerializeField("Rotation")]
	private quaternion rotation;
	[SerializeField("Scale")]
	private float3 scale;

	// Public
	public static readonly Transform Identity = .{ position = float3.Zero, rotation = quaternion.Identity, scale = float3.One };

	// Properties
	public matrix4 TransformMatrix
	{
		get
		{
			// Create this matrix
			matrix4 mat = matrix4.TRS(position, rotation, scale);

			// Check for parent
			if(parentMatrix != null)
				mat = parentMatrix.Value * mat;

			return mat;
		}
	}

	public matrix4 InverseTransformMatrix
	{
		get
		{
			// Get the transformation
			matrix4 mat = TransformMatrix;

			// Invert the matrix
			return matrix4.Inverse(mat);
		}
	}

	public float3 Position
	{
		get
		{
			// Get transformed position
			if(parentMatrix != null)
				return parentMatrix.Value * position;

			// Get local position
			return position;
		}
	}

	public quaternion Rotation
	{
		get
		{
			// Get transformed rotation
			if(parentMatrix != null)
				return parentMatrix.Value.Rotation * rotation;

			// Get local rotation
			return rotation;
		}
	}

	public float3 EulerRotation
	{
		get => Rotation.EulerAngles;
	}

	public float3 Scale
	{
		get
		{
			if (parentMatrix != null)
			{
			    matrix4 m = parentMatrix.Value;

			    float3 parentScale = float3(
			        float3(m.c0.x, m.c0.y, m.c0.z).Length,
			        float3(m.c1.x, m.c1.y, m.c1.z).Length,
			        float3(m.c2.x, m.c2.y, m.c2.z).Length
			    );

			    return float3(
			        scale.x * parentScale.x,
			        scale.y * parentScale.y,
			        scale.z * parentScale.z
			    );
			}

			return scale;
		}
	}

	public float3 Forward
	{
		get => TransformDirection(float3.Forward);
	}

	public float3 Up
	{
		get => TransformDirection(float3.Up);
	}

	public float3 Right
	{
		get => TransformDirection(float3.Right);
	}

	// Constructor
	public this(float3 position, quaternion rotation, float3 scale)
	{
		this.position = position;
		this.rotation = rotation;
		this.scale = scale;
		this.parentMatrix = null;
	}

	public this(matrix4 matrix, float3 position, quaternion rotation, float3 scale)
	{
		this.parentMatrix = matrix;
		this.position = position;
		this.rotation = rotation;
		this.scale = scale;
	}

	// Methods
	public float3 TransformPoint(in float3 point)
	{
		float4 v = float4(point, 1);
		return (TransformMatrix * v).XYZ;
	}

	public float3 InverseTransformPoint(in float3 point)
	{
		float4 v = float4(point, 1);
		return (InverseTransformMatrix * v).XYZ;
	}

	public float3 TransformDirection(in float3 direction)
	{
		float4 v = float4(direction, 0);
		return (TransformMatrix * v).XYZ;
	}
}