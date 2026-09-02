using Box3D;

namespace KoraGame.Physics
{
    public readonly struct Collision
    {
        // Private
        private readonly ContactHitEvent contact;

        // Public
        public readonly Collider ColliderA;
        public readonly Collider ColliderB;

        // Properties
        public readonly Vector3F Point => contact.Point;
        public readonly Vector3F Normal => contact.Normal;
        public readonly float Speed => contact.ApproachSpeed;

        // Constructor
        internal Collision(ContactHitEvent contact)
        {
            this.contact = contact;

            // Get first collider
            if(contact.ShapeA.IsValid == true)
                this.ColliderA = GameElement.FromPtr<Collider>((IntPtr)contact.ShapeA.UserData);

            // Get second collider
            if(contact.ShapeB.IsValid == true)
                this.ColliderB = GameElement.FromPtr<Collider>((IntPtr)contact.ShapeB.UserData);
        }
    }

    public interface ICollisionImpact
    {
        // Methods
        void OnCollisionImpact(Collision collision);

        internal static void DoCollisionImpact(ICollisionImpact receiver, Collision collision)
        {
            if (receiver != null)
            {
                try
                {
                    receiver.OnCollisionImpact(collision);
                }
                catch (Exception e)
                {
                    // Handle exception
                    Debug.LogException(e);
                }
            }
        }
    }
}
