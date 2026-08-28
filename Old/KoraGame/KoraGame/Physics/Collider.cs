using Jitter2.Collision.Shapes;
using Jitter2.Dynamics;
using System.Runtime.Serialization;

namespace KoraGame.Physics
{
    public abstract class Collider : Component
    {
        // Private
        [DataMember(Name = "IsTrigger")]
        private bool isTrigger = false;

        private RigidBody attachedBody = null;

        // Properties
        internal RigidBodyShape PhysicsShape { get; }
        public PhysicsSimulation Physics => Game?.Physics;

        public bool IsTrigger
        {
            get => isTrigger;
            set
            {
                isTrigger = value;
                RebuildCollider();
            }
        }

        // Methods
        internal override void RegisterSubSystems()
        {
            // Get attached body
            attachedBody = GameObject?.GetComponentInParent<RigidBody>();

            // Attach the collider
            if(attachedBody != null)
            {
                // Add the collider
                attachedBody.AttachCollider(this);
            }
        }

        internal override void UnregisterSubSystems()
        {
            // Detatch collider
            if(attachedBody != null)
            {
                // Remove the collider
                attachedBody.DetachCollider(this);
                attachedBody = null;
            }
        }

        protected virtual void RebuildCollider()
        {
        }
    }
}
