
using Jitter2.Collision.Shapes;
using System.Runtime.Serialization;

namespace KoraGame.Physics
{
    public sealed class SphereCollider : Collider
    {
        // Private
        [DataMember(Name = "Radius")]
        private float radius = 0.5f;

        // Internal
        internal SphereShape physicsSphere;

        // Properties
        public float Radius
        {
            get => radius;
            set
            {
                radius = value;
                RebuildCollider();
            }
        }

        // Methods
        protected override void RebuildCollider()
        {
            base.RebuildCollider();

            // Update radius
            physicsSphere.Radius = radius;
        }
    }
}
