using System.Runtime.InteropServices;

namespace KoraGame.Graphics
{
    // Type
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
        // Properties
        public TextureFormat Format
        {
            get
            {
                binding.CheckNative();
                return Texture_GetFormat(binding.NativePtr);
            }
        }

        public TextureUsage Usage
        {
            get
            {
                binding.CheckNative();
                return Texture_GetUsage(binding.NativePtr);
            }
        }

        public TextureShape Shape
        {
            get
            {
                binding.CheckNative();
                return Texture_GetShape(binding.NativePtr);
            }
        }

        public uint Width
        {
            get
            {
                binding.CheckNative();
                return Texture_GetWidth(binding.NativePtr);
            }
        }

        public uint Height
        {
            get
            {
                binding.CheckNative();
                return Texture_GetHeight(binding.NativePtr);
            }
        }

        public uint Depth
        {
            get
            {
                binding.CheckNative();
                return Texture_GetDepth(binding.NativePtr);
            }
        }

        public uint MipMapLevels
        {
            get
            {
                binding.CheckNative();
                return Texture_GetMipMapLevels(binding.NativePtr);
            }
        }

        public uint SizeInBytes
        {
            get
            {
                binding.CheckNative();
                return Texture_GetSizeInBytes(binding.NativePtr);
            }
        }

        // Constructor
        private Texture(IntPtr nativePtr) 
        {
            binding.NativePtr = nativePtr;
        }

        public Texture(uint width, uint height)
        {
            // Create native
            binding.NativePtr = Texture_CreateNative(binding.ManagedPtr, width, height);
        }

        // Methods

        // Bindings
        [UnmanagedCallersOnly]
        internal static IntPtr Texture_CreateManaged(IntPtr nativePtr)
        {
            // Create new managed texture
            Texture texture = new(nativePtr);

            // Get managed
            return texture.binding.ManagedPtr;
        }

        [DllImport("KoraGame")]
        internal static extern IntPtr Texture_CreateNative(IntPtr managedPtr, uint width, uint height);

        [DllImport("KoraGame")]
        internal static extern TextureFormat Texture_GetFormat(IntPtr nativePtr);

        [DllImport("KoraGame")]
        internal static extern TextureUsage Texture_GetUsage(IntPtr nativePtr);

        [DllImport("KoraGame")]
        internal static extern TextureShape Texture_GetShape(IntPtr nativePtr);

        [DllImport("KoraGame")]
        internal static extern uint Texture_GetWidth(IntPtr nativePtr);

        [DllImport("KoraGame")]
        internal static extern uint Texture_GetHeight(IntPtr nativePtr);

        [DllImport("KoraGame")]
        internal static extern uint Texture_GetDepth(IntPtr nativePtr);

        [DllImport("KoraGame")]
        internal static extern uint Texture_GetMipMapLevels(IntPtr nativePtr);

        [DllImport("KoraGame")]
        internal static extern uint Texture_GetSizeInBytes(IntPtr nativePtr);
    }
}
