using SDL3;
using System;
using System.Interop;
using System.IO;
using StbImageBeef;
using internal KoraPlayer.Graphics;

namespace KoraPlayer.Graphics;

public enum TextureFormat : c_int
{
	/* Unsigned Normalized Float Color Formats */
	A8_UNORM = 1,
	R8_UNORM,
	R8G8_UNORM,
	R8G8B8A8_UNORM,
	R16_UNORM,
	R16G16_UNORM,
	R16G16B16A16_UNORM,
	R10G10B10A2_UNORM,
	B5G6R5_UNORM,
	B5G5R5A1_UNORM,
	B4G4R4A4_UNORM,
	B8G8R8A8_UNORM,
	/* Compressed Unsigned Normalized Float Color Formats */
	SBC1_RGBA_UNORM,
	SBC2_RGBA_UNORM,
	SBC3_RGBA_UNORM,
	SBC4_R_UNORM,
	SBC5_RG_UNORM,
	SBC7_RGBA_UNORM,
	/* Compressed Signed Float Color Formats */
	BC6H_RGB_FLOAT,
	/* Compressed Unsigned Float Color Formats */
	BC6H_RGB_UFLOAT,
	/* Signed Normalized Float Color Formats  */
	R8_SNORM,
	R8G8_SNORM,
	R8G8B8A8_SNORM,
	R16_SNORM,
	R16G16_SNORM,
	R16G16B16A16_SNORM,
	/* Signed Float Color Formats */
	R16_FLOAT,
	R16G16_FLOAT,
	R16G16B16A16_FLOAT,
	R32_FLOAT,
	R32G32_FLOAT,
	R32G32B32A32_FLOAT,
	/* Unsigned Float Color Formats */
	R11G11B10_UFLOAT,
	/* Unsigned Integer Color Formats */
	R8_UINT,
	R8G8_UINT,
	R8G8B8A8_UINT,
	R16_UINT,
	R16G16_UINT,
	R16G16B16A16_UINT,
	R32_UINT,
	R32G32_UINT,
	R32G32B32A32_UINT,
	/* Signed Integer Color Formats */
	R8_INT,
	R8G8_INT,
	R8G8B8A8_INT,
	R16_INT,
	R16G16_INT,
	R16G16B16A16_INT,
	R32_INT,
	R32G32_INT,
	R32G32B32A32_INT,
	/* SRGB Unsigned Normalized Color Formats */
	R8G8B8A8_UNORM_SRGB,
	B8G8R8A8_UNORM_SRGB,
	/* Compressed SRGB Unsigned Normalized Color Formats */
	BC1_RGBA_UNORM_SRGB,
	BC2_RGBA_UNORM_SRGB,
	BC3_RGBA_UNORM_SRGB,
	BC7_RGBA_UNORM_SRGB,
	/* Depth Formats */
	D16_UNORM,
	D24_UNORM,
	D32_FLOAT,
	D24_UNORM_S8_UINT,
	D32_FLOAT_S8_UINT,
	/* Compressed ASTC Normalized Float Color Formats*/
	ASTC_4x4_UNORM,
	ASTC_5x4_UNORM,
	ASTC_5x5_UNORM,
	ASTC_6x5_UNORM,
	ASTC_6x6_UNORM,
	ASTC_8x5_UNORM,
	ASTC_8x6_UNORM,
	ASTC_8x8_UNORM,
	ASTC_10x5_UNORM,
	ASTC_10x6_UNORM,
	ASTC_10x8_UNORM,
	ASTC_10x10_UNORM,
	ASTC_12x10_UNORM,
	ASTC_12x12_UNORM,
	/* Compressed SRGB ASTC Normalized Float Color Formats*/
	ASTC_4x4_UNORM_SRGB,
	ASTC_5x4_UNORM_SRGB,
	ASTC_5x5_UNORM_SRGB,
	ASTC_6x5_UNORM_SRGB,
	ASTC_6x6_UNORM_SRGB,
	ASTC_8x5_UNORM_SRGB,
	ASTC_8x6_UNORM_SRGB,
	ASTC_8x8_UNORM_SRGB,
	ASTC_10x5_UNORM_SRGB,
	ASTC_10x6_UNORM_SRGB,
	ASTC_10x8_UNORM_SRGB,
	ASTC_10x10_UNORM_SRGB,
	ASTC_12x10_UNORM_SRGB,
	ASTC_12x12_UNORM_SRGB,
	/* Compressed ASTC Signed Float Color Formats*/
	ASTC_4x4_FLOAT,
	ASTC_5x4_FLOAT,
	ASTC_5x5_FLOAT,
	ASTC_6x5_FLOAT,
	ASTC_6x6_FLOAT,
	ASTC_8x5_FLOAT,
	ASTC_8x6_FLOAT,
	ASTC_8x8_FLOAT,
	ASTC_10x5_FLOAT,
	ASTC_10x6_FLOAT,
	ASTC_10x8_FLOAT,
	ASTC_10x10_FLOAT,
	ASTC_12x10_FLOAT,
	ASTC_12x12_FLOAT
}

