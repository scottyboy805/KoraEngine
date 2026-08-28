using System.Runtime.Serialization;

namespace KoraGame
{
    public struct Transform
    {
        // Private
        private Matrix4F? parentMatrix;

        [DataMember(Name = "Position")]
        private Vector3F position;
        [DataMember(Name = "Rotation")]
        private QuaternionF rotation;
        [DataMember(Name = "Scale")]
        private Vector3F scale;

        // Public
        public static readonly Transform Identity = new Transform(Vector3F.Zero, QuaternionF.Identity, Vector3F.One);

        // Properties
        public Matrix4F TransformMatrix
        {
            get
            {
                // Create this matrix
                Matrix4F mat = Matrix4F.TRS(position, rotation, scale);

                // Check for parent
                if(parentMatrix != null)
                {
                    // Multiply parent matrix
                    mat = parentMatrix.Value * mat;
                }
                return mat;
            }
        }

        public Matrix4F InverseTransformMatrix
        {
            get
            {
                // Get the transform matrix
                Matrix4F mat = TransformMatrix;

                // Invert the matrix
                return Matrix4F.Inverse(mat);
            }
        }

        public Vector3F Position
        {
            get
            {
                if(parentMatrix != null)
                    return parentMatrix.Value * position;

                return position;
            }
        }

        public QuaternionF Rotation
        {
            get
            {
                if (parentMatrix != null)
                {
                    QuaternionF parentRot = parentMatrix.Value.Rotation;
                    return parentRot * rotation;
                }
                return rotation;
            }
        }

        public Vector3F EulerRotation
        {
            get
            {
                return Rotation.EulerAngles;
            }
        }

        public Vector3F Scale
        {
            get
            {
                if (parentMatrix != null)
                {
                    Matrix4F m = parentMatrix.Value;

                    Vector3F parentScale = new Vector3F(
                        new Vector3F(m.C0.X, m.C0.Y, m.C0.Z).Magnitude,
                        new Vector3F(m.C1.X, m.C1.Y, m.C1.Z).Magnitude,
                        new Vector3F(m.C2.X, m.C2.Y, m.C2.Z).Magnitude
                    );

                    return new Vector3F(
                        scale.X * parentScale.X,
                        scale.Y * parentScale.Y,
                        scale.Z * parentScale.Z
                    );
                }

                return scale;
            }
        }

        public Vector3F Forward => TransformDirection(Vector3F.Forward);
        public Vector3F Up => TransformDirection(Vector3F.Up);
        public Vector3F Right => TransformDirection(Vector3F.Right);

        // Constructor
        public Transform(Vector3F position, QuaternionF rotation, Vector3F scale)
        {
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
            this.parentMatrix = null;
        }

        public Transform(Matrix4F matrix, Vector3F position, QuaternionF rotation, Vector3F scale)
        {
            this.parentMatrix = matrix;
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }

        // Methods
        public Vector3F TransformPoint(in Vector3F point)
        {
            Vector4F v = (Vector4F)point;
            return (TransformMatrix * v).XYZ;
        }

        public Vector3F InverseTransformPoint(in Vector3F point)
        {
            Vector4F v = (Vector4F)point;
            return (InverseTransformMatrix * v).XYZ;
        }

        public Vector3F TransformDirection(in Vector3F direction)
        {
            Vector4F v = new Vector4F(direction.X, direction.Y, direction.Z, 0f);
            return (TransformMatrix * v).XYZ;
        }
    }
}
