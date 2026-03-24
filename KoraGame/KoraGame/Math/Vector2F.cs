using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace KoraGame
{
    public struct Vector2F : IEquatable<Vector2F>
    {
        // Public
        public static readonly Vector2F Zero = new Vector2F();
        public static readonly Vector2F One = new Vector2F(1f);
        public static readonly Vector2F Up = new Vector2F(0f, 1f);
        public static readonly Vector2F Down = new Vector2F(0f, -1f);
        public static readonly Vector2F Left = new Vector2F(-1f, 0f);
        public static readonly Vector2F Right = new Vector2F(1f, 0f);

        [DataMember]
        public float X;
        [DataMember]
        public float Y;

        // Properties        
        public float Magnitude
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return (float)Math.Sqrt(X * X + Y * Y); }
        }

        public float SqrMagnitude
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return X * X + Y * Y; }
        }

        public Vector2F Normalized
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                Vector2F result;
                result.X = X;
                result.Y = Y;
                result.Normalize();
                return result;
            }
        }

        public Vector2F XOnly
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Vector2F(X, 0f); }
        }

        public Vector2F YOnly
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Vector2F(0f, Y); }
        }

        // Constructor
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2F(float value)
        {
            this.X = value;
            this.Y = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2F(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }

        // Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Scale(in Vector2F scale)
        {
            X *= scale.X;
            Y *= scale.Y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Normalize()
        {
            float mag = Magnitude;

            if (mag > 1E-05f)
            {
                X /= mag;
                Y /= mag;
            }
            else
            {
                this = Zero;
            }
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public Point2 RoundToPoint()
        //{
        //    Point2 result;
        //    result.X = (int)Math.Round(X);
        //    result.Y = (int)Math.Round(Y);
        //    return result;
        //}

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public Point3 RoundToPoint3()
        //{
        //    Point3 result;
        //    result.X = (int)Math.Round(X);
        //    result.Y = (int)Math.Round(Y);
        //    result.Z = 0;
        //    return result;
        //}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillArray(float[] arr, int offset, int count = 2)
        {
            if (count > 0) arr[offset] = X;
            if (count > 1) arr[offset + 1] = Y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Angle(in Vector2F from, in Vector2F to)
        {
            float mag = (float)Math.Sqrt(from.SqrMagnitude * to.SqrMagnitude);
            if (mag < 1E-15f)
                return 0f;

            float limit = Math.Clamp(Dot(from, to) / mag, -1f, 1f);
            return (float)Math.Acos(limit) * Math.RadToDeg;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Distance(in Vector2F a, in Vector2F b)
        {
            float x = a.X - b.X;
            float y = a.Y - b.Y;
            return (float)Math.Sqrt(x * x + y * y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DistanceSqr(in Vector2F a, in Vector2F b)
        {
            float x = a.X - b.X;
            float y = a.Y - b.Y;
            return x * x + y * y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dot(in Vector2F a, in Vector2F b)
        {
            return a.X * b.Y + a.Y * b.Y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2F Lerp(in Vector2F a, in Vector2F b, float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            Vector2F result = new Vector2F();
            result.X = a.X + (b.X - a.X) * t;
            result.Y = a.Y + (b.Y - a.Y) * t;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2F LerpExtrap(in Vector2F a, in Vector2F b, float t)
        {
            Vector2F result;
            result.X = a.X + (b.X - a.X) * t;
            result.Y = a.Y + (b.Y - a.Y) * t;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2F MoveTowards(in Vector2F a, in Vector2F b, float maxDelta)
        {
            float x = b.X - a.X;
            float y = b.Y - a.Y;
            float magSqr = x * x + y * y;
            if (magSqr == 0f || (maxDelta >= 0f && magSqr <= maxDelta * maxDelta))
                return b;

            float mag = (float)Math.Sqrt(magSqr);

            Vector2F result;
            result.X = a.X + x / mag * maxDelta;
            result.Y = a.Y + y / mag * maxDelta;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2F Max(in Vector2F a, in Vector2F b)
        {
            Vector2F result;
            result.X = Math.Max(a.X, b.X);
            result.Y = Math.Max(a.Y, b.Y);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2F Min(in Vector2F a, in Vector2F b)
        {
            Vector2F result;
            result.X = Math.Min(a.X, b.X);
            result.Y = Math.Min(a.Y, b.Y);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2F Reflect(in Vector2F direction, in Vector2F normal)
        {
            float dot = -2f * Dot(normal, direction);

            Vector2F result;
            result.X = dot * normal.X + direction.X;
            result.Y = dot * normal.Y + direction.Y;
            return result;
        }

        public static Vector2F FromArray(float[] arr, int offset, int count = 2)
        {
            Vector2F result = new Vector2F();
            if (count > 0) result.X = arr[offset];
            if (count > 1) result.Y = arr[offset + 1];
            return result;
        }

        #region Object Overrides
        public override string ToString()
        {
            return string.Format("({0}, {1})", X, Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            if (obj is Vector2F)
                return Equals((Vector2F)obj);

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Vector2F other)
        {
            return X == other.X && Y == other.Y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() << 4;
        }
        #endregion

        #region Operators
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2F operator +(in Vector2F a, in Vector2F b)
        {
            Vector2F result;
            result.X = a.X + b.X;
            result.Y = a.Y + b.Y;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2F operator -(in Vector2F a, in Vector2F b)
        {
            Vector2F result;
            result.X = a.X - b.X;
            result.Y = a.Y - b.Y;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2F operator *(in Vector2F a, in Vector2F b)
        {
            Vector2F result;
            result.X = a.X * b.X;
            result.Y = a.Y * b.Y;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2F operator /(in Vector2F a, in Vector2F b)
        {
            Vector2F result;
            result.X = a.X / b.X;
            result.Y = a.Y / b.Y;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2F operator -(in Vector2F a)
        {
            Vector2F result;
            result.X = -a.X;
            result.Y = -a.Y;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2F operator *(in Vector2F a, float val)
        {
            Vector2F result;
            result.X = a.X * val;
            result.Y = a.Y * val;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2F operator *(float val, in Vector2F a)
        {
            Vector2F result;
            result.X = a.X * val;
            result.Y = a.Y * val;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2F operator /(in Vector2F a, float val)
        {
            Vector2F result;
            result.X = a.X / val;
            result.Y = a.Y / val;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in Vector2F a, in Vector2F b)
        {
            float x = a.X - b.X;
            float y = a.Y - b.Y;
            return x * x + y * y < 9.99999944E-11f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in Vector2F a, in Vector2F b)
        {
            return !(a == b);
        }
        #endregion

        #region Conversion
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public static explicit operator Point2(Vector2F vector)
        //{
        //    Point2 result;
        //    result.X = (int)vector.X;
        //    result.Y = (int)vector.Y;
        //    return result;
        //}

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public static explicit operator Point3(Vector2F vector)
        //{
        //    Point3 result;
        //    result.X = (int)vector.X;
        //    result.Y = (int)vector.Y;
        //    result.Z = 0;
        //    return result;
        //}

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public static explicit operator Point4(Vector2F vector)
        //{
        //    Point4 result;
        //    result.X = (int)vector.X;
        //    result.Y = (int)vector.Y;
        //    result.Z = 0;
        //    result.W = 0;
        //    return result;
        //}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Vector2(Vector2F vector)
        {
            Vector2 result;
            result.X = vector.X;
            result.Y = vector.Y;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Vector3(Vector2F vector)
        {
            Vector3 result;
            result.X = vector.X;
            result.Y = vector.Y;
            result.Z = 0f;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Vector4(Vector2F vector)
        {
            Vector4 result;
            result.X = vector.X;
            result.Y = vector.Y;
            result.Z = 0f;
            result.W = 0f;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Vector2F(Vector2 vector)
        {
            Vector2F result;
            result.X = vector.X;
            result.Y = vector.Y;
            return result;
        }
        #endregion
    }
}
