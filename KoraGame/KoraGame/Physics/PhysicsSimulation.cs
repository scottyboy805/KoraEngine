using Jitter2;
using Jitter2.DataStructures;
using System;

namespace KoraGame.Physics
{
    public sealed class PhysicsSimulation
    {
        // Private
        private int threadCount = 1;
        private Vector3F gravity = new Vector3F(0f, -9.81f, 0f);
        private float fixedStep = 1f / 100f;

        private float fixedStepTimer = 0f;

        // Public
        public const int MaxThreadCount = 8;

        // Internal
        internal World physicsWorld;

        // Properties
        public Vector3F Gravity
        {
            get => gravity;
            set
            {
                gravity = value;
                physicsWorld.Gravity = value.Jitter();
            }
        }

        // Constructor
        internal PhysicsSimulation()
        {
            // Get thread count
            threadCount = Math.Max(1, Math.Min(MaxThreadCount, Environment.ProcessorCount > 4
                ? Environment.ProcessorCount - 2
                : Environment.ProcessorCount - 1));

            World.Capacity worldCapacity = new World.Capacity
            {
                BodyCount = 64000,
                ContactCount = 64000,
                ConstraintCount = 32000,
                SmallConstraintCount = 32000,
            };

            // Create the world
            physicsWorld = new World(worldCapacity);

            // Set gravity
            physicsWorld.Gravity = gravity.Jitter();

            // Setup iterations
            physicsWorld.SubstepCount = 2;
            physicsWorld.SolverIterations = (8, 4);

            // Update thread pool
            Jitter2.Parallelization.ThreadPool.Instance.ChangeThreadCount(threadCount);
            Debug.Log($"Physics simulation assigned thread count: '{threadCount}'", LogFilter.Physics);
        }

        // Methods
        public void Step()
        {
            fixedStepTimer += Time.DeltaTime;

            // Update fixed time
            while (fixedStepTimer >= fixedStep)
            {
                // Update physics world
                physicsWorld.Step(fixedStep, true);
                fixedStepTimer -= fixedStep;

                // Sync after update
                SyncRigidBodies();

                // Update physics time
                Time.UpdateFixedTime(fixedStep);

                // Run physics update for behaviour scripts
                //PerformPhysicsUpdate(gameTime, fixedStep);
            }
        }

        private void SyncRigidBodies()
        {
            // Process all bodies
            ReadOnlyPartitionedSet<Jitter2.Dynamics.RigidBody> activeBodies = physicsWorld.RigidBodies;

            // Update only active bodies
            for (int i = 0; i < activeBodies.ActiveCount; i++)
            {
                // Get the body
                Jitter2.Dynamics.RigidBody body = activeBodies[i];

                // Check for body associated
                if (body.Tag is RigidBody rigidBody)
                {
                    // Sync with physics object
                    rigidBody.SyncTransform();
                }
            }
        }
    }
}
