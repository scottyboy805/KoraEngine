
namespace KoraGame.Physics
{
    public interface ICollisionContact
    {
        // Methods
        void OnContactBegin(Collider other);
        void OnContactEnd(Collider other);

        internal static void DoContactBegin(ICollisionContact receiver, Collider collider)
        {
            if(receiver != null && collider != null)
            {
                try
                {
                    receiver.OnContactBegin(collider);
                }
                catch (Exception e)
                {
                    // Handle exception
                    Debug.LogException(e);
                }
            }
        }

        internal static void DoContactEnd(ICollisionContact receiver, Collider collider)
        {
            if (receiver != null && collider != null)
            {
                try
                {
                    receiver.OnContactEnd(collider);
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
