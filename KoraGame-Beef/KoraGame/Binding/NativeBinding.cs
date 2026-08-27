using System.Runtime.InteropServices;

namespace KoraGame
{
    internal struct NativeBinding : IDisposable
    {
        // Private
        public GCHandle ManagedHandle;
        public IntPtr NativePtr;

        // Properties
        public IntPtr ManagedPtr => (IntPtr)ManagedHandle;

        // Constructor
        public NativeBinding(object managedObject, IntPtr nativePtr = default)
        {
            // Check for null
            if (managedObject == null)
                throw new ArgumentException("Managed object cannot be null");

            ManagedHandle = GCHandle.Alloc(managedObject, GCHandleType.Normal);
            NativePtr = nativePtr;
        }

        // Methods
        public void Dispose()
        {
            if (ManagedHandle.IsAllocated == true)
            {
                ManagedHandle.Free();
                ManagedHandle = default;
            }
        }

        public void CheckNative()
        {
            if (NativePtr == IntPtr.Zero)
                throw new ObjectDisposedException("Native object is destroyed");
        }

        public static T GetManagedObject<T>(IntPtr managedHandle) where T : class
        {
            // Check for null ptr
            if (managedHandle == IntPtr.Zero)
                return default;

            // Try to get object
            return ((GCHandle)managedHandle).Target as T;
        }
    }
}
