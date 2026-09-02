using Box3D;
using System.Runtime.Serialization;

namespace KoraGame.Physics
{
    public sealed class BoxCollider : Collider
    {
        // Private
        [DataMember(Name = "Size")]
        private Vector3F size = Vector3F.One;

        // Properties
        public Vector3F Size
        {
            get => size;
            set
            {
                size = value;
                ShapeModified();
            }
        }

        // Methods
        protected override Shape CreateShape(Body b3dBody, in ShapeDefinition b3dShapeDef)
        {
            // Create the box
            Box box = new Box(size / 2, GameObject.LocalPosition);

            // Add the shape
            return b3dBody.AddBox(box, b3dShapeDef);
        }
    }
}
