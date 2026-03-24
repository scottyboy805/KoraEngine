using SDL;

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

    public enum IndexBufferFormat : uint
    {
        Int16,
        Int32,
    }

    public unsafe sealed class GraphicsBuffer
    {
        // Private
        private readonly GraphicsDevice device;
        private readonly GraphicsBufferUsage usage;
        private readonly uint size;

        // Internal
        internal readonly SDL_GPUBuffer* gpuBuffer;
        internal readonly SDL_GPUTransferBuffer* gpuUploadBuffer;

        // Properties
        public GraphicsDevice Device => device;
        public GraphicsBufferUsage Usage => usage;
        public uint Size => size;

        // Constructor
        public GraphicsBuffer(GraphicsDevice device, GraphicsBufferUsage usage, uint size)
        {
            // Check for null
            if(device == null)
                throw new ArgumentNullException(nameof(device));

            this.device = device;
            this.usage = usage;
            this.size = size;

            // Setup buffer
            SDL_GPUBufferCreateInfo bufferInfo = new SDL_GPUBufferCreateInfo
            {
                usage = (SDL_GPUBufferUsageFlags)usage,
                size = size,
            };

            // Create the buffer
            gpuBuffer = SDL3.SDL_CreateGPUBuffer(device.gpuDevice, &bufferInfo);

            // Check for error
            if (gpuBuffer == null)
                throw new InvalidOperationException("Failed to create GPU buffer: " + SDL3.SDL_GetError());


            // Setup upload buffer
            SDL_GPUTransferBufferCreateInfo uploadBufferInfo = new SDL_GPUTransferBufferCreateInfo
            {
                usage = SDL_GPUTransferBufferUsage.SDL_GPU_TRANSFERBUFFERUSAGE_UPLOAD,
                size = size,
            };

            // Create the upload buffer
            gpuUploadBuffer = SDL3.SDL_CreateGPUTransferBuffer(device.gpuDevice, &uploadBufferInfo);

            // Check for error
            if (gpuUploadBuffer == null)
                throw new InvalidOperationException("Failed to create GPU upload buffer: " + SDL3.SDL_GetError());
        }

        ~GraphicsBuffer()
        {
            SDL3.SDL_ReleaseGPUBuffer(device.gpuDevice, gpuBuffer);
            SDL3.SDL_ReleaseGPUTransferBuffer(device.gpuDevice, gpuUploadBuffer);
        }

        // Methods
        public void Write<T>(ReadOnlySpan<T> data) where T : unmanaged
        {
            // Map the upload buffer
            IntPtr dst = SDL3.SDL_MapGPUTransferBuffer(device.gpuDevice, gpuUploadBuffer, false);

            // Pin the memory
            fixed (T* src = data)
            {
                // Copy the memory
                SDL3.SDL_memcpy(dst, (IntPtr)src, size);
            }

            // Unmap the pointer
            SDL3.SDL_UnmapGPUTransferBuffer(device.gpuDevice, gpuUploadBuffer);
        }

        public void MapMemory(Action<IntPtr> bufferMemoryAction)
        {
            // Map the upload buffer
            IntPtr dst = SDL3.SDL_MapGPUTransferBuffer(device.gpuDevice, gpuUploadBuffer, false);

            // Get the pointer
            byte* ptr = (byte*)dst;

            // Trigger the write action
            try
            {
                // Attempt to write the data
                bufferMemoryAction((IntPtr)ptr);
            }
            finally
            {
                // Unmap the pointer
                SDL3.SDL_UnmapGPUTransferBuffer(device.gpuDevice, gpuUploadBuffer);
            }
        }

        public GraphicsBuffer Copy()
        {
            // Create the buffer
            GraphicsBuffer copy = new GraphicsBuffer(device, usage, size);

            // Map the source buffer memory
            MapMemory((IntPtr src) =>
            {
                // Map the destination buffer memory
                copy.MapMemory((IntPtr dst) =>
                {
                    // Copy the memory
                    SDL3.SDL_memcpy(dst, src, size);
                });
            });

            return copy;
        }
    }
}
