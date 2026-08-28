using System;
namespace KoraPlayer;

[CRepr]
public struct matrix4
{
	// Public
	public float4 c0;
	public float4 c1;
	public float4 c2;
	public float4 c3;

	public const matrix4 Identity = matrix4(
		float4(1f, 0f, 0f, 0f),
		float4(0f, 1f, 0f, 0f),
		float4(0f, 0f, 1f, 0f),
		float4(0f, 0f, 0f, 1f));

	// Properties
	public float3 Position
	{
		[Inline]
		get => float3(c3.x, c3.y, c3.z);
	}

	public quaternion Rotation
	{
		get
		{
			float m00 = c0.x;
			float m01 = c1.x;
			float m02 = c2.x;

			float m10 = c0.y;
			float m11 = c1.y;
			float m12 = c2.y;

			float m20 = c0.z;
			float m21 = c1.z;
			float m22 = c2.z;

			float trace = m00 + m11 + m22;

			quaternion q;

			if (trace > 0f)
			{
			    float s = Math.Sqrt(trace + 1f) * 2f;
			    q = quaternion((m21 - m12) / s, (m02 - m20) / s, (m10 - m01) / s, 0.25f * s);
			}
			else if (m00 > m11 && m00 > m22)
			{
			    float s = Math.Sqrt(1f + m00 - m11 - m22) * 2f;
			    q = quaternion(0.25f * s, (m01 + m10) / s, (m02 + m20) / s, (m21 - m12) / s);
			}
			else if (m11 > m22)
			{
			    float s = Math.Sqrt(1f + m11 - m00 - m22) * 2f;
			    q = quaternion((m01 + m10) / s, 0.25f * s, (m12 + m21) / s, (m02 - m20) / s);
			}
			else
			{
			    float s = Math.Sqrt(1f + m22 - m00 - m11) * 2f;
			    q = quaternion((m02 + m20) / s, (m12 + m21) / s, 0.25f * s, (m10 - m01) / s);
			}

			return q.Normalized;
		}
	}

	// Constructor
	public this(float4 c0, float4 c1, float4 c2, float4 c3)
	{
		this.c0 = c0;
		this.c1 = c1;
		this.c2 = c2;
		this.c3 = c3;
	}

	public this(float m00, float m01, float m02, float m03,
                float m10, float m11, float m12, float m13,
                float m20, float m21, float m22, float m23,
                float m30, float m31, float m32, float m33)
    {
        this.c0 = float4(m00, m10, m20, m30);
		this.c1 = float4(m01, m11, m21, m31);
		this.c2 = float4(m02, m12, m22, m32);
		this.c3 = float4(m03, m13, m23, m33);
    }

	// Methods
	public static matrix4 Orthographic(float left, float right, float bottom, float top, float near, float far)
	{
		float rml = right - left;
		float tmb = top - bottom;
		float fmn = far - near;

		float rpl = right + left;
		float tpb = top + bottom;
		float fpn = far + near;

		return matrix4(
		    float4(2f / rml, 0f, 0f, 0f),
		    float4(0f, 2f / tmb, 0f, 0f),
		    float4(0f, 0f, -2f / fmn, 0f),
		   	float4(-rpl / rml, -tpb / tmb, -fpn / fmn, 1f)
		);
	}

	public static matrix4 Perspective(float fovDegrees, float aspect, float near, float far)
	{
		float fovRad = Math.DegreesToRadians(fovDegrees);
		float f = 1f / Math.Tan(fovRad * 0.5f);
		float rangeInv = 1f / (near - far);

		return matrix4(
	        float4(f / aspect, 0f, 0f, 0f),
	        float4(0f, f, 0f, 0f),
	        float4(0f, 0f, (far + near) * rangeInv, 2f * far * near * rangeInv),
	        float4(0f, 0f, -1f, 0f)
	    );
	}

	public static matrix4 Translate(float3 position) => matrix4(
		float4(1f, 0f, 0f, 0f),
		float4(0f, 1f, 0f, 0f),
		float4(0f, 0f, 1f, 0f),
		float4(position.x, position.y, position.z, 1f));

	public static matrix4 RotateX(float degrees)
	{
		float rad = Math.DegreesToRadians(degrees);
		float sin = Math.Sin(rad);
		float cos = Math.Cos(rad);

		return matrix4(
			float4(1f, 0f, 0f, 0f),
			float4(0f, cos, -sin, 0f),
			float4(0f, sin, cos, 0f),
			float4(0f, 0f, 0f, 1f));
	}

	public static matrix4 RotateY(float degrees)
	{
		float rad = Math.DegreesToRadians(degrees);
		float sin = Math.Sin(rad);
		float cos = Math.Cos(rad);

		return matrix4(
			float4(cos, 0f, sin, 0f),
			float4(0f, 0f, 0f, 0f),
			float4(-sin, 0f, cos, 0f),
			float4(0f, 0f, 0f, 1f));
	}

