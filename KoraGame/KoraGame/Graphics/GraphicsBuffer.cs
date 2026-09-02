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
        private readonly GraphicsDevice graphics;
        private readonly GraphicsBufferUsage usage;
        private readonly uint size;

        // Internal
        internal readonly SDL_GPUBuffer* gpuBuffer;
        internal SDL_GPUTransferBuffer* gpuUploadBuffer;
        internal SDL_GPUTransferBuffer* gpuDownloadBuffer;

        // Properties
        public GraphicsDevice Graphics => graphics;
        public GraphicsBufferUsage Usage => usage;
        public uint Size => size;

        private SDL_GPUTransferBuffer* GPUUploadBuffer
        {
            get
            {
                // Create on demand
                if(gpuUploadBuffer == null && gpuBuffer != null)
                {
                    // Setup upload buffer
                    SDL_GPUTransferBufferCreateInfo uploadBufferInfo = new SDL_GPUTransferBufferCreateInfo
                    {
                        usage = SDL_GPUTransferBufferUsage.SDL_GPU_TRANSFERBUFFERUSAGE_UPLOAD,
                        size = size,
                    };

                    // Create the upload buffer
                    gpuUploadBuffer = SDL3.SDL_CreateGPUTransferBuffer(graphics.gpuDevice, &uploadBufferInfo);

                    // Check for error
                    if (gpuUploadBuffer == null)
                        throw new InvalidOperationException("Failed to create GPU upload buffer: " + SDL3.SDL_GetError());
                }
                return gpuUploadBuffer;
            }
        }

        private SDL_GPUTransferBuffer* GPUDownloadBuffer
        {
            get
            {
                // Create on demand
                if (gpuDownloadBuffer == null && gpuBuffer != null)
                {
                    // Setup download
                    SDL_GPUTransferBufferCreateInfo downloadBufferInfo = new SDL_GPUTransferBufferCreateInfo
                    {
                        usage = SDL_GPUTransferBufferUsage.SDL_GPU_TRANSFERBUFFERUSAGE_DOWNLOAD,
                        size = size,
                    };

                    // Create the download buffer
                    gpuDownloadBuffer = SDL3.SDL_CreateGPUTransferBuffer(graphics.gpuDevice, &downloadBufferInfo);

                    // Check for error
                    if (gpuDownloadBuffer == null)
                        throw new InvalidOperationException("Failed to create GPU download buffer: " + SDL3.SDL_GetError());
                }
                return gpuDownloadBuffer;
            }
        }

        // Constructor
        public GraphicsBuffer(GraphicsDevice graphics, uint size, GraphicsBufferUsage usage)
        {
            // Check for null
            if(graphics == null)
                throw new ArgumentNullException(nameof(graphics));

            this.graphics = graphics;
            this.size = size;
            this.usage = usage;            

            // Setup buffer
            SDL_GPUBufferCreateInfo bufferInfo = new SDL_GPUBufferCreateInfo
            {
                usage = (SDL_GPUBufferUsageFlags)usage,
                size = size,
            };

            // Create the buffer
            gpuBuffer = SDL3.SDL_CreateGPUBuffer(graphics.gpuDevice, &bufferInfo);

            // Check for error
            if (gpuBuffer == null)
                throw new InvalidOperationException("Failed to create GPU buffer: " + SDL3.SDL_GetError());
        }

        ~GraphicsBuffer()
        {
            SDL3.SDL_ReleaseGPUBuffer(graphics.gpuDevice, gpuBuffer);

            if (gpuUploadBuffer != null)
            {
                SDL3.SDL_ReleaseGPUTransferBuffer(graphics.gpuDevice, gpuUploadBuffer);
                gpuUploadBuffer = null;
            }

            if(gpuDownloadBuffer != null)
            {
                SDL3.SDL_ReleaseGPUTransferBuffer(graphics.gpuDevice, gpuDownloadBuffer);
                gpuDownloadBuffer = null;
            }
        }

        // Methods
        public void Write<T>(ReadOnlySpan<T> data) where T : unmanaged
        {
            // Map the upload buffer
            IntPtr dst = SDL3.SDL_MapGPUTransferBuffer(graphics.gpuDevice, GPUUploadBuffer, false);

            // Pin the memory
            fixed (T* src = data)
            {
                // Copy the memory
                SDL3.SDL_memcpy(dst, (IntPtr)src, size);
            }

            // Unmap the pointer
            SDL3.SDL_UnmapGPUTransferBuffer(graphics.gpuDevice, GPUUploadBuffer);
        }

        public void Write(Action<IntPtr> bufferMemoryAction)
        {
            // Map the upload buffer
            IntPtr dst = SDL3.SDL_MapGPUTransferBuffer(graphics.gpuDevice, GPUUploadBuffer, false);

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
                SDL3.SDL_UnmapGPUTransferBuffer(graphics.gpuDevice, GPUUploadBuffer);
            }
        }

        public GraphicsBuffer Copy()
        {
            // Create the buffer
            GraphicsBuffer copy = new GraphicsBuffer(graphics, size, usage);

            // Map the source buffer memory
            Write((IntPtr src) =>
            {
                // Map the destination buffer memory
                copy.Write((IntPtr dst) =>
                {
                    // Copy the memory
                    SDL3.SDL_memcpy(dst, src, size);
                });
            });

            return copy;
        }
    }
}
