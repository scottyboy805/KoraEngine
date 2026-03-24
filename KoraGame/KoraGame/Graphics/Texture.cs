using SDL;
using StbImageSharp;

namespace KoraGame.Graphics
{
    public enum TextureFormat : uint
    {
        /* Unsigned Normalized Float Color Formats */
        A8Unorm = 1,
        R8Unorm,
        R8G8Unorm,
        R8G8B8A8Unorm,
        R16Unorm,
        R16G16Unorm,
        R16G16B16A16Unorm,
        R10G10B10A2Unorm,
        B5G6R5Unorm,
        B5G5R5A1Unorm,
        B4G4R4A4Unorm,
        B8G8R8A8Unorm,
        /* Compressed Unsigned Normalized Float Color Formats */
        Sbc1RgbaUnorm,
        Sbc2RgbaUnorm,
        Sbc3RgbaUnorm,
        Sbc4RUnorm,
        Sbc5RgUnorm,
        Sbc7RgbaUnorm,
        /* Compressed Signed Float Color Formats */
        Bc6hRgbFloat,
        /* Compressed Unsigned Float Color Formats */
        Bc6hRgbUfloat,
        /* Signed Normalized Float Color Formats  */
        R8Snorm,
        R8G8Snorm,
        R8G8B8A8Snorm,
        R16Snorm,
        R16G16Snorm,
        R16G16B16A16Snorm,
        /* Signed Float Color Formats */
        R16Float,
        R16G16Float,
        R16G16B16A16Float,
        R32Float,
        R32G32Float,
        R32G32B32A32Float,
        /* Unsigned Float Color Formats */
        R11G11B10Ufloat,
        /* Unsigned Integer Color Formats */
        R8Uint,
        R8G8Uint,
        R8G8B8A8Uint,
        R16Uint,
        R16G16Uint,
        R16G16B16A16Uint,
        R32Uint,
        R32G32Uint,
        R32G32B32A32Uint,
        /* Signed Integer Color Formats */
        R8Int,
        R8G8Int,
        R8G8B8A8Int,
        R16Int,
        R16G16Int,
        R16G16B16A16Int,
        R32Int,
        R32G32Int,
        R32G32B32A32Int,
        /* SRGB Unsigned Normalized Color Formats */
        R8G8B8A8UnormSrgb,
        B8G8R8A8UnormSrgb,
        /* Compressed SRGB Unsigned Normalized Color Formats */
        Bc1RgbaUnormSrgb,
        Bc2RgbaUnormSrgb,
        Bc3RgbaUnormSrgb,
        Bc7RgbaUnormSrgb,
        /* Depth Formats */
        D16Unorm,
        D24Unorm,
        D32Float,
        D24UnormS8Uint,
        D32FloatS8Uint,
        /* Compressed ASTC Normalized Float Color Formats*/
        Astc4x4Unorm,
        Astc5x4Unorm,
        Astc5x5Unorm,
        Astc6x5Unorm,
        Astc6x6Unorm,
        Astc8x5Unorm,
        Astc8x6Unorm,
        Astc8x8Unorm,
        Astc10x5Unorm,
        Astc10x6Unorm,
        Astc10x8Unorm,
        Astc10x10Unorm,
        Astc12x10Unorm,
        Astc12x12Unorm,
        /* Compressed SRGB ASTC Normalized Float Color Formats*/
        Astc4x4UnormSrgb,
        Astc5x4UnormSrgb,
        Astc5x5UnormSrgb,
        Astc6x5UnormSrgb,
        Astc6x6UnormSrgb,
        Astc8x5UnormSrgb,
        Astc8x6UnormSrgb,
        Astc8x8UnormSrgb,
        Astc10x5UnormSrgb,
        Astc10x6UnormSrgb,
        Astc10x8UnormSrgb,
        Astc10x10UnormSrgb,
        Astc12x10UnormSrgb,
        Astc12x12UnormSrgb,
        /* Compressed ASTC Signed Float Color Formats*/
        Astc4x4Float,
        Astc5x4Float,
        Astc5x5Float,
        Astc6x5Float,
        Astc6x6Float,
        Astc8x5Float,
        Astc8x6Float,
        Astc8x8Float,
        Astc10x5Float,
        Astc10x6Float,
        Astc10x8Float,
        Astc10x10Float,
        Astc12x10Float,
        Astc12x12Float
    }