	public static matrix4 RotateZ(float degrees)
	{
		float rad = Math.DegreesToRadians(degrees);
		float sin = Math.Sin(rad);
		float cos = Math.Cos(rad);

		return matrix4(
			float4(cos, -sin, 0f, 0f),
			float4(sin, cos, 0f, 0f),
			float4(0f, 0f, 0f, 0f),
			float4(0f, 0f, 0f, 1f));
	}

	public static matrix4 Scale(float3 scale) => matrix4(
		float4(scale.x, 0f, 0f, 0f),
		float4(0f, scale.y, 0f, 0f),
		float4(0f, 0f, scale.z, 0f),
		float4(0f, 0f, 0f, 1f));

	public static matrix4 Scale(float uniformScale) => matrix4(
		float4(uniformScale, 0f, 0f, 0f),
		float4(0f, uniformScale, 0f, 0f),
		float4(0f, 0f, uniformScale, 0f),
		float4(0f, 0f, 0f, 1f));

	public static matrix4 TRS(float3 position, quaternion rotation, float3 scale)
	{
		float x = rotation.x * 2f;
		float y = rotation.y * 2f;
		float z = rotation.z * 2f;

		float xx = rotation.x * x;
		float yy = rotation.y * y;
		float zz = rotation.z * z;

		float xy = rotation.x * y;
		float xz = rotation.x * z;
		float yz = rotation.y * z;
		float wx = rotation.w * x;
		float wy = rotation.w * y;
		float wz = rotation.w * z;

		float sx = scale.x;
		float sy = scale.y;
		float sz = scale.z;

		return matrix4(
		    float4((1f - (yy + zz)) * sx, (xy + wz) * sx, (xz - wy) * sx, 0f),
		    float4((xy - wz) * sy, (1f - (xx + zz)) * sy, (yz + wx) * sy, 0f),
		    float4((xz + wy) * sz, (yz - wx) * sz, (1f - (xx + yy)) * sz, 0f),
		    float4(position.x, position.y, position.z, 1f)
		);
	}

	public static matrix4 Inverse(matrix4 mat)
	{
		// Extract rotation-scale 3x3 matrix (upper-left)
		float3 r0 = float3(mat.c0.x, mat.c0.y, mat.c0.z);
		float3 r1 = float3(mat.c1.x, mat.c1.y, mat.c1.z);
		float3 r2 = float3(mat.c2.x, mat.c2.y, mat.c2.z);

		// Extract translation
		float3 t = float3(mat.c3.x, mat.c3.y, mat.c3.z);

		// Compute inverse 3x3 rotation-scale matrix
		// For rotation only, this is just the transpose
		// For TRS with uniform scaling, divide by squared scale if necessary
		// We'll assume general scale: compute inverse via adjugate/determinant per 3x3
		float det = r0.x * (r1.y * r2.z - r2.y * r1.z)
		          - r1.x * (r0.y * r2.z - r2.y * r0.z)
		          + r2.x * (r0.y * r1.z - r1.y * r0.z);

		if (Math.Abs(det) < 1e-6f)
		    return matrix4.Identity; // Non-invertible, fallback

		float invDet = 1f / det;

		// Compute adjugate of 3x3
		float3 i0 = float3(
		    (r1.y * r2.z - r2.y * r1.z) * invDet,
		    (r2.y * r0.z - r0.y * r2.z) * invDet,
		    (r0.y * r1.z - r1.y * r0.z) * invDet
		);
		float3 i1 = float3(
		    (r2.x * r1.z - r1.x * r2.z) * invDet,
		    (r0.x * r2.z - r2.x * r0.z) * invDet,
		    (r1.x * r0.z - r0.x * r1.z) * invDet
		);
		float3 i2 = float3(
		    (r1.x * r2.y - r2.x * r1.y) * invDet,
		    (r2.x * r0.y - r0.x * r2.y) * invDet,
		    (r0.x * r1.y - r1.x * r0.y) * invDet
		);

		// Compute inverse translation
		float3 invT = float3(
		    -(i0.x * t.x + i1.x * t.y + i2.x * t.z),
		    -(i0.y * t.x + i1.y * t.y + i2.y * t.z),
		    -(i0.z * t.x + i1.z * t.y + i2.z * t.z)
		);

		// Construct inverse matrix
		return matrix4(
		    i0.x, i1.x, i2.x, 0f,
		    i0.y, i1.y, i2.y, 0f,
		    i0.z, i1.z, i2.z, 0f,
		    invT.x, invT.y, invT.z, 1f
		);
	}

