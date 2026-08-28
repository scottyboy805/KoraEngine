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

public class GraphicsBuffer
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
	public this(GraphicsDevice device, uint32 size, GraphicsBufferUsage usage)
	{
		// Setup buffer
		SDL_GPUBufferCreateInfo createInfo = SDL_GPUBufferCreateInfo();
		createInfo.size = (uint32)size;
		createInfo.usage = (SDL_GPUBufferUsageFlags)usage;

		// Create the buffer
		SDL_GPUBuffer* buffer = SDL_CreateGPUBuffer(device.gpuDevice, &createInfo);

		// Setup transfer buffer
		SDL_GPUTransferBufferCreateInfo uploadCreateInfo = SDL_GPUTransferBufferCreateInfo();
		uploadCreateInfo.size = (uint32)size;
		uploadCreateInfo.usage = .SDL_GPU_TRANSFERBUFFERUSAGE_UPLOAD;

		// Create the upload buffer
		SDL_GPUTransferBuffer* uploadBuffer = SDL_CreateGPUTransferBuffer(device.gpuDevice, &uploadCreateInfo);

		this.gpuDevice = device.gpuDevice;
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
	public void Write<T>(Span<T> data, uint32 offset)
	{
		uint32 copySize = size;

		// Map the upload buffer
		void* dst = SDL_MapGPUTransferBuffer(gpuDevice, gpuUploadBuffer, false);

		// Get offset
		dst = ((uint8*)dst) + offset;

		// Copy the memory
		SDL_memcpy(dst, data.Ptr, copySize);

		// Unmap the pointer
		SDL_UnmapGPUTransferBuffer(gpuDevice, gpuUploadBuffer);
	}

	public Span<T> MapMemory<T>()
	{
		return MapMemory<T>(0, size);
	}

	public Span<T> MapMemory<T>(uint offset, uint size)
	{
		// Map the upload buffer
		void* dst = SDL_MapGPUTransferBuffer(gpuDevice, gpuUploadBuffer, false);

		// Get offset
		void* ptr = ((uint8*)dst) + offset;

		// Get the span
		return Span<T>((T*)ptr, (int)size);
	}

	public void UnmapMemory()
	{
		// Unmap the pointer
		SDL_UnmapGPUTransferBuffer(gpuDevice, gpuUploadBuffer);
	}
}