public enum TextureUsage : uint32
{
	Sampler = (1u << 0), /**< Texture supports sampling. */
	ColorTarget  = (1u << 1), /**< Texture is a color render target. */
	DepthStencilTarget = (1u << 2), /**< Texture is a depth stencil target. */
	GraphicsStorageRead = (1u << 3), /**< Texture supports storage reads in graphics stages. */
	ComputeStorageRead = (1u << 4), /**< Texture supports storage reads in the compute stage. */
	ComputeStorageWrite = (1u << 5), /**< Texture supports storage writes in the compute stage. */
	ComputeStorageReadWrite = (1u << 6), /**< Texture supports reads and writes in the same compute shader. This is NOT equivalent to READ | WRITE. */
}

public enum TextureShape : uint32
{
    Texture2D,
    Texture2DArray,
    Texture3D,
    Cube,
    CubeArray
}

public sealed class Texture : NativeElement
{
	// Private
	private GraphicsDevice device;
	private TextureFormat format = 0;
	private TextureUsage usage = 0;
	private TextureShape shape = 0;
	private uint32 width = 0;
	private uint32 height = 0;
	private uint32 depth = 0;
	private uint32 mipMapLevels = 1;
	private uint32 sizeInBytes = 0;

	// Internal
	internal SDL_GPUDevice* gpuDevice;
	internal SDL_GPUTexture* gpuTexture;
	internal SDL_GPUSampler* gpuSampler;
	internal SDL_GPUTransferBuffer* gpuUploadBuffer;

	// Properties
	public TextureFormat Format => format;
	public TextureUsage Usage => usage;
	public TextureShape Shape => shape;
	public uint32 Width => width;
	public uint32 Height => height;
	public uint32 Depth => depth;
	public uint32 MipMapLevels => mipMapLevels;
	public uint32 SizeInBytes => sizeInBytes;

	// Constructor
	public this(GraphicsDevice device, uint32 width, uint32 height, TextureFormat format = 0, TextureShape shape = .Texture2D, TextureUsage usage = .Sampler, uint32 mipMaplevels = 1)
		: this(device, width, height, 1, format, shape, usage, mipMapLevels)
	{
	}

	public this(GraphicsDevice device, uint32 width, uint32 height, uint32 depth, TextureFormat format = 0, TextureShape shape = .Texture2D, TextureUsage usage = .Sampler, uint32 mipMaplevels = 1)
	{
		this.device = device;
		this.gpuDevice = device.gpuDevice;

		this.format = format == 0 ? device.PreferredFormat : format;
		this.shape = shape;
		this.usage = usage;
		this.width = width;
		this.height = height;
		this.depth = depth;
		this.mipMapLevels = mipMapLevels;

		// Create the texture
		CreateTexture();
	}

	internal ~this()
	{
		// Release the texture
		DestroyTexture();

		// Clear device
		gpuDevice = null;
	}

	// Methods
	public void Resize(uint32 width, uint32 height, uint32 depth = 1)
	{
	    this.width = width;
	    this.height = height;
	    this.depth = depth;

	    // Recreate texture
	    CreateTexture();
	}

	public void Write<T>(Span<T> span)
	{
		// Map the upload buffer
		void* dst = SDL_MapGPUTransferBuffer(gpuDevice, gpuUploadBuffer, false);

		// Copy the memory
		SDL_memcpy(dst, span.Ptr, sizeInBytes);

		// Unmap the pointer
		SDL_UnmapGPUTransferBuffer(gpuDevice, gpuUploadBuffer);
	}

	public void LoadTexture(StringView path)
	{
		// Create file stream
		FileStream fs = scope .();

		// Try to oepn
		fs.Open(path, .Open, .Read);

		// Try to load image
		ImageResult img = ImageResult.FromStream(fs, .RedGreenBlueAlpha);
		defer delete img; // Free after loaded

		// Update format and size
		this.width = (uint32)img.Width;
		this.height = (uint32)img.Height;
		this.format = .R8G8B8A8_UNORM;

		// Initialize the texture
		CreateTexture();

		// Create span
		Span<uint8> mem = .(img.Data, sizeInBytes);

		// Copy the data
		Write(mem);
	}

