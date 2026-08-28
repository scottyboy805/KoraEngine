using System;
using System.Threading;
using SDL3;
using internal KoraPlayer;
using internal KoraPlayer.Graphics;

namespace KoraPlayer.Graphics;

public struct GraphicsCommand
{
	// Internal
	internal SDL_GPUDevice* gpuDevice;
	internal SDL_GPUCommandBuffer* gpuCommandBuffer;
	internal SDL_GPUCopyPass* gpuCopyPass;
	internal SDL_GPURenderPass* gpuRenderPass;
	internal uint32 renderWidth, renderHeight;
	internal Screen defaultTarget;

	// Properties
	public bool IsValid => gpuCommandBuffer != null;
	public bool IsCopyPass => gpuCopyPass != null;
	public bool IsRenderPass => gpuRenderPass != null;

	public uint32 RenderWidth => renderWidth;
	public uint32 RenderHeight => renderHeight;

	// Constructor
	internal this(SDL_GPUDevice* gpuDevice, SDL_GPUCommandBuffer* gpuCommandBuffer, Screen defaultTarget)
	{
		this.gpuDevice = gpuDevice;
		this.gpuCommandBuffer = gpuCommandBuffer;
		this.gpuCopyPass = null;
		this.gpuRenderPass = null;
		this.renderWidth = 0;
		this.renderHeight = 0;
		this.defaultTarget = defaultTarget;
	}

	// Methods
	public void BindUniform<T>(uint location, ShaderStage stage, in T data) where T : struct
	{
		T value = data;
		BindUniform(location, stage, &value, (uint32)sizeof(T));
	}

	public void BindUniform(uint location, ShaderStage stage, void* data, uint size)
	{
		// Check stage
		switch(stage)
		{
		case .Vertex: SDL_PushGPUVertexUniformData(gpuCommandBuffer, (uint32)location, data, (uint32)size);
		case .Fragment: SDL_PushGPUFragmentUniformData(gpuCommandBuffer, (uint32)location, data, (uint32)size);
		}
	}

	#region CopyPass
	public void BeginCopyPass() mut
	{
	    // Check for any active pass
	    if(CheckActivePass() == false)
			return;

	    // Start the copy
	    gpuCopyPass = SDL3.SDL_BeginGPUCopyPass(gpuCommandBuffer);
	}

	public void EndCopyPass() mut
	{
	    // Check for any
	    if(gpuCopyPass == null)
		{
			Debug.LogError("No copy pass to end");
			return;
		}

	    // End the pass
	    SDL3.SDL_EndGPUCopyPass(gpuCopyPass);
	    gpuCopyPass = null;
	}

	public void UploadBuffer(GraphicsBuffer buffer)
	{
		UploadBuffer(buffer, 0, buffer.Size);
	}

	public void UploadBuffer(GraphicsBuffer buffer, uint32 offset, uint32 size)
	{
		// Check for null
		if (buffer == null)
			return;

		// Check for copy
		if(RequireActiveCopyPass() == false)
			return;

		// Setup transfer location
		SDL_GPUTransferBufferLocation uploadLocation = SDL_GPUTransferBufferLocation
		{
			transfer_buffer = buffer.gpuUploadBuffer,
			offset = offset,
		};

		// Setup upload region
		SDL_GPUBufferRegion uploadRegion = SDL_GPUBufferRegion
		{
			buffer = buffer.gpuBuffer,
			size = size,
			offset = offset,
		};

		// Upload to GPU
		SDL_UploadToGPUBuffer(gpuCopyPass, &uploadLocation, &uploadRegion, true);
	}

	public void UploadMesh(Mesh mesh)
	{
	    // Check for null
	    if(mesh == null)
			return;

	    // Check for copy
		if(RequireActiveCopyPass() == false)
			return;

	    // Copy index
	    if (mesh.HasIndices == true)
	        UploadBuffer(mesh.IndexBuffer);

	    // Copy vertex
	    if (mesh.HasVertices == true)
	        UploadBuffer(mesh.VertexBuffer);
	}

