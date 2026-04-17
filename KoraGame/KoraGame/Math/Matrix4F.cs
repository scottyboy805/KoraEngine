using System.Runtime.InteropServices;

namespace KoraGame
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Matrix4F
    {
        // Public Fields
        public Vector4F C0;
        public Vector4F C1;
        public Vector4F C2;
        public Vector4F C3;

        public static readonly Matrix4F Identity = new Matrix4F(
            new Vector4F(1f, 0f, 0f, 0f),
            new Vector4F(0f, 1f, 0f, 0f),
            new Vector4F(0f, 0f, 1f, 0f),
            new Vector4F(0f, 0f, 0f, 1f)
        );

        // Properties
        public Vector3F Position => new Vector3F(C3.X, C3.Y, C3.Z);

        public QuaternionF Rotation
        {
            get
            {
                float m00 = C0.X;
                float m01 = C1.X;
                float m02 = C2.X;

                float m10 = C0.Y;
                float m11 = C1.Y;
                float m12 = C2.Y;

                float m20 = C0.Z;
                float m21 = C1.Z;
                float m22 = C2.Z;

                float trace = m00 + m11 + m22;

                QuaternionF q;

                if (trace > 0f)
                {
                    float s = MathF.Sqrt(trace + 1f) * 2f;
                    q = new QuaternionF((m21 - m12) / s, (m02 - m20) / s, (m10 - m01) / s, 0.25f * s);
                }
                else if (m00 > m11 && m00 > m22)
                {
                    float s = MathF.Sqrt(1f + m00 - m11 - m22) * 2f;
                    q = new QuaternionF(0.25f * s, (m01 + m10) / s, (m02 + m20) / s, (m21 - m12) / s);
                }
                else if (m11 > m22)
                {
                    float s = MathF.Sqrt(1f + m11 - m00 - m22) * 2f;
                    q = new QuaternionF((m01 + m10) / s, 0.25f * s, (m12 + m21) / s, (m02 - m20) / s);
                }
                else
                {
                    float s = MathF.Sqrt(1f + m22 - m00 - m11) * 2f;
                    q = new QuaternionF((m02 + m20) / s, (m12 + m21) / s, 0.25f * s, (m10 - m01) / s);
                }

                return q.Normalized;
            }
        }

        // Constructors
        public Matrix4F(Vector4F c0, Vector4F c1, Vector4F c2, Vector4F c3)
        {
            C0 = c0;
            C1 = c1;
            C2 = c2;
            C3 = c3;
        }

        // Static Factory Methods
        public static Matrix4F Orthographic(float left, float right, float bottom, float top, float near, float far)
        {
            float rml = right - left;
            float tmb = top - bottom;
            float fmn = far - near;

            float rpl = right + left;
            float tpb = top + bottom;
            float fpn = far + near;

            return new Matrix4F(
                new Vector4F(2f / rml, 0f, 0f, 0f),
                new Vector4F(0f, 2f / tmb, 0f, 0f),
                new Vector4F(0f, 0f, -2f / fmn, 0f),
                new Vector4F(-rpl / rml, -tpb / tmb, -fpn / fmn, 1f)
            );
        }

        public static Matrix4F Perspective(float fovDegrees, float aspect, float near, float far)
        {
            // Convert FOV from degrees to radians
            float fovRad = fovDegrees * MathF.PI / 180f;
            float f = 1f / MathF.Tan(fovRad * 0.5f);
            float rangeInv = 1f / (near - far);

            // FIXED: Correct Unity-compatible perspective matrix
            return new Matrix4F(
                new Vector4F(f / aspect, 0f, 0f, 0f),                    // Column 0: X scaling
                new Vector4F(0f, f, 0f, 0f),                             // Column 1: Y scaling  
                new Vector4F(0f, 0f, (far + near) * rangeInv, -1f),     // Column 2: Z mapping + perspective divide
                new Vector4F(0f, 0f, 2f * far * near * rangeInv, 0f)    // Column 3: Translation + Z offset
            );
        }

        public static Matrix4F Translate(Vector3F position) => new Matrix4F(
            new Vector4F(1f, 0f, 0f, 0f),
            new Vector4F(0f, 1f, 0f, 0f),
            new Vector4F(0f, 0f, 1f, 0f),
            new Vector4F(position.X, position.Y, position.Z, 1f)
        );

        public static Matrix4F RotateX(float degrees)
        {
            float rad = MathF.PI / 180f * degrees;
            float sin = MathF.Sin(rad);
            float cos = MathF.Cos(rad);

            return new Matrix4F(
                new Vector4F(1f, 0f, 0f, 0f),
                new Vector4F(0f, cos, sin, 0f),
                new Vector4F(0f, -sin, cos, 0f),
                new Vector4F(0f, 0f, 0f, 1f)
            );
        }

        public static Matrix4F RotateY(float degrees)
        {
            float rad = MathF.PI / 180f * degrees;
            float sin = MathF.Sin(rad);
            float cos = MathF.Cos(rad);

            return new Matrix4F(
                new Vector4F(cos, 0f, -sin, 0f),
                new Vector4F(0f, 1f, 0f, 0f),
                new Vector4F(sin, 0f, cos, 0f),
                new Vector4F(0f, 0f, 0f, 1f)
            );
        }

        public static Matrix4F RotateZ(float degrees)
        {
            float rad = MathF.PI / 180f * degrees;
            float sin = MathF.Sin(rad);
            float cos = MathF.Cos(rad);

            return new Matrix4F(
                new Vector4F(cos, sin, 0f, 0f),
                new Vector4F(-sin, cos, 0f, 0f),
                new Vector4F(0f, 0f, 1f, 0f),
                new Vector4F(0f, 0f, 0f, 1f)
            );
        }

        public static Matrix4F Rotate(QuaternionF rotation)
        {
            rotation = rotation.Normalized;

            float x = rotation.X;
            float y = rotation.Y;
            float z = rotation.Z;
            float w = rotation.W;

            float xx = x * x;
            float yy = y * y;
            float zz = z * z;

            float xy = x * y;
            float xz = x * z;
            float yz = y * z;

            float wx = w * x;
            float wy = w * y;
            float wz = w * z;

            return new Matrix4F(
                new Vector4F(1f - 2f * (yy + zz), 2f * (xy + wz), 2f * (xz - wy), 0f),
                new Vector4F(2f * (xy - wz), 1f - 2f * (xx + zz), 2f * (yz + wx), 0f),
                new Vector4F(2f * (xz + wy), 2f * (yz - wx), 1f - 2f * (xx + yy), 0f),
                new Vector4F(0f, 0f, 0f, 1f)
            );
        }

        public static Matrix4F Scale(Vector3F scale) => new Matrix4F(
            new Vector4F(scale.X, 0f, 0f, 0f),
            new Vector4F(0f, scale.Y, 0f, 0f),
            new Vector4F(0f, 0f, scale.Z, 0f),
            new Vector4F(0f, 0f, 0f, 1f)
        );

        public static Matrix4F Scale(float uniformScale) => new Matrix4F(
            new Vector4F(uniformScale, 0f, 0f, 0f),
            new Vector4F(0f, uniformScale, 0f, 0f),
            new Vector4F(0f, 0f, uniformScale, 0f),
            new Vector4F(0f, 0f, 0f, 1f)
        );

        public static Matrix4F TRS(Vector3F position, QuaternionF rotation, Vector3F scale)
        {
            // Ensure quaternion is normalized to prevent stretching
            rotation = rotation.Normalized;

            float x = rotation.X * 2f;
            float y = rotation.Y * 2f;
            float z = rotation.Z * 2f;

            float xx = rotation.X * x;
            float yy = rotation.Y * y;
            float zz = rotation.Z * z;

            float xy = rotation.X * y;
            float xz = rotation.X * z;
            float yz = rotation.Y * z;
            float wx = rotation.W * x;
            float wy = rotation.W * y;
            float wz = rotation.W * z;

            float sx = scale.X;
            float sy = scale.Y;
            float sz = scale.Z;

            return new Matrix4F(
                new Vector4F((1f - (yy + zz)) * sx, (xy + wz) * sx, (xz - wy) * sx, 0f),
                new Vector4F((xy - wz) * sy, (1f - (xx + zz)) * sy, (yz + wx) * sy, 0f),
                new Vector4F((xz + wy) * sz, (yz - wx) * sz, (1f - (xx + yy)) * sz, 0f),
                new Vector4F(position.X, position.Y, position.Z, 1f)
            );
        }

        public static Matrix4F Inverse(Matrix4F mat)
        {
            Vector3F r0 = new Vector3F(mat.C0.X, mat.C0.Y, mat.C0.Z);
            Vector3F r1 = new Vector3F(mat.C1.X, mat.C1.Y, mat.C1.Z);
            Vector3F r2 = new Vector3F(mat.C2.X, mat.C2.Y, mat.C2.Z);
            Vector3F t = new Vector3F(mat.C3.X, mat.C3.Y, mat.C3.Z);

            float det = r0.X * (r1.Y * r2.Z - r2.Y * r1.Z)
                      - r1.X * (r0.Y * r2.Z - r2.Y * r0.Z)
                      + r2.X * (r0.Y * r1.Z - r1.Y * r0.Z);

            if (MathF.Abs(det) < 1e-6f)
                return Identity;

            float invDet = 1f / det;

            Vector3F i0 = new Vector3F(
                (r1.Y * r2.Z - r2.Y * r1.Z) * invDet,
                (r2.Y * r0.Z - r0.Y * r2.Z) * invDet,
                (r0.Y * r1.Z - r1.Y * r0.Z) * invDet
            );
            Vector3F i1 = new Vector3F(
                (r2.X * r1.Z - r1.X * r2.Z) * invDet,
                (r0.X * r2.Z - r2.X * r0.Z) * invDet,
                (r1.X * r0.Z - r0.X * r1.Z) * invDet
            );
            Vector3F i2 = new Vector3F(
                (r1.X * r2.Y - r2.X * r1.Y) * invDet,
                (r2.X * r0.Y - r0.X * r2.Y) * invDet,
                (r0.X * r1.Y - r1.X * r0.Y) * invDet
            );

            Vector3F invT = new Vector3F(
                -(i0.X * t.X + i1.X * t.Y + i2.X * t.Z),
                -(i0.Y * t.X + i1.Y * t.Y + i2.Y * t.Z),
                -(i0.Z * t.X + i1.Z * t.Y + i2.Z * t.Z)
            );

            // UPDATED: Use Vector4F constructor instead of 16-float constructor
            return new Matrix4F(
                new Vector4F(i0.X, i0.Y, i0.Z, 0f),
                new Vector4F(i1.X, i1.Y, i1.Z, 0f),
                new Vector4F(i2.X, i2.Y, i2.Z, 0f),
                new Vector4F(invT.X, invT.Y, invT.Z, 1f)
            );
        }

        // Operators
        public static Matrix4F operator *(Matrix4F lhs, Matrix4F rhs)
        {
            // Calculate each column of the result matrix
            Vector4F c0 = new Vector4F(
                lhs.C0.X * rhs.C0.X + lhs.C1.X * rhs.C0.Y + lhs.C2.X * rhs.C0.Z + lhs.C3.X * rhs.C0.W,
                lhs.C0.Y * rhs.C0.X + lhs.C1.Y * rhs.C0.Y + lhs.C2.Y * rhs.C0.Z + lhs.C3.Y * rhs.C0.W,
                lhs.C0.Z * rhs.C0.X + lhs.C1.Z * rhs.C0.Y + lhs.C2.Z * rhs.C0.Z + lhs.C3.Z * rhs.C0.W,
                lhs.C0.W * rhs.C0.X + lhs.C1.W * rhs.C0.Y + lhs.C2.W * rhs.C0.Z + lhs.C3.W * rhs.C0.W
            );

            Vector4F c1 = new Vector4F(
                lhs.C0.X * rhs.C1.X + lhs.C1.X * rhs.C1.Y + lhs.C2.X * rhs.C1.Z + lhs.C3.X * rhs.C1.W,
                lhs.C0.Y * rhs.C1.X + lhs.C1.Y * rhs.C1.Y + lhs.C2.Y * rhs.C1.Z + lhs.C3.Y * rhs.C1.W,
                lhs.C0.Z * rhs.C1.X + lhs.C1.Z * rhs.C1.Y + lhs.C2.Z * rhs.C1.Z + lhs.C3.Z * rhs.C1.W,
                lhs.C0.W * rhs.C1.X + lhs.C1.W * rhs.C1.Y + lhs.C2.W * rhs.C1.Z + lhs.C3.W * rhs.C1.W
            );

            Vector4F c2 = new Vector4F(
                lhs.C0.X * rhs.C2.X + lhs.C1.X * rhs.C2.Y + lhs.C2.X * rhs.C2.Z + lhs.C3.X * rhs.C2.W,
                lhs.C0.Y * rhs.C2.X + lhs.C1.Y * rhs.C2.Y + lhs.C2.Y * rhs.C2.Z + lhs.C3.Y * rhs.C2.W,
                lhs.C0.Z * rhs.C2.X + lhs.C1.Z * rhs.C2.Y + lhs.C2.Z * rhs.C2.Z + lhs.C3.Z * rhs.C2.W,
                lhs.C0.W * rhs.C2.X + lhs.C1.W * rhs.C2.Y + lhs.C2.W * rhs.C2.Z + lhs.C3.W * rhs.C2.W
            );

            Vector4F c3 = new Vector4F(
                lhs.C0.X * rhs.C3.X + lhs.C1.X * rhs.C3.Y + lhs.C2.X * rhs.C3.Z + lhs.C3.X * rhs.C3.W,
                lhs.C0.Y * rhs.C3.X + lhs.C1.Y * rhs.C3.Y + lhs.C2.Y * rhs.C3.Z + lhs.C3.Y * rhs.C3.W,
                lhs.C0.Z * rhs.C3.X + lhs.C1.Z * rhs.C3.Y + lhs.C2.Z * rhs.C3.Z + lhs.C3.Z * rhs.C3.W,
                lhs.C0.W * rhs.C3.X + lhs.C1.W * rhs.C3.Y + lhs.C2.W * rhs.C3.Z + lhs.C3.W * rhs.C3.W
            );

            return new Matrix4F(c0, c1, c2, c3);
        }

        public static Vector3F operator *(Matrix4F lhs, Vector3F rhs)
        {
            Vector4F v = (Vector4F)rhs;

            return new Vector3F(
                lhs.C0.X * v.X + lhs.C1.X * v.Y + lhs.C2.X * v.Z + lhs.C3.X * v.W,
                lhs.C0.Y * v.X + lhs.C1.Y * v.Y + lhs.C2.Y * v.Z + lhs.C3.Y * v.W,
                lhs.C0.Z * v.X + lhs.C1.Z * v.Y + lhs.C2.Z * v.Z + lhs.C3.Z * v.W
            );
        }

        public static Vector4F operator *(Matrix4F lhs, Vector4F rhs)
        {
            return new Vector4F(
                lhs.C0.X * rhs.X + lhs.C1.X * rhs.Y + lhs.C2.X * rhs.Z + lhs.C3.X * rhs.W,
                lhs.C0.Y * rhs.X + lhs.C1.Y * rhs.Y + lhs.C2.Y * rhs.Z + lhs.C3.Y * rhs.W,
                lhs.C0.Z * rhs.X + lhs.C1.Z * rhs.Y + lhs.C2.Z * rhs.Z + lhs.C3.Z * rhs.W,
                lhs.C0.W * rhs.X + lhs.C1.W * rhs.Y + lhs.C2.W * rhs.Z + lhs.C3.W * rhs.W
            );
        }

        public static Matrix4F operator *(Matrix4F lhs, float rhs)
            => new Matrix4F(lhs.C0 * rhs, lhs.C1 * rhs, lhs.C2 * rhs, lhs.C3 * rhs);

        public static Matrix4F operator *(float lhs, Matrix4F rhs)
            => new Matrix4F(lhs * rhs.C0, lhs * rhs.C1, lhs * rhs.C2, lhs * rhs.C3);

        public static Matrix4F operator /(Matrix4F lhs, float rhs)
            => new Matrix4F(lhs.C0 / rhs, lhs.C1 / rhs, lhs.C2 / rhs, lhs.C3 / rhs);
    }
}