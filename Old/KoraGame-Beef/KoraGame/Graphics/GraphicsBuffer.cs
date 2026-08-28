using System.Runtime.InteropServices;

namespace KoraGame.Graphics
{
    [Flags]
    public enum GraphicsBufferUsage : uint
    {
        Vertex = (1u << 0),
        Index = (1u << 1),
        Indirect = (1u << 2),
        GraphicsRead = (1u << 3),
        ComputeRead = (1u << 4),
        ComputeWrite = (1u << 5),
    }

    public enum IndexBufferFormat : int
    {
        Int16,
        Int32,
    }

    public sealed class GraphicsBuffer
    {
        // Internal
        internal NativeBinding binding;

        // Properties
        public uint Size
        {
            get
            {
                binding.CheckNative();
                return GraphicsBuffer_GetSize(binding.NativePtr);
            }
        }

        public GraphicsBufferUsage Usage
        {
            get
            {
                binding.CheckNative();
                return GraphicsBuffer_GetUsage(binding.NativePtr);
            }
        }

        // Constructor
        private GraphicsBuffer(IntPtr nativePtr)
        {
            binding.NativePtr = nativePtr;
        }

        public GraphicsBuffer(uint size, GraphicsBufferUsage usage)
        {
            binding.NativePtr = GraphicsBuffer_CreateNative(binding.ManagedPtr, size, usage);
        }

        // Methods

        // Bindings
        [UnmanagedCallersOnly]
        internal static IntPtr GraphicsBuffer_CreateManaged(IntPtr nativePtr)
        {
            // Create new managed buffer
            GraphicsBuffer buffer = new(nativePtr);

            // Get managed
            return buffer.binding.ManagedPtr;
        }

        [DllImport("KoraGame")]
        internal static extern IntPtr GraphicsBuffer_CreateNative(IntPtr managedPtr, uint size, GraphicsBufferUsage usage);

        [DllImport("KoraGame")]
        internal static extern uint GraphicsBuffer_GetSize(IntPtr nativePtr);

        [DllImport("KoraGame")]
        internal static extern GraphicsBufferUsage GraphicsBuffer_GetUsage(IntPtr nativePtr);
    }
}