	private void CreateTexture()
	{
		// Delete old
		DestroyTexture();

		// Setup new texture
		uint32 pixelCount = width * height * depth;
		this.sizeInBytes = GetFormatByteSize(format) * pixelCount;

		// Init the texture
		SDL_GPUTextureCreateInfo textureInfo = SDL_GPUTextureCreateInfo
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
		SDL_GPUSamplerCreateInfo samplerCreateInfo = SDL_GPUSamplerCreateInfo()
		{
			min_filter = .SDL_GPU_FILTER_NEAREST,
			mag_filter = .SDL_GPU_FILTER_NEAREST,
			mipmap_mode = .SDL_GPU_SAMPLERMIPMAPMODE_NEAREST,
			address_mode_u = .SDL_GPU_SAMPLERADDRESSMODE_CLAMP_TO_EDGE,
			address_mode_v = .SDL_GPU_SAMPLERADDRESSMODE_CLAMP_TO_EDGE,
			address_mode_w = .SDL_GPU_SAMPLERADDRESSMODE_CLAMP_TO_EDGE,
		};

		// Create the sampler
		gpuSampler = SDL_CreateGPUSampler(gpuDevice, &samplerCreateInfo);

		// Setup transfer buffer
		SDL_GPUTransferBufferCreateInfo uploadCreateInfo = SDL_GPUTransferBufferCreateInfo
		{
			size = (uint32)sizeInBytes,
			usage = .SDL_GPU_TRANSFERBUFFERUSAGE_UPLOAD,
		};

		// Create the upload buffer
		gpuUploadBuffer = SDL_CreateGPUTransferBuffer(gpuDevice, &uploadCreateInfo);
	}

	private void DestroyTexture()
	{
		// Destroy old
		if(gpuTexture != null)
		{
			SDL_ReleaseGPUTexture(gpuDevice, gpuTexture);
			gpuTexture = null;
		}

		// Destroy sampler
		if(gpuSampler != null)
		{
			SDL_ReleaseGPUSampler(gpuDevice, gpuSampler);
			gpuSampler = null;
		}

		// Destroy upload
		if(gpuUploadBuffer != null)
		{
			SDL_ReleaseGPUTransferBuffer(gpuDevice, gpuUploadBuffer);
			gpuUploadBuffer = null;
		}
	}

