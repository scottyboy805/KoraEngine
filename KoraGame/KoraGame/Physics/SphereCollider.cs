using Box3D;
using System.Runtime.Serialization;

namespace KoraGame.Physics
{
    public sealed class SphereCollider : Collider
    {
        // Private
        [DataMember(Name = "Radius")]
        private float radius = 0.5f;

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

        // Methods
        protected override Shape CreateShape(Body b3dBody, in ShapeDefinition b3dShapeDef)            
        {
            // Create the sphere
            Sphere sphere = new Sphere(GameObject.LocalPosition, radius);

            // Add the shape
            return b3dBody.AddSphere(sphere, b3dShapeDef);
        }
    }
}
