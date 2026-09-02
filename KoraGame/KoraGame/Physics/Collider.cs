using Box3D;
using System.Runtime.Serialization;

namespace KoraGame.Physics
{
    public abstract class Collider : Component
    {
        // Private
        private RigidBody associatedBody = null;

        [DataMember(Name = "IsTrigger")]
        private bool isTrigger = false;

        // Internal
        internal Body b3dStaticBody;        // Only used for static colliders, otherwise the shape gets attached to a rigidbody
        internal Shape b3dShape;

        // Properties
        public PhysicsWorld PhysicsWorld => Game.PhysicsWorld;

        public bool IsTrigger
        {
            get => isTrigger;
            set
            {
                isTrigger = value;

                // Update shape
                if (b3dShape.IsValid == true)
                    ShapeModified();
            }
        }

        internal bool IsDynamic => associatedBody != null;

        internal Body AttachedBody => associatedBody != null ? associatedBody.b3dBody : b3dStaticBody;

        // Methods
        protected abstract Shape CreateShape(Body b3dBody, in ShapeDefinition b3dShapeDef);

        protected override void OnEnable()
        {
            // Find associated body
            associatedBody = GameObject.GetComponentInParent<RigidBody>();

            // Create the collider
            CreateCollider();          
        }

        protected override void OnDisable()
        {
            // Destroy the collider
            DestroyCollider();

            // Clear associated body
            associatedBody = null;
        }

        internal void ShapeModified()
        {
            // Destroy the existing shape
            if (b3dShape.IsValid == true)
            {
                b3dShape.Destroy();
                b3dShape = default;
            }

            // Create the shape def
            ShapeDefinition shapeDef = ShapeDefinition.Default with
            {
                EnableContactEvents = true,
                EnableHitEvents = true,
                EnableSensorEvents = isTrigger,
                IsSensor = isTrigger,
            };

            // Create the new shape
            b3dShape = CreateShape(AttachedBody, shapeDef);

            // Set shape data
            if (b3dShape.IsValid == true)
                b3dShape.UserData = (ulong)Ptr;
        }

        internal void ParentBodyModified(RigidBody updatingBody)
        {
            associatedBody = updatingBody != null && updatingBody.IsDestroyed == false ? updatingBody : null;

            // Destroy the shape
            if (b3dShape.IsValid == true)
            {
                DestroyCollider();
                CreateCollider();
            }
        }

        private void CreateCollider()
        {
            // Check for null
            if (associatedBody == null)
            {
                // Create the static body
                b3dStaticBody = PhysicsWorld.b3dWorld.CreateStaticBody(GameObject.WorldPosition, GameObject.WorldRotation);
            }

            // Create the shape
            ShapeModified();
        }

        private void DestroyCollider()
        {
            // Destroy the shape
            if (b3dShape.IsValid == true)
            {
                b3dShape.Destroy();
                b3dShape = default;
            }

            // Destroy the static body
            if (b3dStaticBody.IsValid == true)
            {
                b3dStaticBody.Destroy();
                b3dStaticBody = default;
            }
        }

        internal static Collider Get(in Shape shape)
        {
            // Check for valid
            if (shape.IsValid == false)
                return null;

            // Get the user data
            IntPtr ptr = (IntPtr)shape.UserData;

            // Get the rigid body from the pointer
            return FromPtr<Collider>(ptr);
        }
    }
}
