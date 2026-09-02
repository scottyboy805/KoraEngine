using Box3D;
using System.Diagnostics.Contracts;
using System.Reflection.Metadata;
using System.Runtime.Serialization;

namespace KoraGame.Physics
{
    public sealed class PhysicsWorld
    {
        // Type
        private readonly struct RaycastReceiver : IRaycastCallback
        {
            // Private
            private readonly IList<RaycastHit> recordedHits;

            // Constructor
            public RaycastReceiver(IList<RaycastHit> raycastHits)
            {
                this.recordedHits = raycastHits;
            }

            // Methods
            public RaycastAction OnHit(in Box3D.RaycastHit hit)
            {
                recordedHits?.Add(new RaycastHit(hit));
                return RaycastAction.Continue;
            }
        }

        // Private
        private float accumulatedTime = 0f;

        [DataMember(Name = "Gravity")]
        private Vector3F gravity = new Vector3F(0, -9.81f, 0);
        [DataMember(Name = "FixedTimeStep")]
        private float fixedTimeStep = 1.0f / 100.0f;

        // Internal
        internal Box3D.PhysicsWorld b3dWorld;

        // Properties
        public Vector3F Gravity
        {
            get => gravity;
            set
            {
                gravity = value;

                // Update world gravity
                if (b3dWorld != null && b3dWorld.IsDisposed == false)
                    b3dWorld.Gravity = gravity;
            }
        }

        public float FixedTimeStep
        {
            get => fixedTimeStep;
            set => fixedTimeStep = Math.Max(0, value);
        }

        // Internal
        internal PhysicsWorld() { }

        // Methods
        internal void Initialize()
        {
            // Create world settings
            WorldSettings worldSettings = WorldSettings.Default with
            {
                Gravity = gravity,
            };

            // Create world
            b3dWorld = new Box3D.PhysicsWorld(worldSettings);
        }

        internal void Update()
        {
            // Check for any time step
            if(fixedTimeStep <= 0f)
                return;

            // Add delta time
            accumulatedTime += Time.DeltaTime;

            // Step the world in fixed time steps
            while (accumulatedTime > fixedTimeStep)
            {
                // Step the world by the smaller fixed time step
                b3dWorld.Step(fixedTimeStep);
                accumulatedTime -= fixedTimeStep;

                // Do collision and trigger events after step
                HandleWorldCollisionEvents(b3dWorld.Events);
                HandleWorldTriggerEvents(b3dWorld.Events);
            }

            // Sync transforms of all bodies in the world
            HandleWorldSync(b3dWorld.Events);
        }

        internal void Shutdown()
        {
            // Dispose world
            b3dWorld.Dispose();
            b3dWorld = null;
        }

        public RaycastHit Raycast(Vector3F origin, Vector3F direction, float maxDistance = float.MaxValue)
        {
            // Get no hit with 0 distance
            if (maxDistance <= 0f)
                return default;

            // Get aim direction
            Vector3F aimDirection = direction.Normalized * maxDistance;

            // Perform raycast
            Box3D.RaycastHit b3dHit = b3dWorld.RaycastClosest(origin, aimDirection);

            // Return the hit result
            return new RaycastHit(b3dHit);
        }

        public IReadOnlyList<RaycastHit> RaycastAll(Vector3F origin, Vector3F direction, float maxDistance = float.MaxValue)
        {
            // Get no hit with 0 distance
            if (maxDistance <= 0f)
                return Array.Empty<RaycastHit>();

            // Get aim direction
            Vector3F aimDirection = direction.Normalized * maxDistance;

            // Create results
            List<RaycastHit> hits = new();
            RaycastReceiver receiver = new(hits);

            // Perform raycast
            b3dWorld.Raycast(origin, aimDirection, ref receiver);

            // Get all hits
            return hits;
        }