	public void UploadTexture(Texture texture)
	{
	    // Check for null
	    if (texture == null)
			return;

	    // Check for copy
	    if(RequireActiveCopyPass() == false)
			return;

	    // Setup transfer
	    SDL_GPUTextureTransferInfo transferInfo = SDL_GPUTextureTransferInfo
	    {
	        offset = 0,
	        pixels_per_row = texture.Width,
	        rows_per_layer = texture.Height,
	        transfer_buffer = texture.gpuUploadBuffer,
	    };

	    // Setup upload region
	    SDL_GPUTextureRegion regionInfo = SDL_GPUTextureRegion
	    {
	        x = 0,
	        y = 0,
	        z = 0,
	        w = texture.Width,
	        h = texture.Height,
	        d = texture.Depth,
	        texture = texture.gpuTexture,
	    };

	    // Upload to gpu
	    SDL3.SDL_UploadToGPUTexture(gpuCopyPass, &transferInfo, &regionInfo, true);
	}

#endregion

#region RenderPass
	public void BeginRenderPass(Color clearColor, Texture renderTarget = null) mut
	{
		// Check for any active pass
		if(CheckActivePass() == false)
			return;

		SDL_GPUTexture* target = null;

		if(renderTarget == null)
		{
			// Get the swap chain target
			uint32 width = 0, height = 0;
			SDL_WaitAndAcquireGPUSwapchainTexture(gpuCommandBuffer, defaultTarget.window, &target, &width, &height);

			// Update size
			renderWidth = width;
			renderHeight = height;
		}
		else
		{
			// Check for render usage??
			//if((renderTarget.Usage & .ColorTarget) == 0)

			// Get the target texture
			target = renderTarget.gpuTexture;
			renderWidth = renderTarget.Width;
			renderHeight = renderTarget.Height;
		}

		// Create color target
		SDL_GPUColorTargetInfo colorTargetInfo = SDL_GPUColorTargetInfo
		{
			clear_color = clearColor.SDL(),
			load_op = .SDL_GPU_LOADOP_CLEAR,
			store_op = .SDL_GPU_STOREOP_STORE,
			texture = target,
		};

		// Create the render pass
		gpuRenderPass = SDL_BeginGPURenderPass(gpuCommandBuffer, &colorTargetInfo, 1, null);
	}

	public void EndRenderPass() mut
	{
	    // Check for none
	    if (gpuRenderPass == null)
		{
			Debug.LogError("No render pass to end");
			return;
		}

	    // End the pass
	    SDL3.SDL_EndGPURenderPass(gpuRenderPass);
	    gpuRenderPass = null;
		renderWidth = 0;
		renderHeight = 0;
	}

	public void BindShader(Shader shader, MeshPrimitiveType primitiveType, MeshVertexElements elements)
	{
		// Check for null
		if(shader == null)
			return;

		// Check for render pass
		if(RequireActiveRenderPass() == false)
			return;

		// Get the pipeline
		SDL_GPUGraphicsPipeline* gpuPipeline = shader.GetOrCreatePipeline(primitiveType, elements);

		// Bind the pipeline
		SDL_BindGPUGraphicsPipeline(gpuRenderPass, gpuPipeline);
	}

	public void BindMesh(Mesh mesh, uint32 submesh = 0)
	{
		// Check for null
		if(mesh == null)
			return;

		// Check for render pass
		if(RequireActiveRenderPass() == false)
			return;

		// Check for vertices
		if(mesh.HasVertices == true)
		{
			// Bind the vertex buffer
			BindVertexBuffer(mesh.VertexBuffer);
		}

		// Check for indices
		if(mesh.HasIndices == true)
		{
			// Get the index format
			IndexBufferFormat format = mesh.GetIndexFormat(submesh);

			// Bind the index buffer
			BindIndexBuffer(mesh.IndexBuffer, format, 0);
		}
	}

