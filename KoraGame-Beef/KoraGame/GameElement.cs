
namespace KoraGame
{
    public abstract class GameElement
    {
        // Internal
        internal NativeBinding binding;

        //// Private
        //private GCHandle managedHandle;

        //// Properties
        //internal IntPtr ManagedPtr => (IntPtr)managedHandle;
        //internal IntPtr NativePtr { get; set; }

        // Constructor
        protected GameElement()
        {
            binding = new NativeBinding(this);
        }

        ~GameElement()
        {
            binding.Dispose();
        }

        // Methods
        //protected void CheckNative()
        //{
        //    if (NativePtr == IntPtr.Zero)
        //        throw new ObjectDisposedException("Native object is destroyed");
        //}

        //internal static T GetManagedObject<T>(IntPtr managedHandle) where T : GameElement
        //{
        //    return ((GCHandle)managedHandle).Target as T;
        //}
    }
}
