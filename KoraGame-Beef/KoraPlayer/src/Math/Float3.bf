using System;

namespace KoraPlayer;

[CRepr]
public struct float3
{
	// Public
	public float x, y, z;

	public const float3 Zero = float3(0f);
	public const float3 One = float3(1f);
	public const float3 Up = float3(0f, 1f, 0f);
	public const float3 Down = float3(0f, -1f, 0f);
	public const float3 Left = float3(-1f, 0f, 0f);
	public const float3 Right = float3(1f, 0f, 0f);
	public const float3 Forward = float3(0f, 0f, 1f);
	public const float3 Backward = float3(0f, 0f, -1f);

	// Properties
	public float Length
	{
		[Inline]
		get => Math.Sqrt(x * x + y * y + z * z);
	}

	public float LengthSqr
	{
		[Inline]
		get => x * x + y * y + z * z;
	}

	public float3 Normalized
	{
		[Inline]
		get
		{
			float3 local = this;
			local.Normalize();
			return local;
		}
	}

	public float3 XOnly
	{
		[Inline]
		get => float3(x, 0, 0);
	}

	public float3 YOnly
	{
		[Inline]
		get => float3(0, y, 0);
	}

	public float3 ZOnly
	{
		[Inline]
		get => float3(0, 0, z);
	}

	public float2 XY
	{
		[Inline]
		get => float2(x, y);
	}

	// Constructor
	public this(float value)
	{
		this.x = value;
		this.y = value;
		this.z = value;
	}

	public this(float x, float y, float z)
	{
		this.x = x;
		this.y = y;
		this.z = z;
	}

	public this(in float2 xy, float z)
	{
		this.x = xy.x;
		this.y = xy.y;
		this.z = z;
	}

	// Methods
	[Inline]
	public void Scale(in float3 scale) mut
	{
		x *= scale.x;
		y *= scale.y;
		z *= scale.z;
	}

	public void Normalize() mut
	{
		float mag = Length;
		if (mag > 1E-05f)
		{
		    float invMag = 1f / mag; // More efficient
		    x *= invMag;
		    y *= invMag;
		    z *= invMag;
		}
		else
		{
		    this = Zero;
		}
	}

	public static float Angle(in float3 from, in float3 to)
	{
		float mag = Math.Sqrt(from.LengthSqr * to.LengthSqr);
		if (mag < 1E-15f)
		    return 0f;

		float limit = Math.Max(-1f, Math.Min(1f, Dot(from, to) / mag)); // Use MathF
		return Math.Acos(limit) * (180f / Math.PI_f); // Direct conversion to degrees
	}

	[Inline]
	public static float Distance(in float3 a, in float3 b)
	{
		float x = a.x - b.x;
		float y = a.y - b.y;
		float z = a.z - b.z;
		return Math.Sqrt(x * x + y * y + z * z);
	}

	[Inline]
	public static float DistanceSqr(in float3 a, in float3 b)
	{
		float x = a.x - b.x;
		float y = a.y - b.y;
		float z = a.z - b.z;
		return x * x + y * y + z * z;
	}

	[Inline]
	public static float Dot(in float3 a, in float3 b)
	{
		return a.x * b.x + a.y * b.y + a.z * b.z;
	}

	[Inline]
	public static float3 Cross(in float3 a, in float3 b)
	{
		float3 result;
		result.x = a.y * b.z - a.z * b.y;
		result.y = a.z * b.x - a.x * b.z;
		result.z = a.x * b.y - a.y * b.x;
		return result;
	}

	// Operators
	public static float3 operator*(float3 lhs, float3 rhs)
		=> float3(lhs.x * rhs.x, lhs.y * rhs.y, lhs.z * rhs.z);
	public static float3 operator*(float3 lhs, float rhs)
		=> float3(lhs.x * rhs, lhs.y * rhs, lhs.z * rhs);
	public static float3 operator*(float lhs, float3 rhs)
		=> float3(lhs * rhs.x, lhs * rhs.y, lhs * rhs.z);


	public static float3 operator/(float3 lhs, float3 rhs)
		=> float3(lhs.x / rhs.x, lhs.y / rhs.y, lhs.z / rhs.z);
	public static float3 operator/(float3 lhs, float rhs)
		=> float3(lhs.x / rhs, lhs.y / rhs, lhs.z / rhs);
	public static float3 operator/(float lhs, float3 rhs)
		=> float3(lhs / rhs.x, lhs / rhs.y, lhs / rhs.z);


	public static float3 operator+(float3 lhs, float3 rhs)
		=> float3(lhs.x + rhs.x, lhs.y + rhs.y, lhs.z + rhs.z);

	public static float3 operator-(float3 lhs, float3 rhs)
		=> float3(lhs.x - rhs.x, lhs.y - rhs.y, lhs.z - rhs.z);
}