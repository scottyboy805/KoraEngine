using System;

namespace KoraPlayer;

public struct quaternion
{
	// Private
	private const float KEpsilon = 0.000001F;

	// Public
	public float x, y, z, w;

	public const quaternion Identity = quaternion(0f, 0f, 0f, 1f);

	// Properties
	public float3 EulerAngles
	{
		get => MakePositive(ToEulerRad(this)) * 57.2958f;	// Rad to deg
		set mut => this = FromEulerRad(value * (Math.PI_f / 180f)); // Deg to rad
	}

	public quaternion Normalized
	{
		[Inline]
		get
		{
			quaternion local;
			local.x = x;
			local.y = y;
			local.z = z;
			local.w = w;
			local.Normalize();
			return local;
		}
	}

	// Constructor
	public this(float value)
	{
		this.x = value;
		this.y = value;
		this.z = value;
		this.w = value;
	}

	public this(float x, float y, float z, float w)
	{
		this.x = x;
		this.y = y;
		this.z = z;
		this.w = w;
	}

	// Methods
	public void Normalize() mut
	{
	    float mag = Math.Sqrt(Dot(this, this));
	    if (mag > KEpsilon)
	    {
	        float invMag = 1f / mag;
	        x *= invMag;
	        y *= invMag;
	        z *= invMag;
	        w *= invMag;
	    }
	    else
	    {
	        this = Identity;
	    }
	}

	[Inline]
	public static float Dot(in quaternion a, in quaternion b)
	{
	    return a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w;
	}

	public static quaternion Inverse(in quaternion val)
	{
		quaternion result;
		float num2 = (((val.x * val.x) + (val.y * val.y)) + (val.z * val.z)) + (val.w * val.w);
		float num = 1f / num2;
		result.x = -val.x * num;
		result.y = -val.y * num;
		result.z = -val.z * num;
		result.w = val.w * num;
		return result;
	}

	public static quaternion Euler(in float3 euler)
	{
		return FromEulerRad(euler * (Math.PI_f / 180f)); // Deg to rad
	}

	private static float3 ToEulerRad(quaternion rotation)
	{
	    // Ensure quaternion is normalized
		var rotation;
	    float sqw = rotation.w * rotation.w;
	    float sqx = rotation.x * rotation.x;
	    float sqy = rotation.y * rotation.y;
	    float sqz = rotation.z * rotation.z;
	    float unit = sqx + sqy + sqz + sqw;

	    // Normalize if needed
	    if (unit > 1f + KEpsilon || unit < 1f - KEpsilon)
	    {
	        float invUnit = 1f / Math.Sqrt(unit);
	        rotation.x *= invUnit;
	        rotation.y *= invUnit;
	        rotation.z *= invUnit;
	        rotation.w *= invUnit;
	        sqw = rotation.w * rotation.w;
	        sqx = rotation.x * rotation.x;
	        sqy = rotation.y * rotation.y;
	        sqz = rotation.z * rotation.z;
	        unit = sqx + sqy + sqz + sqw;
	    }

	    float test = rotation.x * rotation.y + rotation.z * rotation.w;
	    float3 v;

	    if (test > 0.4995f * unit)
	    { // singularity at north pole
	        v.y = 2f * Math.Atan2(rotation.x, rotation.w);
	        v.z = Math.PI_f / 2;
	        v.x = 0;
	        return NormalizeAngles(v);
	    }
	    if (test < -0.4995f * unit)
	    { // singularity at south pole
	        v.y = -2f * Math.Atan2(rotation.x, rotation.w);
	        v.z = -Math.PI_f / 2;
	        v.x = 0;
	        return NormalizeAngles(v);
	    }

	    v.y = Math.Atan2(2f * rotation.y * rotation.w - 2f * rotation.x * rotation.z, sqx - sqy - sqz + sqw);
	    v.z = Math.Asin(2f * test / unit);
	    v.x = Math.Atan2(2f * rotation.x * rotation.w - 2f * rotation.y * rotation.z, -sqx + sqy - sqz + sqw);
	    return NormalizeAngles(v);
	}

	private static quaternion FromEulerRad(float3 euler)
	{
		float cx = Math.Cos(euler.x * 0.5f);
		float sx = Math.Sin(euler.x * 0.5f);
		float cy = Math.Cos(euler.y * 0.5f);
		float sy = Math.Sin(euler.y * 0.5f);
		float cz = Math.Cos(euler.z * 0.5f);
		float sz = Math.Sin(euler.z * 0.5f);

		quaternion result;
		result.w = cx * cy * cz + sx * sy * sz;
		result.x = sx * cy * cz - cx * sy * sz;
		result.y = cx * sy * cz + sx * cy * sz;
		result.z = cx * cy * sz - sx * sy * cz;
		return result;
	}

	private static float3 NormalizeAngles(float3 angles)
	{
		var angles;
	    angles.x = NormalizeAngle(angles.x);
	    angles.y = NormalizeAngle(angles.y);
	    angles.z = NormalizeAngle(angles.z);
	    return angles;
	}

	private static float NormalizeAngle(float angle)
	{
		var angle;
	    while (angle > Math.PI_f)
	        angle -= 2 * Math.PI_f;
	    while (angle < -Math.PI_f)
	        angle += 2 * Math.PI_f;
	    return angle;
	}

	private static float3 MakePositive(float3 euler)
	{
		var euler;
	    float negativeFlip = -0.0001f;
	    float positiveFlip = 2 * Math.PI_f + negativeFlip;

	    if (euler.x < negativeFlip)
	        euler.x += 2 * Math.PI_f;
	    else if (euler.x > positiveFlip)
	        euler.x -= 2 * Math.PI_f;

	    if (euler.y < negativeFlip)
	        euler.y += 2 * Math.PI_f;
	    else if (euler.y > positiveFlip)
	        euler.y -= 2 * Math.PI_f;

	    if (euler.z < negativeFlip)
	        euler.z += 2 * Math.PI_f;
	    else if (euler.z > positiveFlip)
	        euler.z -= 2 * Math.PI_f;

	    return euler;
	}

	// Operators
	public static quaternion operator* (quaternion lhs, quaternion rhs)
		=> quaternion(lhs.w * rhs.x + lhs.x * rhs.w + lhs.y * rhs.z - lhs.z * rhs.y, lhs.w * rhs.y + lhs.y * rhs.w + lhs.z * rhs.x - lhs.x * rhs.z, lhs.w * rhs.z + lhs.z * rhs.w + lhs.x * rhs.y - lhs.y * rhs.x, lhs.w * rhs.w - lhs.x * rhs.x - lhs.y * rhs.y - lhs.z * rhs.z);
}