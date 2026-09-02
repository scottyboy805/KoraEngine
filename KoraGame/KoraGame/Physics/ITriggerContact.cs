
namespace KoraGame.Physics
{
    public interface ITriggerContact
    {
        // Methods
        void OnTriggerBegin(Collider other);
        void OnTriggerEnd(Collider other);

        internal static void DoTriggerBegin(ITriggerContact receiver, Collider collider)
        {
            if (receiver != null && collider != null)
            {
                try
                {
                    receiver.OnTriggerBegin(collider);
                }
                catch (Exception e)
                {
                    // Handle exception
                    Debug.LogException(e);
                }
            }
        }

        internal static void DoTriggerEnd(ITriggerContact receiver, Collider collider)
        {
            if (receiver != null && collider != null)
            {
                try
                {
                    receiver.OnTriggerEnd(collider);
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