	public static uint32 GetFormatByteSize(TextureFormat format)
	{
		switch(format)
		{
			/* Unsigned Normalized Float Color Formats */
		case .A8_UNORM: return 1;
		case .R8_UNORM: return 1;
		case .R8G8_UNORM: return 2;
		case .R8G8B8A8_UNORM: return 4;
		case .R16_UNORM: return 2;
		case .R16G16_UNORM: return 4;
		case .R16G16B16A16_UNORM: return 8;
		case .R10G10B10A2_UNORM: return 4;
		case .B5G6R5_UNORM: return 2;
		case .B5G5R5A1_UNORM: return 2;
		case .B4G4R4A4_UNORM: return 2;
		case .B8G8R8A8_UNORM: return 4;

			/* Compressed Unsigned Normalized Float Color Formats */
		case .SBC1_RGBA_UNORM: return 8;   // BC1, 8 bytes per 4x4 block
		case .SBC2_RGBA_UNORM: return 16;  // BC2, 16 bytes per 4x4 block
		case .SBC3_RGBA_UNORM: return 16;  // BC3, 16 bytes per 4x4 block
		case .SBC4_R_UNORM: return 8;      // BC4, 8 bytes per 4x4 block
		case .SBC5_RG_UNORM: return 16;    // BC5, 16 bytes per 4x4 block
		case .SBC7_RGBA_UNORM: return 16;  // BC7, 16 bytes per 4x4 block

			/* Compressed Signed/Unsigned Float Color Formats */
		case .BC6H_RGB_FLOAT: return 16;
		case .BC6H_RGB_UFLOAT: return 16;

			/* Signed Normalized Float Color Formats */
		case .R8_SNORM: return 1;
		case .R8G8_SNORM: return 2;
		case .R8G8B8A8_SNORM: return 4;
		case .R16_SNORM: return 2;
		case .R16G16_SNORM: return 4;
		case .R16G16B16A16_SNORM: return 8;

			/* Signed Float Color Formats */
		case .R16_FLOAT: return 2;
		case .R16G16_FLOAT: return 4;
		case .R16G16B16A16_FLOAT: return 8;
		case .R32_FLOAT: return 4;
		case .R32G32_FLOAT: return 8;
		case .R32G32B32A32_FLOAT: return 16;

			/* Unsigned Float Color Formats */
		case .R11G11B10_UFLOAT: return 4;

			/* Unsigned Integer Color Formats */
		case .R8_UINT: return 1;
		case .R8G8_UINT: return 2;
		case .R8G8B8A8_UINT: return 4;
		case .R16_UINT: return 2;
		case .R16G16_UINT: return 4;
		case .R16G16B16A16_UINT: return 8;
		case .R32_UINT: return 4;
		case .R32G32_UINT: return 8;
		case .R32G32B32A32_UINT: return 16;

			/* Signed Integer Color Formats */
		case .R8_INT: return 1;
		case .R8G8_INT: return 2;
		case .R8G8B8A8_INT: return 4;
		case .R16_INT: return 2;
		case .R16G16_INT: return 4;
		case .R16G16B16A16_INT: return 8;
		case .R32_INT: return 4;
		case .R32G32_INT: return 8;
		case .R32G32B32A32_INT: return 16;

			/* SRGB Unsigned Normalized Color Formats */
		case .R8G8B8A8_UNORM_SRGB: return 4;
		case .B8G8R8A8_UNORM_SRGB: return 4;

			/* Compressed SRGB Unsigned Normalized Color Formats */
		case .BC1_RGBA_UNORM_SRGB: return 8;
		case .BC2_RGBA_UNORM_SRGB: return 16;
		case .BC3_RGBA_UNORM_SRGB: return 16;
		case .BC7_RGBA_UNORM_SRGB: return 16;

			/* Depth Formats */
		case .D16_UNORM: return 2;
		case .D24_UNORM: return 3; // Typically packed into 4 bytes, but 3 bytes of depth data
		case .D32_FLOAT: return 4;
		case .D24_UNORM_S8_UINT: return 4; // 3 bytes depth + 1 byte stencil
		case .D32_FLOAT_S8_UINT: return 8; // padded to 64-bit alignment

			/* ASTC compressed (bytes per 4x4 block, though actual block size varies) */
		case .ASTC_4x4_UNORM: return 16;
		case .ASTC_5x4_UNORM: return 16;
		case .ASTC_5x5_UNORM: return 16;
		case .ASTC_6x5_UNORM: return 16;
		case .ASTC_6x6_UNORM: return 16;
		case .ASTC_8x5_UNORM: return 16;
		case .ASTC_8x6_UNORM: return 16;
		case .ASTC_8x8_UNORM: return 16;
		case .ASTC_10x5_UNORM: return 16;
		case .ASTC_10x6_UNORM: return 16;
		case .ASTC_10x8_UNORM: return 16;
		case .ASTC_10x10_UNORM: return 16;
		case .ASTC_12x10_UNORM: return 16;
		case .ASTC_12x12_UNORM: return 16;
		case .ASTC_4x4_UNORM_SRGB: return 16;
		case .ASTC_5x4_UNORM_SRGB: return 16;
		case .ASTC_5x5_UNORM_SRGB: return 16;
		case .ASTC_6x5_UNORM_SRGB: return 16;
		case .ASTC_6x6_UNORM_SRGB: return 16;
		case .ASTC_8x5_UNORM_SRGB: return 16;
		case .ASTC_8x6_UNORM_SRGB: return 16;
		case .ASTC_8x8_UNORM_SRGB: return 16;
		case .ASTC_10x5_UNORM_SRGB: return 16;
		case .ASTC_10x6_UNORM_SRGB: return 16;
		case .ASTC_10x8_UNORM_SRGB: return 16;
		case .ASTC_10x10_UNORM_SRGB: return 16;
		case .ASTC_12x10_UNORM_SRGB: return 16;
		case .ASTC_12x12_UNORM_SRGB: return 16;
		case .ASTC_4x4_FLOAT: return 16;
		case .ASTC_5x4_FLOAT: return 16;
		case .ASTC_5x5_FLOAT: return 16;
		case .ASTC_6x5_FLOAT: return 16;
		case .ASTC_6x6_FLOAT: return 16;
		case .ASTC_8x5_FLOAT: return 16;
		case .ASTC_8x6_FLOAT: return 16;
		case .ASTC_8x8_FLOAT: return 16;
		case .ASTC_10x5_FLOAT: return 16;
		case .ASTC_10x6_FLOAT: return 16;
		case .ASTC_10x8_FLOAT: return 16;
		case .ASTC_10x10_FLOAT: return 16;
		case .ASTC_12x10_FLOAT: return 16;
		case .ASTC_12x12_FLOAT: return 16;

		default: return 0;
		}
	}
}