	// Operators
	public static matrix4 operator* (matrix4 lhs, matrix4 rhs)
	{
		return matrix4(
	        // Column 0
	        lhs.c0.x * rhs.c0.x + lhs.c1.x * rhs.c0.y + lhs.c2.x * rhs.c0.z + lhs.c3.x * rhs.c0.w,
	        lhs.c0.y * rhs.c0.x + lhs.c1.y * rhs.c0.y + lhs.c2.y * rhs.c0.z + lhs.c3.y * rhs.c0.w,
	        lhs.c0.z * rhs.c0.x + lhs.c1.z * rhs.c0.y + lhs.c2.z * rhs.c0.z + lhs.c3.z * rhs.c0.w,
	        lhs.c0.w * rhs.c0.x + lhs.c1.w * rhs.c0.y + lhs.c2.w * rhs.c0.z + lhs.c3.w * rhs.c0.w,
	
	        // Column 1
	        lhs.c0.x * rhs.c1.x + lhs.c1.x * rhs.c1.y + lhs.c2.x * rhs.c1.z + lhs.c3.x * rhs.c1.w,
	        lhs.c0.y * rhs.c1.x + lhs.c1.y * rhs.c1.y + lhs.c2.y * rhs.c1.z + lhs.c3.y * rhs.c1.w,
	        lhs.c0.z * rhs.c1.x + lhs.c1.z * rhs.c1.y + lhs.c2.z * rhs.c1.z + lhs.c3.z * rhs.c1.w,
	        lhs.c0.w * rhs.c1.x + lhs.c1.w * rhs.c1.y + lhs.c2.w * rhs.c1.z + lhs.c3.w * rhs.c1.w,
	
	        // Column 2
	        lhs.c0.x * rhs.c2.x + lhs.c1.x * rhs.c2.y + lhs.c2.x * rhs.c2.z + lhs.c3.x * rhs.c2.w,
	        lhs.c0.y * rhs.c2.x + lhs.c1.y * rhs.c2.y + lhs.c2.y * rhs.c2.z + lhs.c3.y * rhs.c2.w,
	        lhs.c0.z * rhs.c2.x + lhs.c1.z * rhs.c2.y + lhs.c2.z * rhs.c2.z + lhs.c3.z * rhs.c2.w,
	        lhs.c0.w * rhs.c2.x + lhs.c1.w * rhs.c2.y + lhs.c2.w * rhs.c2.z + lhs.c3.w * rhs.c2.w,
	
	        // Column 3
	        lhs.c0.x * rhs.c3.x + lhs.c1.x * rhs.c3.y + lhs.c2.x * rhs.c3.z + lhs.c3.x * rhs.c3.w,
	        lhs.c0.y * rhs.c3.x + lhs.c1.y * rhs.c3.y + lhs.c2.y * rhs.c3.z + lhs.c3.y * rhs.c3.w,
	        lhs.c0.z * rhs.c3.x + lhs.c1.z * rhs.c3.y + lhs.c2.z * rhs.c3.z + lhs.c3.z * rhs.c3.w,
	        lhs.c0.w * rhs.c3.x + lhs.c1.w * rhs.c3.y + lhs.c2.w * rhs.c3.z + lhs.c3.w * rhs.c3.w
	    );
	}

	public static float3 operator* (matrix4 lhs, float3 rhs)
	{
		float4 v = float4(rhs, 1);
		return float3(
                lhs.c0.x * v.x + lhs.c1.x * v.y + lhs.c2.x * v.z + lhs.c3.x * v.w,
                lhs.c0.y * v.x + lhs.c1.y * v.y + lhs.c2.y * v.z + lhs.c3.y * v.w,
                lhs.c0.z * v.x + lhs.c1.z * v.y + lhs.c2.z * v.z + lhs.c3.z * v.w
            );
	}

	public static float4 operator* (matrix4 lhs, float4 rhs)
    {
        return float4(
            lhs.c0.x * rhs.x + lhs.c1.x * rhs.y + lhs.c2.x * rhs.z + lhs.c3.x * rhs.w,
            lhs.c0.y * rhs.x + lhs.c1.y * rhs.y + lhs.c2.y * rhs.z + lhs.c3.y * rhs.w,
            lhs.c0.z * rhs.x + lhs.c1.z * rhs.y + lhs.c2.z * rhs.z + lhs.c3.z * rhs.w,
            lhs.c0.w * rhs.x + lhs.c1.w * rhs.y + lhs.c2.w * rhs.z + lhs.c3.w * rhs.w
        );
    }

	public static matrix4 operator* (matrix4 lhs, float rhs)
		=> matrix4(lhs.c0 * rhs, lhs.c1 * rhs, lhs.c2 * rhs, lhs.c3 * rhs);
	public static matrix4 operator* (float lhs, matrix4 rhs)
		=> matrix4(lhs * rhs.c0, lhs * rhs.c1, lhs * rhs.c2, lhs * rhs.c3);

	public static matrix4 operator/ (matrix4 lhs, matrix4 rhs)
		=> matrix4(lhs.c0 / rhs.c0, lhs.c1 / rhs.c1, lhs.c2 / rhs.c2, lhs.c3 / rhs.c3);
	public static matrix4 operator/ (matrix4 lhs, float rhs)
		=> matrix4(lhs.c0 / rhs, lhs.c1 / rhs, lhs.c2 / rhs, lhs.c3 / rhs);
	public static matrix4 operator/ (float lhs, matrix4 rhs)
		=> matrix4(lhs / rhs.c0, lhs / rhs.c1, lhs / rhs.c2, lhs / rhs.c3);

}