    [Flags]
    public enum TextureUsage : uint
    {
        Sampler = (1u << 0),
        ColorTarget = (1u << 1),
        DepthStencilTarget = (1u << 2),
        GraphicsStorageRead = (1u << 3),
        ComputeStorageRead = (1u << 4),
        ComputeStorageWrite = (1u << 5),
        ComputeStorageReadWrite = (1u << 6),
    }

    public enum TextureShape
    {
        Texture2D,
        Texture2DArray,
        Texture3D,
        Cube,
        CubeArray
    }

    public enum TextureFilter
    {
        Nearest,
        Linear,
    }

    public enum TextureClamp
    {
        Repeat,
        MirroredRepeat,
        ClampToEdge,
    }

    public sealed class Texture : GameElement
    {
        // Private
        private GraphicsDevice device;
        private TextureFormat format = 0;
        private TextureUsage usage = 0;
        private TextureShape shape = 0;
        private uint width = 0;
        private uint height = 0;
        private uint depth = 0;
        private uint mipMapLevels = 1;
        private uint sizeInBytes = 0;

        // Internal
        internal unsafe SDL_GPUTexture* gpuTexture;
        internal unsafe SDL_GPUSampler* gpuSampler;
        internal unsafe SDL_GPUTransferBuffer* gpuUploadBuffer;

        // Properties
        public TextureFormat Format => format;
        public TextureUsage Usage => usage;
        public TextureShape Shape => shape;
        public uint Width => width;
        public uint Height => height;
        public uint Depth => depth;
        public uint MipMapLevels => mipMapLevels;
        public uint SizeInBytes => sizeInBytes;

        // Constructor
        public Texture(GraphicsDevice device, uint width, uint height, TextureFormat format = 0, uint mipMapLevels = 1, TextureUsage usage = TextureUsage.Sampler, TextureShape shape = TextureShape.Texture2D)
            : this(device, width, height, 1, format, mipMapLevels, usage, shape)
        {
        }

        public Texture(GraphicsDevice device, uint width, uint height, uint depth, TextureFormat format = 0, uint mipMapLevels = 1, TextureUsage usage = TextureUsage.Sampler, TextureShape shape = TextureShape.Texture2D)
        {
            // Check for null
            if (device == null)
                throw new ArgumentNullException(nameof(device));

            // Check format and use preferred by default
            if (format == 0)
                format = device.PreferredFormat;

            this.device = device;
            this.format = format;
            this.usage = usage;
            this.shape = shape;
            this.width = width;
            this.height = height;
            this.depth = depth;
            this.mipMapLevels = mipMapLevels;

            // Create texture
            CreateTexture();
        }

        ~Texture()
        {
            OnDestroy();
        }

        // Methods
        protected override void OnDestroy()
        {
            if(device != null)
            {
                DestroyTexture();

                // Reset object
                format = 0;
                usage = 0;
                shape = 0;
                width = 0;
                height = 0;
                depth = 0;
                mipMapLevels = 0;
                sizeInBytes = 0;
            }
        }

        public void Resize(uint width, uint height, uint depth = 1)
        {
            this.width = width;
            this.height = height;
            this.depth = depth;

            // Recreate texture
            CreateTexture();
        }

        public unsafe void Write(byte[] data)
        {
            // Map the upload buffer
            IntPtr dst = SDL3.SDL_MapGPUTransferBuffer(device.gpuDevice, gpuUploadBuffer, false);

            // Pin the memory
            fixed (byte* ptr = data)
            {
                // Copy the memory
                SDL3.SDL_memcpy(dst, (IntPtr)ptr, sizeInBytes);
            }

            // Unmap the pointer
            SDL3.SDL_UnmapGPUTransferBuffer(device.gpuDevice, gpuUploadBuffer);
        }

        public unsafe void Write(Color32[,] pixels)
        {
            // Map the upload buffer
            IntPtr dst = SDL3.SDL_MapGPUTransferBuffer(device.gpuDevice, gpuUploadBuffer, false);

            // Pin the memory
            fixed (Color32* ptr = pixels)
            {
                // Copy the memory
                SDL3.SDL_memcpy(dst, (IntPtr)ptr, sizeInBytes);
            }

            // Unmap the pointer
            SDL3.SDL_UnmapGPUTransferBuffer(device.gpuDevice, gpuUploadBuffer);
        }