        public int RaycastAll(Vector3F origin, Vector3F direction, IList<RaycastHit> hits, float maxDistance = float.MaxValue)
        {
            // Get no hit with 0 distance
            if (hits == null || maxDistance <= 0f)
                return 0;

            // Get aim direction
            Vector3F aimDirection = direction.Normalized * maxDistance;

            // Get initial count
            int initialCount = hits.Count;

            // Create results
            RaycastReceiver receiver = new(hits);

            // Perform raycast
            b3dWorld.Raycast(origin, aimDirection, ref receiver);

            // Get all hit count
            return hits.Count - initialCount;
        }

        private static void HandleWorldCollisionEvents(in WorldEvents worldEvents)
        {
            // Handle contact begins
            foreach (ContactBeginEvent contact in worldEvents.ContactBegins)
            {
                // Get colliders
                Collider colliderA = Collider.Get(contact.ShapeA);
                Collider colliderB = Collider.Get(contact.ShapeB);

                // Handle collider A - B
                if (colliderA != null && colliderA.IsDynamic == true)
                {
                    // Get collision receivers
                    foreach (ICollisionContact contactReceiver in colliderA.GameObject.EnumerateComponentsInParent<ICollisionContact>(false))
                        ICollisionContact.DoContactBegin(contactReceiver, colliderB);
                }
            }

            // Handle contact ends
            foreach (ContactEndEvent contact in worldEvents.ContactEnds)
            {
                // Get colliders
                Collider colliderA = Collider.Get(contact.ShapeA);
                Collider colliderB = Collider.Get(contact.ShapeB);

                // Handle collider A - B
                if (colliderA != null && colliderA.IsDynamic == true)
                {
                    // Get collision receivers
                    foreach (ICollisionContact contactReceiver in colliderA.GameObject.EnumerateComponentsInParent<ICollisionContact>(false))
                        ICollisionContact.DoContactEnd(contactReceiver, colliderB);
                }
            }

            // Handle impacts
            foreach(ContactHitEvent contact in worldEvents.ContactHits)
            {
                // Create the collision
                Collision collision = new(contact);

                // Get first collider
                Collider colliderA = Collider.Get(contact.ShapeA);

                // Handle collider A -B
                if (colliderA != null && colliderA.IsDynamic == true)
                {
                    // Get collision receivers
                    foreach (ICollisionImpact contactReceiver in colliderA.GameObject.EnumerateComponentsInParent<ICollisionImpact>(false))
                        ICollisionImpact.DoCollisionImpact(contactReceiver, collision);
                }
            }
        }

        private static void HandleWorldTriggerEvents(in WorldEvents worldEvents)
        {
            // Handle begin
            foreach (SensorBeginEvent trigger in worldEvents.SensorBegins)
            {
                // Get colliders
                Collider colliderA = Collider.Get(trigger.Sensor);
                Collider colliderB = Collider.Get(trigger.Visitor);

                // Handle collider A - B
                if (colliderA != null && colliderA.IsDynamic == true)
                {
                    // Get trigger receivers
                    foreach (ITriggerContact triggerReceiver in colliderA.GameObject.EnumerateComponentsInParent<ITriggerContact>(false))
                        ITriggerContact.DoTriggerBegin(triggerReceiver, colliderB);
                }
            }

            // Handle end
            foreach (SensorEndEvent trigger in worldEvents.SensorEnds)
            {
                // Get colliders
                Collider colliderA = Collider.Get(trigger.Sensor);
                Collider colliderB = Collider.Get(trigger.Visitor);

                // Handle collider A - B
                if (colliderA != null && colliderA.IsDynamic == true)
                {
                    // Get trigger receivers
                    foreach (ITriggerContact triggerReceiver in colliderA.GameObject.EnumerateComponentsInParent<ITriggerContact>(false))
                        ITriggerContact.DoTriggerEnd(triggerReceiver, colliderB);
                }
            }
        }

        private static void HandleWorldSync(in WorldEvents worldEvents)
        {
            // Sync transforms of all bodies in the world
            foreach (BodyMoveEvent move in worldEvents.BodyMoves)
            {
                // Get the rigid body component from the user data
                RigidBody rigidBody = RigidBody.Get(move.Body);

                // Sync the scene transform with the physics body transform
                rigidBody?.SyncTransform();
            }
        }
    }
}
