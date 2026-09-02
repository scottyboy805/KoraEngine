using SDL3;
using System.Interop;
using System;
using internal KoraPlayer.Graphics;

namespace KoraPlayer.Graphics;

public enum GraphicsBufferUsage : uint32
{
	Vertex = (1u << 0),
	Index = (1u << 1),
	Indirect = (1u << 2),
	GraphicsRead = (1u << 3),
	ComputeRead = (1u << 4),
	ComputeWrite = (1u << 5),
}

public enum IndexBufferFormat : c_int
{
	Int16,
	Int32,
}

public enum GraphicsBufferError
{
	case Unknown;
	case WriteError;
}

public class GraphicsBuffer : NativeElement
{
	// Private
	private readonly uint32 size;
	private readonly GraphicsBufferUsage usage;

	// Internal
	internal SDL_GPUDevice* gpuDevice;
	internal SDL_GPUBuffer* gpuBuffer;
	internal SDL_GPUTransferBuffer* gpuUploadBuffer;

	// Properties
	public uint32 Size => size;
	public GraphicsBufferUsage Usage => usage;

	// Constructor
	public this(GraphicsDevice device, uint32 size, GraphicsBufferUsage usage){}

	public this(SDL_GPUDevice* gpuDevice, uint32 size, GraphicsBufferUsage usage)
	{
		// Setup buffer
		SDL_GPUBufferCreateInfo createInfo = SDL_GPUBufferCreateInfo();
		createInfo.size = (uint32)size;
		createInfo.usage = (SDL_GPUBufferUsageFlags)usage;

		// Create the buffer
		SDL_GPUBuffer* buffer = SDL_CreateGPUBuffer(gpuDevice, &createInfo);

		// Setup transfer buffer
		SDL_GPUTransferBufferCreateInfo uploadCreateInfo = SDL_GPUTransferBufferCreateInfo();
		uploadCreateInfo.size = (uint32)size;
		uploadCreateInfo.usage = .SDL_GPU_TRANSFERBUFFERUSAGE_UPLOAD;

		// Create the upload buffer
		SDL_GPUTransferBuffer* uploadBuffer = SDL_CreateGPUTransferBuffer(gpuDevice, &uploadCreateInfo);

		this.gpuDevice = gpuDevice;
		this.gpuBuffer = buffer;
		this.gpuUploadBuffer = uploadBuffer;
		this.size = size;
		this.usage = usage;
	}

	public ~this()
	{
		SDL_ReleaseGPUBuffer(gpuDevice, gpuBuffer);
		SDL_ReleaseGPUTransferBuffer(gpuDevice, gpuUploadBuffer);
		gpuDevice = null;
		gpuBuffer = null;
		gpuUploadBuffer = null;
	}

	// Methods
	public void Write<T>(Span<T> data)
	{
		uint32 copySize = size;

		// Map the upload buffer
		void* dst = SDL_MapGPUTransferBuffer(gpuDevice, gpuUploadBuffer, false);

		// Copy the memory
		SDL_memcpy(dst, data.Ptr, copySize);

		// Unmap the pointer
		SDL_UnmapGPUTransferBuffer(gpuDevice, gpuUploadBuffer);
	}

	public Span<T> BeginWrite<T>()
	{
		return BeginWrite<T>(0, size);
	}

	public Span<T> BeginWrite<T>(uint offset, uint size)
	{
		// Map the upload buffer
		void* dst = SDL_MapGPUTransferBuffer(gpuDevice, gpuUploadBuffer, false);

		// Get offset
		void* ptr = ((uint8*)dst) + offset;

		// Get the span
		return Span<T>((T*)ptr, (int)size);
	}

	public void EndWrite()
	{
		// Unmap the pointer
		SDL_UnmapGPUTransferBuffer(gpuDevice, gpuUploadBuffer);
	}

	[Export, CLink]
	public static NativePointer GraphicsBuffer_Create(NativePointer graphics, uint32 size, uint32 usage)
	{
		// Create the buffer
		let buffer = new GraphicsBuffer((SDL_GPUDevice*)graphics, size, (GraphicsBufferUsage)usage);

		// Get the pointer
		return buffer.Ptr;
	}

	[Export, CLink]
	public static void GraphicsBuffer_Destroy(NativePointer ptr)
	{
		// Check for null
		if(ptr == null)
			return;

		// Try to get buffer
		let buffer = Get<GraphicsBuffer>(ptr);

		// Free the buffer
		if(buffer != null)
			delete buffer;
	}

	[Export, CLink]
	public static uint32 GraphicsBuffer_GetSize(NativePointer ptr)
 	{
		 // Try to get buffer
		 let buffer = Get<GraphicsBuffer>(ptr);

		 // Get size or default
		 return buffer != null ? buffer.size : 0;
	}

	[Export, CLink]
	public static uint32 GraphicsBuffer_GetUsage(NativePointer ptr)
	{
		// Try to get buffer
		let buffer = Get<GraphicsBuffer>(ptr);

		// Get size or default
		return buffer != null ? (uint32)buffer.usage : 0;
	}

	[Export, CLink]
	public static void GraphicsBuffer_Write(NativePointer ptr, void* srcMem, uint32 srcSize)
	{
		// Try to get buffer
		let buffer = Get<GraphicsBuffer>(ptr);

		// Try to write to buffer
		if(buffer != null)
			buffer.Write(Span<uint8>((uint8*)srcMem, (int32)srcSize));
	}

	[Export, CLink]
	public static NativePointer GraphicsBuffer_BeginWrite(NativePointer ptr, uint32 offset, uint32 size)
	{
		// Try to get buffer
		let buffer = Get<GraphicsBuffer>(ptr);

		// Try to write to buffer
		return buffer != null
			? buffer.BeginWrite<uint8>(offset, size).Ptr
			: null;
	}

	[Export, CLink]
	public static void GraphicsBuffer_EndWrite(NativePointer ptr)
	{
		// Try to get buffer
		let buffer = Get<GraphicsBuffer>(ptr);

		// Finish write
		if(buffer != null)
			buffer.EndWrite();

	}
}