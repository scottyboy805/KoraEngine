using Box3D;

namespace KoraGame.Physics
{
    public sealed class CapsuleCollider : Collider
    {
        // Private
        private float radius = 0.5f;
        private float height = 1f;

        // Properties
        public float Radius
        {
            get => radius;
            set
            {
                radius = value;
                ShapeModified();
            }
        }

        public float Height
        {
            get => height;
            set
            {
                height = value;
                ShapeModified();
            }
        }

        // Methods
        protected override Shape CreateShape(Body b3dBody, in ShapeDefinition b3dShapeDef)
        {
            // Create the capsule
            Capsule capsule = new Capsule(
                GameObject.LocalPosition + new Vector3F(0f, -height / 2, 0f),
                GameObject.LocalPosition + new Vector3F(0f, height / 2, 0f),
                radius);

            // Add the capsule
            return b3dBody.AddCapsule(capsule, b3dShapeDef);
        }
    }
}
