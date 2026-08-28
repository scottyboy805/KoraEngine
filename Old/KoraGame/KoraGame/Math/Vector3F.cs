using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace KoraGame
{
    public struct Vector3F : IEquatable<Vector3F>
    {
        // Public
        public static readonly Vector3F Zero = new Vector3F();
        public static readonly Vector3F One = new Vector3F(1f);
        public static readonly Vector3F Up = new Vector3F(0f, 1f, 0f);
        public static readonly Vector3F Down = new Vector3F(0f, -1f, 0f);
        public static readonly Vector3F Left = new Vector3F(-1f, 0f, 0f);
        public static readonly Vector3F Right = new Vector3F(1f, 0f, 0f);
        public static readonly Vector3F Forward = new Vector3F(0f, 0f, 1f);
        public static readonly Vector3F Backward = new Vector3F(0f, 0f, -1f);

        [DataMember]
        public float X;
        [DataMember]
        public float Y;
        [DataMember]
        public float Z;

        // Properties
        public float Magnitude
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return MathF.Sqrt(X * X + Y * Y + Z * Z); } // Use MathF for consistency
        }

        public float SqrMagnitude
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return X * X + Y * Y + Z * Z; }
        }

        public Vector3F Normalized
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                Vector3F result;
                result.X = X;
                result.Y = Y;
                result.Z = Z;
                result.Normalize();
                return result;
            }
        }

        public Vector3F XOnly
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Vector3F(X, 0f, 0f); }
        }

        public Vector3F YOnly
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Vector3F(0f, Y, 0f); }
        }

        public Vector3F ZOnly
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Vector3F(0f, 0f, Z); }
        }

        public Vector2F XY
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new Vector2F(X, Y);
        }

        // Constructor
        public Vector3F(float value)
        {
            this.X = value;
            this.Y = value;
            this.Z = value;
        }

        public Vector3F(float x, float y)
        {
            this.X = x;
            this.Y = y;
            this.Z = 0f;
        }

        public Vector3F(float x, float y, float z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        // Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Scale(in Vector3F scale)
        {
            X *= scale.X;
            Y *= scale.Y;
            Z *= scale.Z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Normalize()
        {
            float mag = Magnitude;
            if (mag > 1E-05f)
            {
                float invMag = 1f / mag; // More efficient
                X *= invMag;
                Y *= invMag;
                Z *= invMag;
            }
            else
            {
                this = Zero;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillArray(float[] arr, int offset, int count = 3)
        {
            if (count > 0) arr[offset] = X;
            if (count > 1) arr[offset + 1] = Y;
            if (count > 2) arr[offset + 2] = Z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Jitter2.LinearMath.JVector Jitter()
        {
            return new Jitter2.LinearMath.JVector(X, Y, Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Angle(in Vector3F from, in Vector3F to)
        {
            float mag = MathF.Sqrt(from.SqrMagnitude * to.SqrMagnitude);
            if (mag < 1E-15f)
                return 0f;

            float limit = MathF.Max(-1f, MathF.Min(1f, Dot(from, to) / mag)); // Use MathF
            return MathF.Acos(limit) * (180f / MathF.PI); // Direct conversion to degrees
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Distance(in Vector3F a, in Vector3F b)
        {
            float x = a.X - b.X;
            float y = a.Y - b.Y;
            float z = a.Z - b.Z;
            return MathF.Sqrt(x * x + y * y + z * z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DistanceSqr(in Vector3F a, in Vector3F b)
        {
            float x = a.X - b.X;
            float y = a.Y - b.Y;
            float z = a.Z - b.Z;
            return x * x + y * y + z * z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dot(in Vector3F a, in Vector3F b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }

        // CRITICAL: Missing Cross Product for Unity compatibility
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3F Cross(in Vector3F a, in Vector3F b)
        {
            Vector3F result;
            result.X = a.Y * b.Z - a.Z * b.Y;
            result.Y = a.Z * b.X - a.X * b.Z;
            result.Z = a.X * b.Y - a.Y * b.X;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3F Lerp(in Vector3F a, in Vector3F b, float t)
        {
            t = MathF.Max(0f, MathF.Min(1f, t)); // Use MathF
            Vector3F result;
            result.X = a.X + (b.X - a.X) * t;
            result.Y = a.Y + (b.Y - a.Y) * t;
            result.Z = a.Z + (b.Z - a.Z) * t;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3F LerpExtrap(in Vector3F a, in Vector3F b, float t)
        {
            Vector3F result;
            result.X = a.X + (b.X - a.X) * t;
            result.Y = a.Y + (b.Y - a.Y) * t;
            result.Z = a.Z + (b.Z - a.Z) * t;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3F MoveTowards(in Vector3F a, in Vector3F b, float maxDelta)
        {
            float x = b.X - a.X;
            float y = b.Y - a.Y;
            float z = b.Z - a.Z;
            float magSqr = x * x + y * y + z * z;
            if (magSqr == 0f || (maxDelta >= 0f && magSqr <= maxDelta * maxDelta))
                return b;

            float mag = MathF.Sqrt(magSqr);

            Vector3F result;
            result.X = a.X + x / mag * maxDelta;
            result.Y = a.Y + y / mag * maxDelta;
            result.Z = a.Z + z / mag * maxDelta;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3F Max(in Vector3F a, in Vector3F b)
        {
            Vector3F result;
            result.X = MathF.Max(a.X, b.X);
            result.Y = MathF.Max(a.Y, b.Y);
            result.Z = MathF.Max(a.Z, b.Z);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3F Min(in Vector3F a, in Vector3F b)
        {
            Vector3F result;
            result.X = MathF.Min(a.X, b.X);
            result.Y = MathF.Min(a.Y, b.Y);
            result.Z = MathF.Min(a.Z, b.Z);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3F Reflect(in Vector3F direction, in Vector3F normal)
        {
            float dot = -2f * Dot(normal, direction);

            Vector3F result;
            result.X = dot * normal.X + direction.X;
            result.Y = dot * normal.Y + direction.Y;
            result.Z = dot * normal.Z + direction.Z;
            return result;
        }

        public static Vector3F FromArray(float[] arr, int offset, int count = 3)
        {
            Vector3F result = new Vector3F();
            if (count > 0) result.X = arr[offset];
            if (count > 1) result.Y = arr[offset + 1];
            if (count > 2) result.Z = arr[offset + 2];
            return result;
        }

        #region Object Overrides
        public override string ToString()
        {
            return string.Format("({0}, {1}, {2})", X, Y, Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            if (obj is Vector3F)
                return Equals((Vector3F)obj);

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Vector3F other)
        {
            return X == other.X && Y == other.Y && Z == other.Z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode() << 4;
        }
        #endregion

        #region Operators
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3F operator +(in Vector3F a, in Vector3F b)
        {
            Vector3F result;
            result.X = a.X + b.X;
            result.Y = a.Y + b.Y;
            result.Z = a.Z + b.Z;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3F operator -(in Vector3F a, in Vector3F b)
        {
            Vector3F result;
            result.X = a.X - b.X;
            result.Y = a.Y - b.Y;
            result.Z = a.Z - b.Z;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3F operator *(in Vector3F a, in Vector3F b)
        {
            Vector3F result;
            result.X = a.X * b.X;
            result.Y = a.Y * b.Y;
            result.Z = a.Z * b.Z;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3F operator /(in Vector3F a, in Vector3F b)
        {
            Vector3F result;
            result.X = a.X / b.X;
            result.Y = a.Y / b.Y;
            result.Z = a.Z / b.Z;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3F operator -(in Vector3F a)
        {
            Vector3F result;
            result.X = -a.X;
            result.Y = -a.Y;
            result.Z = -a.Z;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3F operator *(in Vector3F a, float val)
        {
            Vector3F result;
            result.X = a.X * val;
            result.Y = a.Y * val;
            result.Z = a.Z * val;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3F operator *(float val, in Vector3F a)
        {
            Vector3F result;
            result.X = a.X * val;
            result.Y = a.Y * val;
            result.Z = a.Z * val;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3F operator /(in Vector3F a, float val)
        {
            Vector3F result;
            result.X = a.X / val;
            result.Y = a.Y / val;
            result.Z = a.Z / val;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in Vector3F a, in Vector3F b)
        {
            float x = a.X - b.X;
            float y = a.Y - b.Y;
            float z = a.Z - b.Z;
            return x * x + y * y + z * z < 9.99999944E-11f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in Vector3F a, in Vector3F b)
        {
            return !(a == b);
        }
        #endregion

        #region Conversion
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Vector2(Vector3F vector)
        {
            Vector2 result;
            result.X = vector.X;
            result.Y = vector.Y;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Vector4F(Vector3F vector)
        {
            Vector4F result;
            result.X = vector.X;
            result.Y = vector.Y;
            result.Z = vector.Z;
            result.W = 1f;
            return result;
        }
        #endregion
    }
}