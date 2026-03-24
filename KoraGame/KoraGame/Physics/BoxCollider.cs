using Jitter2.Collision.Shapes;
using System.Runtime.Serialization;

namespace KoraGame.Physics
{
    public sealed class BoxCollider : Collider
    {
        // Private
        [DataMember(Name = "Extents")]
        private Vector3F extents = Vector3F.One;

        // Internal
        internal BoxShape physicsBox;

        // Properties
        public Vector3F Extents
        {
            get => extents;
            set
            {
                extents = value;
                RebuildCollider();
            }
        }

        // Constructor
        public BoxCollider()
        {
            this.physicsBox = new BoxShape(extents.Jitter());
        }

        // Methods
        protected override void RebuildCollider()
        {
            base.RebuildCollider();

            // Update extents
            physicsBox.Size = extents.Jitter();
        }
    }
}
