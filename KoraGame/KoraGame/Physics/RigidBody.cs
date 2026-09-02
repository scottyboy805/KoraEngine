using Box3D;
using Box3D.Interop;
using System.Runtime.Serialization;

namespace KoraGame.Physics
{
    public sealed class RigidBody : Component
    {
        // Private
        [DataMember(Name = "IsKinematic")]
        private bool isKinematic = false;
        [DataMember(Name = "Mass")]
        private float mass = 1;
        [DataMember(Name = "GravityScale")]
        private float gravityScale = 1f;

        // Internal
        internal Body b3dBody;

        // Properties
        public PhysicsWorld PhysicsWorld => Game.PhysicsWorld;

        public bool IsKinematic
        {
            get => isKinematic;
            set
            {
                isKinematic = value;

                // Update body
                if (b3dBody.IsValid == true)
                    b3dBody.Type = isKinematic ? BodyType.Kinematic : BodyType.Dynamic;
            }
        }

        public float Mass
        {
            get => mass;
            set
            {
                mass = Math.Max(0f, value);

                // Update body
                if (b3dBody.IsValid == true)
                {
                    // Get the mass data
                    Box3D.Native.b3MassData massData = Box3D.Native.B3.b3Body_GetMassData(b3dBody.ToNativeId());

                    // Update mass
                    massData.mass = mass;

                    // Apply the mass data
                    Box3D.Native.B3.b3Body_SetMassData(b3dBody.ToNativeId(), massData);
                }
            }
        }

        public float GravityScale
        {
            get => gravityScale;
            set
            {
                gravityScale = Math.Max(0, value);

                // Update body
                if(b3dBody.IsValid == true)
                    b3dBody.GravityScale = gravityScale;
            }
        }

        public Vector3F CenterOfMass
        {
            get => b3dBody.IsValid ? b3dBody.CenterOfMass : default;
            set
            {
                // Update body
                if (b3dBody.IsValid == true)
                {
                    // Get the mass data
                    Box3D.Native.b3MassData massData = Box3D.Native.B3.b3Body_GetMassData(b3dBody.ToNativeId());

                    // Update mass
                    massData.center = value;

                    // Apply the mass data
                    Box3D.Native.B3.b3Body_SetMassData(b3dBody.ToNativeId(), massData);
                }
            }
        }

        public Vector3F Position
        {
            get => b3dBody.IsValid ? b3dBody.Position : default;
            set
            {
                if (b3dBody.IsValid == true)
                    b3dBody.SetTransform(value, b3dBody.Rotation);
            }
        }

        public QuaternionF Rotation
        {
            get => b3dBody.IsValid ? b3dBody.Rotation : default;
            set
            {
                if (b3dBody.IsValid == true)
                    b3dBody.SetTransform(b3dBody.Position, value);
            }
        }

        public Vector3F LinearVelocity => b3dBody.IsValid ? b3dBody.LinearVelocity : default;
        public Vector3F AngularVelocity => b3dBody.IsValid ? b3dBody.AngularVelocity : default;

        // Methods
        protected override void OnEnable()
        {
            // Create body
            BodyDefinition bodyDef = BodyDefinition.Dynamic(GameObject.WorldPosition, GameObject.WorldRotation) with
            {
                Position = GameObject.WorldPosition,
                Rotation = GameObject.WorldRotation,
                GravityScale = gravityScale,
            };

            // Creeate the body in the physics world
            b3dBody = PhysicsWorld.b3dWorld.CreateBody(bodyDef);

            // Set user data
            b3dBody.UserData = (ulong)Ptr;

            // Set the initial mass
            Mass = mass;

            // Inform child colliders that the parent body has been modified
            foreach (Collider collider in GameObject.EnumerateComponentsInChildren<Collider>(false))
                collider.ParentBodyModified(this);
        }

        protected override void OnDisable()
        {
            if (b3dBody.ShapeCount > 0)
            {
                // Detach registered colliders
                Span<Shape> shapes = stackalloc Shape[b3dBody.ShapeCount];

                // Get the shapes
                b3dBody.GetShapes(shapes);

                // Detach all shapes
                foreach (Shape shape in shapes)
                {
                    // Detach from rigid body
                    Collider childCollider = Collider.Get(shape);
                    childCollider?.ParentBodyModified(null);
                }
            }

            // Destroy the body in the physics world
            b3dBody.Destroy();
            b3dBody = default;
        }

        public void SetTransform(Vector3F position, QuaternionF rotation)
        {
            if (b3dBody.IsValid == true)
                b3dBody.SetTransform(position, rotation);
        }

        internal void SyncTransform()
        {
            // Update world transform from physics body
            GameObject.WorldTransform = new Transform(Position, Rotation, GameObject.LocalScale);
        }

        internal static RigidBody Get(in Body body)
        {
            // Check for valid
            if (body.IsValid == false)
                return null;

            // Get the user data
            IntPtr ptr = (IntPtr)body.UserData;

            // Get the rigid body from the pointer
            return FromPtr<RigidBody>(ptr);
        }
    }
}
