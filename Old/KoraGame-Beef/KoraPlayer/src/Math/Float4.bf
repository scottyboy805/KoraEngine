using System;

namespace KoraPlayer;

[CRepr]
public struct float4 : IHashable
{
	// Public
	public float x, y, z, w;

	public const float4 Zero = float4(0f, 1f);
	public const float4 One = float4(1f, 1f);
	public const float4 Up = float4(0f, 1f, 0f, 1f);
	public const float4 Down = float4(0f, -1f, 0f, 1f);
	public const float4 Left = float4(-1f, 0f, 0f, 1f);
	public const float4 Right = float4(1f, 0f, 0f, 1f);
	public const float4 Forward = float4(0f, 0f, 1f, 1f);
	public const float4 Backward = float4(0f, 0f, -1f, 1f);

	// Properties
	public float4 XOnly
	{
		[Inline]
		get => float4(x, 0, 0, 0);
	}

	public float4 YOnly
	{
		[Inline]
		get => float4(0, y, 0, 0);
	}

	public float4 ZOnly
	{
		[Inline]
		get => float4(0, 0, z, 0);
	}

	public float4 XWOnly
	{
		[Inline]
		get => float4(x, 0, 0, w);
	}

	public float4 YWOnly
	{
		[Inline]
		get => float4(0, y, 0, w);
	}

	public float4 ZWOnly
	{
		[Inline]
		get => float4(0, 0, z, w);
	}

	public float2 XY
	{
		[Inline]
		get => float2(x, y);
	}

	public float3 XYZ
	{
		[Inline]
		get => float3(x, y, z);
	}

	// Constructor
	public this(float value, float w)
	{
		this.x = value;
		this.y = value;
		this.z = value;
		this.w = w;
	}

	public this(float x, float y, float z, float w)
	{
		this.x = x;
		this.y = y;
		this.z = z;
		this.w = w;
	}

	public this(float3 xyz, float w)
	{
		this.x = xyz.x;
		this.y = xyz.y;
		this.z = xyz.z;
		this.w = w;
	}

	// Methods
	public int GetHashCode()
	{
		let xy = HashCode.Mix(
			HashCode.Generate(x),
			HashCode.Generate(y));

		let zw = HashCode.Mix(
			HashCode.Generate(z),
			HashCode.Generate(w));

		return HashCode.Mix(xy, zw);
	}

	// Operators
	public static float4 operator*(float4 lhs, float4 rhs)
		=> float4(lhs.x * rhs.x, lhs.y * rhs.y, lhs.z * rhs.z, lhs.w * rhs.w);
	public static float4 operator*(float4 lhs, float rhs)
		=> float4(lhs.x * rhs, lhs.y * rhs, lhs.z * rhs, lhs.w * rhs);
	public static float4 operator*(float lhs, float4 rhs)
		=> float4(lhs * rhs.x, lhs * rhs.y, lhs * rhs.z, lhs * rhs.w);


	public static float4 operator/(float4 lhs, float4 rhs)
		=> float4(lhs.x / rhs.x, lhs.y / rhs.y, lhs.z / rhs.z, lhs.w / rhs.w);
	public static float4 operator/(float4 lhs, float rhs)
		=> float4(lhs.x / rhs, lhs.y / rhs, lhs.z / rhs, lhs.w / rhs);
	public static float4 operator/(float lhs, float4 rhs)
		=> float4(lhs / rhs.x, lhs / rhs.y, lhs / rhs.z, lhs / rhs.w);


	public static float4 operator+(float4 lhs, float4 rhs)
		=> float4(lhs.x + rhs.x, lhs.y + rhs.y, lhs.z + rhs.z, lhs.w + rhs.w);

	public static float4 operator-(float4 lhs, float4 rhs)
		=> float4(lhs.x - rhs.x, lhs.y - rhs.y, lhs.z - rhs.z, lhs.w - rhs.w);
}