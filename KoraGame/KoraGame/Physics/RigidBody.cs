using Jitter2.LinearMath;
using System.Runtime.Serialization;

namespace KoraGame.Physics
{
    public sealed class RigidBody : Component
    {
        // Private
        [DataMember(Name = "Mass")]
        private float mass = 1f;
        [DataMember(Name = "IsKinematic")]
        private bool isKinematic = false;
        [DataMember(Name = "LinearDamping")]
        private float linearDamping = 0f;
        [DataMember(Name = "AngularDamping")]
        private float angularDamping = 0.5f;

        private Collider mainCollider = null;
        private List<Collider> additionalColliders = null;

        // Internal
        internal Jitter2.Dynamics.RigidBody physicsBody;
        internal Vector3F velocity = default;
        internal Vector3F angularVelocity = default;

        // Properties
        public PhysicsSimulation Physics => Game.Instance?.Physics;

        public float Mass
        {
            get => mass;
            set
            {
                mass = value;
                RebuildBody();
            }
        }

        public bool IsKinematic
        {
            get => isKinematic;
            set
            {
                isKinematic = value;
                RebuildBody();
            }
        }

        public float LinearDamping
        {
            get => linearDamping;
            set
            {
                linearDamping = value;
                RebuildBody();
            }
        }

        public float AngularDamping
        {
            get => angularDamping;
            set
            {
                angularDamping = value;
                RebuildBody();
            }
        }

        public Vector3F Velocity
        {
            get => velocity;
            set
            {
                velocity = value;
                RebuildBody();
            }
        }

        public Vector3F AngularVelocity
        {
            get => angularVelocity;
            set
            {
                angularVelocity = value;
                RebuildBody();
            }
        }

        // Constructor
        public RigidBody()
        {
            // Create body
            physicsBody = Physics.physicsWorld.CreateRigidBody();
            physicsBody.Tag = this;
        }

        // Methods
        internal override void RegisterSubSystems()
        {
            // Check for no colliders
            if (mainCollider != null)
            {
                // Create the body
                physicsBody = Physics.physicsWorld.CreateRigidBody();
                physicsBody.Tag = this;

                // Check for compound collider
                if (additionalColliders != null)
                {
                    // Add shapes
                    foreach (Collider collider in additionalColliders)
                        physicsBody.AddShape(collider.PhysicsShape);
                }
                // Single collider
                else
                {
                    // Use main collider
                    physicsBody.AddShape(mainCollider.PhysicsShape);
                }
            }

            // Rebuild the body
            RebuildBody();
        }

        internal override void UnregisterSubSystems()
        {
            // Check for handle
            if (physicsBody != null)
            {
                if (additionalColliders != null)
                {
                    // Remove shapes
                    foreach (Collider collider in additionalColliders)
                        physicsBody.RemoveShape(collider.PhysicsShape);
                }
                else if (mainCollider != null)
                {
                    // Remove main collider
                    physicsBody.RemoveShape(mainCollider.PhysicsShape);
                }

                // Remove dynamic
                Physics.physicsWorld.Remove(physicsBody);
                physicsBody = null;
            }
        }

        internal void AttachCollider(Collider collider)
        {
            if (mainCollider != null)
            {
                // Create list if required
                if (additionalColliders == null)
                    additionalColliders = new List<Collider>(8);

                // Update colliders
                additionalColliders.Add(collider);
            }
            else
            {
                // Set main collider
                mainCollider = collider;
            }

            // Attach to body
            if (physicsBody != null)
                physicsBody.AddShape(collider.PhysicsShape);
        }

        internal void DetachCollider(Collider collider)
        {
            if(mainCollider == collider)
            {
                // Revert main collider
                mainCollider = additionalColliders != null && additionalColliders.Count > 0
                    ? additionalColliders[0] : null;
            }
            else if (additionalColliders != null)
            {
                // Remove if found
                if (additionalColliders.Contains(collider) == true)
                    additionalColliders.Remove(collider);
            }

            // Remove collider
            if (physicsBody != null)
                physicsBody.RemoveShape(collider.PhysicsShape);
        }

        internal void RebuildBody()
        {
            // Update transform
            physicsBody.Position = GameObject.WorldPosition.Jitter();
            physicsBody.Orientation = GameObject.WorldRotation.Jitter();

            // Check for kinematic
            if (isKinematic == false)
            {
                physicsBody.SetMassInertia(mass);
                physicsBody.Damping = (linearDamping, angularDamping);
            }
            else
            {
                physicsBody.SetMassInertia(JMatrix.Zero, 1e-3f, true);
                physicsBody.Damping = (0f, 0f);
            }
        }

        internal void SyncTransform()
        {
            // Sync position
            JVector position = physicsBody.Position;
            GameObject?.WorldPosition = new Vector3F(position.X, position.Y, position.Z);

            // Sync rotation
            JQuaternion rotation = physicsBody.Orientation;
            GameObject?.WorldRotation = new QuaternionF(rotation.X, rotation.Y, rotation.Z, rotation.W);
        }
    }
}