	public void BindTexture(Texture texture, uint32 location)
	{
		// Check for null
		if(texture == null)
			return;

		// Check for render pass
		if(RequireActiveRenderPass() == false)
			return;

		// Create the binding
		SDL_GPUTextureSamplerBinding binding = .
		{
			texture = texture.gpuTexture,
			sampler = texture.gpuSampler,
		};

		// Bind texture
		SDL3.SDL_BindGPUFragmentSamplers(gpuRenderPass, location, &binding, 1);
	}

	public void BindIndexBuffer(GraphicsBuffer buffer, IndexBufferFormat format, uint32 offset = 0)
	{
		// Check for null
		if(buffer == null)
			return;

		// Check for render pass
		if(RequireActiveRenderPass() == false)
			return;

		SDL_GPUBufferBinding bufferBinding = SDL_GPUBufferBinding();
		bufferBinding.buffer = buffer.gpuBuffer;
		bufferBinding.offset = (uint32)offset;

		// Bind index buffer
		SDL_BindGPUIndexBuffer(gpuRenderPass, &bufferBinding, (SDL_GPUIndexElementSize)format);
	}

	public void BindVertexBuffer(GraphicsBuffer buffer, uint32 offset = 0)
	{
		// Check for null
		if(buffer == null)
			return;

		// Check for render pass
		if(RequireActiveRenderPass() == false)
			return;

		SDL_GPUBufferBinding bufferBinding = SDL_GPUBufferBinding();
		bufferBinding.buffer = buffer.gpuBuffer;
		bufferBinding.offset = (uint32)offset;

		// Bind the buffer
		SDL_BindGPUVertexBuffers(gpuRenderPass, 0, &bufferBinding, 1);
	}

	public void DrawPrimitives(uint32 vertexCount, uint32 indexCount, uint32 firstVertex = 0, uint32 firstInstance = 0)
	{
		// Check for render pass
		if(RequireActiveRenderPass() == false)
			return;

		SDL_DrawGPUPrimitives(gpuRenderPass, vertexCount, indexCount, firstVertex, firstInstance);
	}

	public void DrawIndexedPrimitives(uint32 indexCount, uint32 instanceCount, uint32 firstIndex = 0, uint32 firstInstance = 0, uint32 vertexOffset = 0)
	{
		// Check for render pass
		if(RequireActiveRenderPass() == false)
			return;

		SDL_DrawGPUIndexedPrimitives(gpuRenderPass, indexCount, instanceCount, firstIndex, (int32)vertexOffset, (uint32)firstInstance);
	}

	public void Submit()
	{
		// Submit the command buffer
		SDL_SubmitGPUCommandBuffer(gpuCommandBuffer);
	}

	public void SubmitAsync(Async async)
	{
		// Submit and get fence
		SDL3.SDL_GPUFence* gpuFence = SDL3.SDL_SubmitGPUCommandBufferAndAcquireFence(gpuCommandBuffer);

		// Schedule polling
		async.Schedule(scope () =>
		{
			// Wait until done
			while(true)
			{
				// Query the fence
				if(SDL3.SDL_QueryGPUFence(gpuDevice, gpuFence) == true)
					return .Ok;

				// Wait some time
				Thread.Sleep(5);
			}
		});
	}

	private bool CheckActivePass()
	{
	    // Check for already in a copy pass
	    if (gpuCopyPass != null)
		{
	        Debug.LogError("A copy pass is already in progress. End the copy pass before starting a new pass");
			return false;
		}

	    // Check for already in render pass
	    if (gpuRenderPass != null)
		{
			Debug.LogError("A render pass is already in progress. End the render pass before starting a new pass");
			return false;
		}
		return true;
	}

	private bool RequireActiveCopyPass()
	{
		if(gpuCopyPass == null)
		{
			Debug.LogError("You must begin a copy pass for this operation");
			return false;
		}
		return true;
	}

	private bool RequireActiveRenderPass()
	{
		if(gpuRenderPass == null)
		{
			Debug.LogError("You must begin a render pass for this operation");
			return false;
		}
		return true;
	}
#endregion
}