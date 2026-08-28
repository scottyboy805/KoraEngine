using System.Runtime.InteropServices.Marshalling;

namespace KoraGame
{
    internal unsafe struct Utf8String : IDisposable
    {
        // Private
        private byte* rawString = null;

        // Properties
        public byte* Ptr => rawString;

        // Constructor
        private Utf8String(string value)
        {
            // Check for null
            if (value != null)
            {
                // Get bytes
                rawString = Utf8StringMarshaller.ConvertToUnmanaged(value);
            }
        }

        // Methods
        public void Dispose()
        {
            if (rawString != null)
            {
                Utf8StringMarshaller.Free(rawString);
                rawString = null;
            }
        }

        public static implicit operator Utf8String(string value)
        {
            return new Utf8String(value);
        }
    }
}
