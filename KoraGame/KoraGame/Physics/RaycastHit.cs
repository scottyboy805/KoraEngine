
namespace KoraGame.Physics
{
    public readonly struct RaycastHit
    {
        // Internal
        internal readonly Box3D.RaycastHit b3dHit;

        // Public
        public readonly Collider Collider;

        // Properties
        public readonly bool Hit => b3dHit.Hit;
        public readonly float Distance => b3dHit.Fraction;
        public readonly Vector3F Point => b3dHit.Point;
        public readonly Vector3F Normal => b3dHit.Normal;

        // Constructor
        internal RaycastHit(Box3D.RaycastHit b3dHit)
        {
            this.b3dHit = b3dHit;

            // Check for hit
            if (b3dHit.Hit == true && b3dHit.Shape.IsValid == true)
                this.Collider = GameElement.FromPtr<Collider>((IntPtr)b3dHit.Shape.UserData);
        }
    }
}
