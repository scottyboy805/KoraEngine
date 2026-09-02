using System.Runtime.Serialization;

namespace KoraGame.Physics
{
    public sealed class PhysicsMaterial : GameElement
    {
        // Events
        internal event Action onModified;

        // Internal
        internal Box3D.PhysicsMaterial b3dMaterial;

        // Private
        [DataMember(Name = "Friction")]
        private float friction = 0f;
        [DataMember(Name = "Restitution")]
        private float restitution = 0f;
        [DataMember(Name = "RollingResistance")]
        private float rollingResistance = 0f;

        // Properties
        public float Friction
        {
            get => friction;
            set
            {
                friction = value;
                onModified?.Invoke();
            }
        }

        // Methods
        protected override void OnCreate()
        {
            // Create the Box3D material
            b3dMaterial = new Box3D.PhysicsMaterial
            {
                Friction = friction,
                Restitution = restitution,
                RollingResistance = rollingResistance,
                UserMaterialId = (ulong)Ptr,
            };
        }

        private void OnModified()
        {
            
        }
    }
}
