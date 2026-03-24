using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace KoraGame
{
    public struct QuaternionF
    {
        // Private
        private const float KEpsilon = 0.000001F;

        // Public
        public static readonly QuaternionF Identity = new QuaternionF(0f, 0f, 0f, 1f);

        [DataMember]
        public float X;
        [DataMember]
        public float Y;
        [DataMember]
        public float Z;
        [DataMember]
        public float W;

        // Properties
        public Vector3F EulerAngles
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return MakePositive(ToEulerRad(this) * Math.RadToDeg);
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                this = FromEulerRad(value * Math.DegToRad);
            }
        }

        public QuaternionF Normalized
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                QuaternionF result;
                result.X = X;
                result.Y = Y;
                result.Z = Z;
                result.W = W;
                result.Normalize();
                return result;
            }
        }

        public float Magnitude
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return MathF.Sqrt(Dot(this, this)); }
        }

        // Constructor
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public QuaternionF(float x, float y, float z, float w)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = w;
        }

        // Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Normalize()
        {
            float mag = MathF.Sqrt(Dot(this, this));
            if (mag > KEpsilon)
            {
                float invMag = 1f / mag;
                X *= invMag;
                Y *= invMag;
                Z *= invMag;
                W *= invMag;
            }
            else
            {
                this = Identity;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Jitter2.LinearMath.JQuaternion Jitter()
        {
            return new Jitter2.LinearMath.JQuaternion(X, Y, Z, W);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static QuaternionF Inverse(QuaternionF quaternion)
        {
            QuaternionF result;
            float num2 = (((quaternion.X * quaternion.X) + (quaternion.Y * quaternion.Y)) + (quaternion.Z * quaternion.Z)) + (quaternion.W * quaternion.W);
            float num = 1f / num2;
            result.X = -quaternion.X * num;
            result.Y = -quaternion.Y * num;
            result.Z = -quaternion.Z * num;
            result.W = quaternion.W * num;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static QuaternionF Euler(float x, float y, float z)
        {
            return FromEulerRad(new Vector3F(x, y, z) * Math.DegToRad);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static QuaternionF Euler(Vector3F euler)
        {
            return FromEulerRad(euler * Math.DegToRad);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Angle(QuaternionF a, QuaternionF b)
        {
            float dot = MathF.Min(MathF.Abs(Dot(a, b)), 1.0F);
            return IsEqualUsingDot(dot) ? 0.0f : MathF.Acos(dot) * 2.0F * Math.RadToDeg;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ToAngleAxis(out float angle, out Vector3F axis)
        {
            ToAxisAngleRad(this, out axis, out angle);
            angle *= Math.RadToDeg;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dot(QuaternionF a, QuaternionF b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z + a.W * b.W;
        }

        private static Vector3F ToEulerRad(QuaternionF rotation)
        {
            // Ensure quaternion is normalized
            float sqw = rotation.W * rotation.W;
            float sqx = rotation.X * rotation.X;
            float sqy = rotation.Y * rotation.Y;
            float sqz = rotation.Z * rotation.Z;
            float unit = sqx + sqy + sqz + sqw;

            // Normalize if needed
            if (unit > 1f + KEpsilon || unit < 1f - KEpsilon)
            {
                float invUnit = 1f / MathF.Sqrt(unit);
                rotation.X *= invUnit;
                rotation.Y *= invUnit;
                rotation.Z *= invUnit;
                rotation.W *= invUnit;
                sqw = rotation.W * rotation.W;
                sqx = rotation.X * rotation.X;
                sqy = rotation.Y * rotation.Y;
                sqz = rotation.Z * rotation.Z;
                unit = sqx + sqy + sqz + sqw;
            }

            float test = rotation.X * rotation.Y + rotation.Z * rotation.W;
            Vector3F v;

            if (test > 0.4995f * unit)
            { // singularity at north pole
                v.Y = 2f * MathF.Atan2(rotation.X, rotation.W);
                v.Z = MathF.PI / 2;
                v.X = 0;
                return NormalizeAngles(v);
            }
            if (test < -0.4995f * unit)
            { // singularity at south pole
                v.Y = -2f * MathF.Atan2(rotation.X, rotation.W);
                v.Z = -MathF.PI / 2;
                v.X = 0;
                return NormalizeAngles(v);
            }

            v.Y = MathF.Atan2(2f * rotation.Y * rotation.W - 2f * rotation.X * rotation.Z, sqx - sqy - sqz + sqw);
            v.Z = MathF.Asin(2f * test / unit);
            v.X = MathF.Atan2(2f * rotation.X * rotation.W - 2f * rotation.Y * rotation.Z, -sqx + sqy - sqz + sqw);
            return NormalizeAngles(v);
        }

        private static QuaternionF FromEulerRad(Vector3F euler)
        {
            float cx = MathF.Cos(euler.X * 0.5f);
            float sx = MathF.Sin(euler.X * 0.5f);
            float cy = MathF.Cos(euler.Y * 0.5f);
            float sy = MathF.Sin(euler.Y * 0.5f);
            float cz = MathF.Cos(euler.Z * 0.5f);
            float sz = MathF.Sin(euler.Z * 0.5f);

            QuaternionF result;
            result.W = cx * cy * cz + sx * sy * sz;
            result.X = sx * cy * cz - cx * sy * sz;
            result.Y = cx * sy * cz + sx * cy * sz;
            result.Z = cx * cy * sz - sx * sy * cz;
            return result;
        }

        private static void ToAxisAngleRad(QuaternionF q, out Vector3F axis, out float angle)
        {
            if (Math.Abs(q.W) > 1.0f)
                q.Normalize();

            angle = 2.0f * MathF.Acos(MathF.Abs(q.W)); // angle
            float den = MathF.Sqrt(1.0f - q.W * q.W);
            if (den > 0.0001f)
            {
                axis = new Vector3F(q.X, q.Y, q.Z) / den;
            }
            else
            {
                // This occurs when the angle is zero. 
                // Not a problem: just set an arbitrary normalized axis.
                axis = new Vector3F(1, 0, 0);
            }
        }

        private static Vector3F NormalizeAngles(Vector3F angles)
        {
            angles.X = NormalizeAngle(angles.X);
            angles.Y = NormalizeAngle(angles.Y);
            angles.Z = NormalizeAngle(angles.Z);
            return angles;
        }

        private static float NormalizeAngle(float angle)
        {
            while (angle > MathF.PI)
                angle -= 2 * MathF.PI;
            while (angle < -MathF.PI)
                angle += 2 * MathF.PI;
            return angle;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsEqualUsingDot(float dot)
        {
            // Returns false in the presence of NaN values.
            return dot > 1.0f - KEpsilon;
        }

        private static Vector3F MakePositive(Vector3F euler)
        {
            float negativeFlip = -0.0001f;
            float positiveFlip = 2 * MathF.PI + negativeFlip;

            if (euler.X < negativeFlip)
                euler.X += 2 * MathF.PI;
            else if (euler.X > positiveFlip)
                euler.X -= 2 * MathF.PI;

            if (euler.Y < negativeFlip)
                euler.Y += 2 * MathF.PI;
            else if (euler.Y > positiveFlip)
                euler.Y -= 2 * MathF.PI;

            if (euler.Z < negativeFlip)
                euler.Z += 2 * MathF.PI;
            else if (euler.Z > positiveFlip)
                euler.Z -= 2 * MathF.PI;

            return euler;
        }

        #region Object Overrides
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return X.GetHashCode() ^ (Y.GetHashCode() << 2) ^ (Z.GetHashCode() >> 2) ^ (W.GetHashCode() >> 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object other)
        {
            if (!(other is QuaternionF)) return false;

            return Equals((QuaternionF)other);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(QuaternionF other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z) && W.Equals(other.W);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
        {
            return ToString(null, null);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(string format)
        {
            return ToString(format, null);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (string.IsNullOrEmpty(format))
                format = "F5";
            if (formatProvider == null)
                formatProvider = CultureInfo.InvariantCulture.NumberFormat;
            return string.Format("({0}, {1}, {2}, {3})", X.ToString(format, formatProvider), Y.ToString(format, formatProvider), Z.ToString(format, formatProvider), W.ToString(format, formatProvider));
        }
        #endregion

        #region Operators
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(QuaternionF lhs, QuaternionF rhs)
        {
            return IsEqualUsingDot(Dot(lhs, rhs));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(QuaternionF lhs, QuaternionF rhs)
        {
            // Returns true in the presence of NaN values.
            return !(lhs == rhs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static QuaternionF operator *(QuaternionF lhs, QuaternionF rhs)
        {
            return new QuaternionF(
                lhs.W * rhs.X + lhs.X * rhs.W + lhs.Y * rhs.Z - lhs.Z * rhs.Y,
                lhs.W * rhs.Y + lhs.Y * rhs.W + lhs.Z * rhs.X - lhs.X * rhs.Z,
                lhs.W * rhs.Z + lhs.Z * rhs.W + lhs.X * rhs.Y - lhs.Y * rhs.X,
                lhs.W * rhs.W - lhs.X * rhs.X - lhs.Y * rhs.Y - lhs.Z * rhs.Z);
        }

        public static Vector3F operator *(QuaternionF rotation, Vector3F point)
        {
            float x = rotation.X * 2F;
            float y = rotation.Y * 2F;
            float z = rotation.Z * 2F;
            float xx = rotation.X * x;
            float yy = rotation.Y * y;
            float zz = rotation.Z * z;
            float xy = rotation.X * y;
            float xz = rotation.X * z;
            float yz = rotation.Y * z;
            float wx = rotation.W * x;
            float wy = rotation.W * y;
            float wz = rotation.W * z;

            Vector3F res;
            res.X = (1F - (yy + zz)) * point.X + (xy - wz) * point.Y + (xz + wy) * point.Z;
            res.Y = (xy + wz) * point.X + (1F - (xx + zz)) * point.Y + (yz - wx) * point.Z;
            res.Z = (xz - wy) * point.X + (yz + wx) * point.Y + (1F - (xx + yy)) * point.Z;
            return res;
        }
        #endregion
    }
}