        public unsafe void MapMemory(Action<IntPtr> textureMemoryAction)
        {
            // Map the upload buffer
            IntPtr ptr = SDL3.SDL_MapGPUTransferBuffer(device.gpuDevice, gpuUploadBuffer, false);

            try
            {
                // Attempt to read or write the data
                textureMemoryAction(ptr);
            }
            finally
            {
                // Unmap the pointer
                SDL3.SDL_UnmapGPUTransferBuffer(device.gpuDevice, gpuUploadBuffer);
            }
        }

        public async Task MapMemoryAsync(Func<IntPtr, Task> textureAsyncMemoryAction)
        {            
            IntPtr ptr = IntPtr.Zero;            
            unsafe
            {
                // Map the upload buffer
                ptr = SDL3.SDL_MapGPUTransferBuffer(device.gpuDevice, gpuUploadBuffer, false);
            }

            try
            {
                // Attempt to read or write the data
                await textureAsyncMemoryAction(ptr);
            }
            finally
            {
                unsafe
                {
                    // Unmap the pointer
                    SDL3.SDL_UnmapGPUTransferBuffer(device.gpuDevice, gpuUploadBuffer);
                }
            }
        }

        public unsafe void LoadTexture(string path)
        {
            using (FileStream stream = File.OpenRead(path))
            {
                // Try to load image
                ImageResult img = ImageResult.FromStream(stream, ColorComponents.RedGreenBlue);

                // Update format and size
                this.width = (uint)img.Width;
                this.height = (uint)img.Height;
                this.format = TextureFormat.R8G8B8A8Unorm;

                // Initialize the texture
                CreateTexture();

                // Copy the data
                Write(img.Data);
            }
        }

        private unsafe void CreateTexture()
        {
            // Delete old
            DestroyTexture();

            // Get pixel count and size
            uint pixelCount = width * height * depth;
            this.sizeInBytes = GetFormatByteSize(format) * pixelCount;

            // Init the texture
            SDL_GPUTextureCreateInfo textureInfo = new SDL_GPUTextureCreateInfo
            {
                width = width,
                height = height,
                format = (SDL_GPUTextureFormat)format,
                type = SDL_GPUTextureType.SDL_GPU_TEXTURETYPE_2D,
                usage = (SDL_GPUTextureUsageFlags)usage,
                layer_count_or_depth = depth,
                num_levels = mipMapLevels,
            };

            // Create texture object
            this.gpuTexture = SDL3.SDL_CreateGPUTexture(device.gpuDevice, &textureInfo);

            // Setup sampler
            SDL_GPUSamplerCreateInfo samplerCreateInfo = new SDL_GPUSamplerCreateInfo
            {
                min_filter = SDL_GPUFilter.SDL_GPU_FILTER_NEAREST,
                mag_filter = SDL_GPUFilter.SDL_GPU_FILTER_NEAREST,
                mipmap_mode = SDL_GPUSamplerMipmapMode.SDL_GPU_SAMPLERMIPMAPMODE_NEAREST,
                address_mode_u = SDL_GPUSamplerAddressMode.SDL_GPU_SAMPLERADDRESSMODE_CLAMP_TO_EDGE,
                address_mode_v = SDL_GPUSamplerAddressMode.SDL_GPU_SAMPLERADDRESSMODE_CLAMP_TO_EDGE,
                address_mode_w = SDL_GPUSamplerAddressMode.SDL_GPU_SAMPLERADDRESSMODE_CLAMP_TO_EDGE,
            };

            // Create the sampler
            gpuSampler = SDL3.SDL_CreateGPUSampler(device.gpuDevice, &samplerCreateInfo);

            // Setup transfer buffer
            SDL_GPUTransferBufferCreateInfo uploadCreateInfo = new SDL_GPUTransferBufferCreateInfo
            {
                size = sizeInBytes,
                usage = SDL_GPUTransferBufferUsage.SDL_GPU_TRANSFERBUFFERUSAGE_UPLOAD,
            };

            // Create the upload buffer
            gpuUploadBuffer = SDL3.SDL_CreateGPUTransferBuffer(device.gpuDevice, &uploadCreateInfo);
        }

