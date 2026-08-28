using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace KoraGame
{
    public struct Vector4F : IEquatable<Vector4F>
    {
        // Public
        public static readonly Vector4F Zero = new Vector4F();
        public static readonly Vector4F One = new Vector4F(1f);
        public static readonly Vector4F Up = new Vector4F(0f, 1f, 0f, 1f);
        public static readonly Vector4F Down = new Vector4F(0f, -1f, 0f, 1f);
        public static readonly Vector4F Left = new Vector4F(-1f, 0f, 0f, 1f);
        public static readonly Vector4F Right = new Vector4F(1f, 0f, 0f, 1f);
        public static readonly Vector4F Forward = new Vector4F(0f, 0f, 1f, 1f);
        public static readonly Vector4F Backward = new Vector4F(0f, 0f, -1f, 1f);

        [DataMember]
        public float X;
        [DataMember]
        public float Y;
        [DataMember]
        public float Z;
        [DataMember]
        public float W;

        // Properties
        public float Magnitude
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return MathF.Sqrt(X * X + Y * Y + Z * Z + W * W); }
        }

        public float SqrMagnitude
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return X * X + Y * Y + Z * Z + W * W; }
        }

        public Vector4F Normalized
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                Vector4F result;
                result.X = X;
                result.Y = Y;
                result.Z = Z;
                result.W = W;
                result.Normalize();
                return result;
            }
        }

        public Vector4F XOnly
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Vector4F(X, 0f, 0f, 0f); }
        }

        public Vector4F YOnly
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Vector4F(0f, Y, 0f, 0f); }
        }

        public Vector4F ZOnly
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Vector4F(0f, 0f, Z, 0f); }
        }

        public Vector2F XY
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new Vector2F(X, Y);
        }

        public Vector3F XYZ
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new Vector3F(X, Y, Z);
        }

        // Constructor
        public Vector4F(float value)
        {
            this.X = value;
            this.Y = value;
            this.Z = value;
            this.W = value; // Fixed: Unity behavior sets all components to value
        }

        public Vector4F(float x, float y, float z, float w)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = w;
        }

        // Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Scale(in Vector4F scale)
        {
            X *= scale.X;
            Y *= scale.Y;
            Z *= scale.Z;
            W *= scale.W;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Normalize()
        {
            float mag = Magnitude;
            if (mag > 1E-05f)
            {
                float invMag = 1f / mag;
                X *= invMag;
                Y *= invMag;
                Z *= invMag;
                W *= invMag;
            }
            else
            {
                this = Zero;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillArray(float[] arr, int offset, int count = 4)
        {
            if (count > 0) arr[offset] = X;
            if (count > 1) arr[offset + 1] = Y;
            if (count > 2) arr[offset + 2] = Z;
            if (count > 3) arr[offset + 3] = W;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Distance(in Vector4F a, in Vector4F b) // Fixed parameter types
        {
            float x = a.X - b.X;
            float y = a.Y - b.Y;
            float z = a.Z - b.Z;
            float w = a.W - b.W; // Fixed: was using a.X - b.X again
            return MathF.Sqrt(x * x + y * y + z * z + w * w);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DistanceSqr(in Vector4F a, in Vector4F b) // Fixed parameter types
        {
            float x = a.X - b.X;
            float y = a.Y - b.Y;
            float z = a.Z - b.Z;
            float w = a.W - b.W; // Fixed: was using a.X - b.X again
            return x * x + y * y + z * z + w * w;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dot(in Vector4F a, in Vector4F b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z + a.W * b.W;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4F Lerp(in Vector4F a, in Vector4F b, float t)
        {
            t = MathF.Max(0f, MathF.Min(1f, t)); // Use MathF instead of Math
            Vector4F result;
            result.X = a.X + (b.X - a.X) * t;
            result.Y = a.Y + (b.Y - a.Y) * t;
            result.Z = a.Z + (b.Z - a.Z) * t;
            result.W = a.W + (b.W - a.W) * t;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4F LerpExtrap(in Vector4F a, in Vector4F b, float t)
        {
            Vector4F result;
            result.X = a.X + (b.X - a.X) * t;
            result.Y = a.Y + (b.Y - a.Y) * t;
            result.Z = a.Z + (b.Z - a.Z) * t;
            result.W = a.W + (b.W - a.W) * t;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4F MoveTowards(in Vector4F a, in Vector4F b, float maxDelta)
        {
            float x = b.X - a.X;
            float y = b.Y - a.Y;
            float z = b.Z - a.Z;
            float w = b.W - a.W;
            float magSqr = x * x + y * y + z * z + w * w;
            if (magSqr == 0f || (maxDelta >= 0f && magSqr <= maxDelta * maxDelta))
                return b;

            float mag = MathF.Sqrt(magSqr);

            Vector4F result;
            result.X = a.X + x / mag * maxDelta;
            result.Y = a.Y + y / mag * maxDelta;
            result.Z = a.Z + z / mag * maxDelta;
            result.W = a.W + w / mag * maxDelta;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4F Max(in Vector4F a, in Vector4F b)
        {
            Vector4F result;
            result.X = MathF.Max(a.X, b.X);
            result.Y = MathF.Max(a.Y, b.Y);
            result.Z = MathF.Max(a.Z, b.Z);
            result.W = MathF.Max(a.W, b.W);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4F Min(in Vector4F a, in Vector4F b)
        {
            Vector4F result;
            result.X = MathF.Min(a.X, b.X);
            result.Y = MathF.Min(a.Y, b.Y);
            result.Z = MathF.Min(a.Z, b.Z);
            result.W = MathF.Min(a.W, b.W);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4F Reflect(in Vector4F direction, in Vector4F normal)
        {
            float dot = -2f * Dot(normal, direction);

            Vector4F result;
            result.X = dot * normal.X + direction.X;
            result.Y = dot * normal.Y + direction.Y;
            result.Z = dot * normal.Z + direction.Z;
            result.W = dot * normal.W + direction.W;
            return result;
        }

        public static Vector4F FromArray(float[] arr, int offset, int count = 4) // Fixed default count
        {
            Vector4F result = new Vector4F();
            if (count > 0) result.X = arr[offset];
            if (count > 1) result.Y = arr[offset + 1];
            if (count > 2) result.Z = arr[offset + 2];
            if (count > 3) result.W = arr[offset + 3];
            return result;
        }

        #region Object Overrides
        public override string ToString()
        {
            return string.Format("({0}, {1}, {2}, {3})", X, Y, Z, W);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            if (obj is Vector4F)
                return Equals((Vector4F)obj);

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Vector4F other)
        {
            return X == other.X && Y == other.Y && Z == other.Z && W == other.W;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode() ^ W.GetHashCode() << 4;
        }
        #endregion

        #region Operators
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4F operator +(in Vector4F a, in Vector4F b)
        {
            Vector4F result;
            result.X = a.X + b.X;
            result.Y = a.Y + b.Y;
            result.Z = a.Z + b.Z;
            result.W = a.W + b.W;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4F operator -(in Vector4F a, in Vector4F b)
        {
            Vector4F result;
            result.X = a.X - b.X;
            result.Y = a.Y - b.Y;
            result.Z = a.Z - b.Z;
            result.W = a.W - b.W;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4F operator *(in Vector4F a, in Vector4F b)
        {
            Vector4F result;
            result.X = a.X * b.X;
            result.Y = a.Y * b.Y;
            result.Z = a.Z * b.Z;
            result.W = a.W * b.W;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4F operator /(in Vector4F a, in Vector4F b)
        {
            Vector4F result;
            result.X = a.X / b.X;
            result.Y = a.Y / b.Y;
            result.Z = a.Z / b.Z;
            result.W = a.W / b.W;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4F operator -(in Vector4F a)
        {
            Vector4F result;
            result.X = -a.X;
            result.Y = -a.Y;
            result.Z = -a.Z;
            result.W = -a.W;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4F operator *(in Vector4F a, float val)
        {
            Vector4F result;
            result.X = a.X * val;
            result.Y = a.Y * val;
            result.Z = a.Z * val;
            result.W = a.W * val;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4F operator *(float val, in Vector4F a)
        {
            Vector4F result;
            result.X = a.X * val;
            result.Y = a.Y * val;
            result.Z = a.Z * val;
            result.W = a.W * val;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4F operator /(in Vector4F a, float val)
        {
            Vector4F result;
            result.X = a.X / val;
            result.Y = a.Y / val;
            result.Z = a.Z / val;
            result.W = a.W / val;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in Vector4F a, in Vector4F b)
        {
            float x = a.X - b.X;
            float y = a.Y - b.Y;
            float z = a.Z - b.Z;
            float w = a.W - b.W;
            return x * x + y * y + z * z + w * w < 9.99999944E-11f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in Vector4F a, in Vector4F b)
        {
            return !(a == b);
        }
        #endregion

        #region Conversion
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Vector2(Vector4F vector)
        {
            Vector2 result;
            result.X = vector.X;
            result.Y = vector.Y;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Vector3(Vector4F vector)
        {
            Vector3 result;
            result.X = vector.X;
            result.Y = vector.Y;
            result.Z = vector.Z;
            return result;
        }
        #endregion
    }
}