        private unsafe void DestroyTexture()
        {
            // Destroy old
            if (gpuTexture != null)
            {
                SDL3.SDL_ReleaseGPUTexture(device.gpuDevice, gpuTexture);
                gpuTexture = null;
            }

            // Destroy sampler
            if (gpuSampler != null)
            {
                SDL3.SDL_ReleaseGPUSampler(device.gpuDevice, gpuSampler);
                gpuSampler = null;
            }

            // Destroy upload
            if (gpuUploadBuffer != null)
            {
                SDL3.SDL_ReleaseGPUTransferBuffer(device.gpuDevice, gpuUploadBuffer);
                gpuUploadBuffer = null;
            }
        }

        public static uint GetFormatByteSize(TextureFormat format)
        {
            switch (format)
            {
                /* Unsigned Normalized Float Color Formats */
                case TextureFormat.A8Unorm: return 1;
                case TextureFormat.R8Unorm: return 1;
                case TextureFormat.R8G8Unorm: return 2;
                case TextureFormat.R8G8B8A8Unorm: return 4;
                case TextureFormat.R16Unorm: return 2;
                case TextureFormat.R16G16Unorm: return 4;
                case TextureFormat.R16G16B16A16Unorm: return 8;
                case TextureFormat.R10G10B10A2Unorm: return 4;
                case TextureFormat.B5G6R5Unorm: return 2;
                case TextureFormat.B5G5R5A1Unorm: return 2;
                case TextureFormat.B4G4R4A4Unorm: return 2;
                case TextureFormat.B8G8R8A8Unorm: return 4;

                /* Compressed Unsigned Normalized Float Color Formats */
                case TextureFormat.Sbc1RgbaUnorm: return 8;   // BC1, 8 bytes per 4x4 block
                case TextureFormat.Sbc2RgbaUnorm: return 16;  // BC2, 16 bytes per 4x4 block
                case TextureFormat.Sbc3RgbaUnorm: return 16;  // BC3, 16 bytes per 4x4 block
                case TextureFormat.Sbc4RUnorm: return 8;      // BC4, 8 bytes per 4x4 block
                case TextureFormat.Sbc5RgUnorm: return 16;    // BC5, 16 bytes per 4x4 block
                case TextureFormat.Sbc7RgbaUnorm: return 16;  // BC7, 16 bytes per 4x4 block

                /* Compressed Signed/Unsigned Float Color Formats */
                case TextureFormat.Bc6hRgbFloat: return 16;
                case TextureFormat.Bc6hRgbUfloat: return 16;

                /* Signed Normalized Float Color Formats */
                case TextureFormat.R8Snorm: return 1;
                case TextureFormat.R8G8Snorm: return 2;
                case TextureFormat.R8G8B8A8Snorm: return 4;
                case TextureFormat.R16Snorm: return 2;
                case TextureFormat.R16G16Snorm: return 4;
                case TextureFormat.R16G16B16A16Snorm: return 8;

                /* Signed Float Color Formats */
                case TextureFormat.R16Float: return 2;
                case TextureFormat.R16G16Float: return 4;
                case TextureFormat.R16G16B16A16Float: return 8;
                case TextureFormat.R32Float: return 4;
                case TextureFormat.R32G32Float: return 8;
                case TextureFormat.R32G32B32A32Float: return 16;

                /* Unsigned Float Color Formats */
                case TextureFormat.R11G11B10Ufloat: return 4;

                /* Unsigned Integer Color Formats */
                case TextureFormat.R8Uint: return 1;
                case TextureFormat.R8G8Uint: return 2;
                case TextureFormat.R8G8B8A8Uint: return 4;
                case TextureFormat.R16Uint: return 2;
                case TextureFormat.R16G16Uint: return 4;
                case TextureFormat.R16G16B16A16Uint: return 8;
                case TextureFormat.R32Uint: return 4;
                case TextureFormat.R32G32Uint: return 8;
                case TextureFormat.R32G32B32A32Uint: return 16;

                /* Signed Integer Color Formats */
                case TextureFormat.R8Int: return 1;
                case TextureFormat.R8G8Int: return 2;
                case TextureFormat.R8G8B8A8Int: return 4;
                case TextureFormat.R16Int: return 2;
                case TextureFormat.R16G16Int: return 4;
                case TextureFormat.R16G16B16A16Int: return 8;
                case TextureFormat.R32Int: return 4;
                case TextureFormat.R32G32Int: return 8;
                case TextureFormat.R32G32B32A32Int: return 16;

                /* SRGB Unsigned Normalized Color Formats */
                case TextureFormat.R8G8B8A8UnormSrgb: return 4;
                case TextureFormat.B8G8R8A8UnormSrgb: return 4;

                /* Compressed SRGB Unsigned Normalized Color Formats */
                case TextureFormat.Bc1RgbaUnormSrgb: return 8;
                case TextureFormat.Bc2RgbaUnormSrgb: return 16;
                case TextureFormat.Bc3RgbaUnormSrgb: return 16;
                case TextureFormat.Bc7RgbaUnormSrgb: return 16;

                /* Depth Formats */
                case TextureFormat.D16Unorm: return 2;
                case TextureFormat.D24Unorm: return 3; // Typically packed into 4 bytes, but 3 bytes of depth data
                case TextureFormat.D32Float: return 4;
                case TextureFormat.D24UnormS8Uint: return 4; // 3 bytes depth + 1 byte stencil
                case TextureFormat.D32FloatS8Uint: return 8; // padded to 64-bit alignment

                /* ASTC compressed (bytes per 4x4 block, though actual block size varies) */
                case TextureFormat.Astc4x4Unorm: return 16;
                case TextureFormat.Astc5x4Unorm: return 16;
                case TextureFormat.Astc5x5Unorm: return 16;
                case TextureFormat.Astc6x5Unorm: return 16;
                case TextureFormat.Astc6x6Unorm: return 16;
                case TextureFormat.Astc8x5Unorm: return 16;
                case TextureFormat.Astc8x6Unorm: return 16;
                case TextureFormat.Astc8x8Unorm: return 16;
                case TextureFormat.Astc10x5Unorm: return 16;
                case TextureFormat.Astc10x6Unorm: return 16;
                case TextureFormat.Astc10x8Unorm: return 16;
                case TextureFormat.Astc10x10Unorm: return 16;
                case TextureFormat.Astc12x10Unorm: return 16;
                case TextureFormat.Astc12x12Unorm: return 16;
                case TextureFormat.Astc4x4UnormSrgb: return 16;
                case TextureFormat.Astc5x4UnormSrgb: return 16;
                case TextureFormat.Astc5x5UnormSrgb: return 16;
                case TextureFormat.Astc6x5UnormSrgb: return 16;
                case TextureFormat.Astc6x6UnormSrgb: return 16;
                case TextureFormat.Astc8x5UnormSrgb: return 16;
                case TextureFormat.Astc8x6UnormSrgb: return 16;
                case TextureFormat.Astc8x8UnormSrgb: return 16;
                case TextureFormat.Astc10x5UnormSrgb: return 16;
                case TextureFormat.Astc10x6UnormSrgb: return 16;
                case TextureFormat.Astc10x8UnormSrgb: return 16;
                case TextureFormat.Astc10x10UnormSrgb: return 16;
                case TextureFormat.Astc12x10UnormSrgb: return 16;
                case TextureFormat.Astc12x12UnormSrgb: return 16;
                case TextureFormat.Astc4x4Float: return 16;
                case TextureFormat.Astc5x4Float: return 16;
                case TextureFormat.Astc5x5Float: return 16;
                case TextureFormat.Astc6x5Float: return 16;
                case TextureFormat.Astc6x6Float: return 16;
                case TextureFormat.Astc8x5Float: return 16;
                case TextureFormat.Astc8x6Float: return 16;
                case TextureFormat.Astc8x8Float: return 16;
                case TextureFormat.Astc10x5Float: return 16;
                case TextureFormat.Astc10x6Float: return 16;
                case TextureFormat.Astc10x8Float: return 16;
                case TextureFormat.Astc10x10Float: return 16;
                case TextureFormat.Astc12x10Float: return 16;
                case TextureFormat.Astc12x12Float: return 16;

                default: return 0;
            }
        }
    }
}
