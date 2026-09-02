using System;
using SDL3;
using internal KoraPlayer;

namespace KoraPlayer.Graphics;

public class GraphicsProvider : NativeElement
{
	// Private
	private readonly SDL_Window* screenTarget;
	private readonly Texture depthRenderTarget;
	private readonly TextureFormat preferredFormat = TextureFormat.B8G8R8A8_UNORM;

	private uint renderWidth;
	private uint renderHeight;

	// Internal
	internal readonly SDL_GPUDevice* gpuDevice;
	internal SDL_GPUCommandBuffer* gpuCommandBuffer;
	internal SDL_GPUCopyPass* gpuCopyPass;
	internal SDL_GPURenderPass* gpuRenderPass;

	// Constructor
	public this(SDL_Window* screen = null)
	{
		this.screenTarget = screen;

		// Create the device
		this.gpuDevice = SDL3.SDL_CreateGPUDevice(.SDL_GPU_SHADERFORMAT_SPIRV | .SDL_GPU_SHADERFORMAT_MSL, true, null);

		if (screen != null)
		{
		    // Claim the window
		    SDL3.SDL_ClaimWindowForGPUDevice(gpuDevice, screen);

		    // Get the preferred format
		    this.preferredFormat = (TextureFormat)SDL3.SDL_GetGPUSwapchainTextureFormat(gpuDevice, screen);

			// Get screen dimensions
			int32 w = 0, h = 0;
			SDL_GetWindowSize(screen, &w, &h);

		    // Create depth texture
		    this.depthRenderTarget = new Texture(null, (uint32)w, (uint32)h, .D32_FLOAT, .Texture2D, .DepthStencilTarget, 1);
		}
	}

	public ~this()
	{
		SDL3.SDL_DestroyGPUDevice(gpuDevice);
	}

	// Methods
	public void Submit()
	{
		if(gpuCommandBuffer != null)
		{
			// Submit the buffer
			SDL3.SDL_SubmitGPUCommandBuffer(gpuCommandBuffer);
			gpuCommandBuffer = null;
		}
	}

	public void BindUniform<T>(T data, uint32 location, ShaderStage stage) where T : struct
	{
	    // Check stage
		var data;
	    switch (stage)
	    {
	        case ShaderStage.Vertex:
	            {
	                SDL3.SDL_PushGPUVertexUniformData(gpuCommandBuffer, location, &data, (uint32)sizeof(T));
	                break;
	            }
	        case ShaderStage.Fragment:
	            {
	                SDL3.SDL_PushGPUFragmentUniformData(gpuCommandBuffer, location, &data, (uint32)sizeof(T));
	                break;
	            }
	    }
	}

#region RenderPass
	public int32 BeginRenderPass(Color clearColor, Texture renderTarget = null, Texture depthTarget = null)
	{
	    // Check for any active pass
		if(gpuCopyPass != null || gpuRenderPass != null)
			return 1;

	    // Create command buffer on demand
	    if (gpuCommandBuffer == null)
	        gpuCommandBuffer = SDL3.SDL_AcquireGPUCommandBuffer(gpuDevice);

	    // Get the render target
	    SDL_GPUTexture* target = null;
	    SDL_GPUTexture* depth = null;
	    uint32 width = 0, height = 0;

	    // Check for target
	    if (renderTarget == null)
	    {
	        // Try to get swap chain texture
	        bool result = SDL3.SDL_WaitAndAcquireGPUSwapchainTexture(gpuCommandBuffer, screenTarget, &target, &width, &height);

	        // Check for error
	        if (result == false)
				return 2;
	            //throw new Exception("Could not acquire swap chain texture");
	    }
	    else
	    {
	        // Check for flag
	        if ((renderTarget.Usage & TextureUsage.ColorTarget) == 0)
				return 3;
	            //throw new InvalidOperationException("Render texture usage must be ColorTarget");

	        target = renderTarget.gpuTexture;
	        width = renderTarget.Width;
	        height = renderTarget.Height;
	    }

	    // Check for depth
	    if (depthTarget == null)
	    {
	        // Get the default texture
	        depth = depthRenderTarget.gpuTexture;
	    }
	    else
	    {
	        // Check for flags
	        if ((depthTarget.Usage & TextureUsage.DepthStencilTarget) == 0)
				return 4;
	            //throw new InvalidOperationException("Depth texture usage must be DepthStencil");

	        // Check format - currently only 32 bit supported
	        if (depthTarget.Format != TextureFormat.D32_FLOAT)
				return 5;
	            //throw new InvalidOperationException("Depth texture format must be D32Float");

	        // Check dimensions
	        if (depthTarget.Shape != TextureShape.Texture2D || depthTarget.Width != width || depthTarget.Height != height)
				return 6;
	            //throw new InvalidOperationException("Depth texture shape and size must match the render target");

	        depth = depthTarget.gpuTexture;
	    }

	    // Create the color target
	    SDL_GPUColorTargetInfo colorTargetInfo = SDL_GPUColorTargetInfo
	    {
	        clear_color = clearColor.SDL(),
	        load_op = SDL_GPULoadOp.SDL_GPU_LOADOP_CLEAR,
	        store_op = SDL_GPUStoreOp.SDL_GPU_STOREOP_STORE,
	        texture = target,
	    };

	    // Create the depth target
	    SDL_GPUDepthStencilTargetInfo depthTargetInfo = SDL_GPUDepthStencilTargetInfo
	    {
	        clear_depth = 1f,
	        load_op = SDL_GPULoadOp.SDL_GPU_LOADOP_CLEAR,
	        store_op = SDL_GPUStoreOp.SDL_GPU_STOREOP_STORE,
	        texture = depth,
	    };

	    // Create the render pass
	    this.gpuRenderPass = SDL3.SDL_BeginGPURenderPass(gpuCommandBuffer, &colorTargetInfo, 1, &depthTargetInfo);

	    // Update render pass
	    this.renderWidth = width;
	    this.renderHeight = height;

		return 0;
	}

	public void EndRenderPass()
	{
	    // Check for none
	    if (gpuRenderPass == null)
			return;

	    // End the pass
	    SDL3.SDL_EndGPURenderPass(gpuRenderPass);
	    gpuRenderPass = null;
	    renderWidth = 0;
	    renderHeight = 0;
	}

//	public void BindIndexBuffer(GraphicsBuffer buffer, IndexBufferFormat format, uint offset = 0)
//	{
//	    // Check for null
//	    if (buffer == null)
//	        throw new ArgumentNullException(nameof(buffer));
//
//	    // Check for render pass begin
//	    RequireActiveRenderPass();
//
//	    // Check for index buffer
//	    if ((buffer.Usage & GraphicsBufferUsage.Index) == 0)
//	        throw new InvalidOperationException("The specified buffer does not support index buffer usage");
//
//	    // Create buffer binding
//	    SDL_GPUBufferBinding bindingInfo = new SDL_GPUBufferBinding
//	    {
//	        buffer = buffer.gpuBuffer,
//	        offset = offset,
//	    };
//
//	    // Bind the buffer
//	    SDL3.SDL_BindGPUIndexBuffer(gpuRenderPass, &bindingInfo, (SDL_GPUIndexElementSize)format);
//	}
//
//	public unsafe void BindVertexBuffer(GraphicsBuffer buffer, uint offset = 0)
//	{
//	    // Check for null
//	    if (buffer == null)
//	        throw new ArgumentNullException(nameof(buffer));
//
//	    // Check for render pass begin
//	    RequireActiveRenderPass();
//
//	    // Check for vertex buffer
//	    if ((buffer.Usage & GraphicsBufferUsage.Vertex) == 0)
//	        throw new InvalidOperationException("The specified buffer does not support vertex buffer usage");
//
//	    // Create buffer binding
//	    SDL_GPUBufferBinding bindingInfo = new SDL_GPUBufferBinding
//	    {
//	        buffer = buffer.gpuBuffer,
//	        offset = offset,
//	    };
//
//	    // Bind the buffer
//	    SDL3.SDL_BindGPUVertexBuffers(gpuRenderPass, 0, &bindingInfo, 1);
//	}
//
//	public unsafe void BindStorageBuffer(GraphicsBuffer buffer, ShaderStage stage)
//	{
//	    // Check for null
//	    if (buffer == null)
//	        throw new ArgumentNullException(nameof(buffer));
//
//	    // Check for render pass begin
//	    RequireActiveRenderPass();
//
//	    // Check for storage buffer
//	    if ((buffer.Usage & GraphicsBufferUsage.GraphicsRead) == 0)
//	        throw new InvalidOperationException("The specified buffer does not support storage buffer usage");
//
//	    // Create the buffer array with single element
//	    SDL_GPUBuffer** buffers = stackalloc SDL_GPUBuffer*[1];
//	    buffers[0] = buffer.gpuBuffer;
//
//	    // Check stage
//	    if (stage == ShaderStage.Vertex)
//	    {
//	        SDL3.SDL_BindGPUVertexStorageBuffers(gpuRenderPass, 0, buffers, 1);
//	    }
//	    else
//	    {
//	        SDL3.SDL_BindGPUFragmentStorageBuffers(gpuRenderPass, 0, buffers, 1);
//	    }
//	}
//
//	public unsafe void BindShader(Shader shader, MeshVertexElements elements)
//	{
//	    // Check for null
//	    if (shader == null)
//	        throw new ArgumentNullException(nameof(shader));
//
//	    // Check for render pass begin
//	    RequireActiveRenderPass();
//
//	    // Get the pipeline
//	    SDL_GPUGraphicsPipeline* pipeline = shader.GetOrCreatePipeline(elements);
//
//	    // Bind the pipeline
//	    SDL3.SDL_BindGPUGraphicsPipeline(gpuRenderPass, pipeline);
//	}
//
//	public void BindMesh(Mesh mesh, uint subMesh = 0)
//	{
//	    // Check for null
//	    if (mesh == null)
//	        throw new ArgumentNullException(nameof(mesh));
//
//	    // Check for render pass begin
//	    RequireActiveRenderPass();
//
//	    // Check for vertices
//	    if (mesh.HasVertices == true)
//	    {
//	        // Bind the vertex buffer
//	        BindVertexBuffer(mesh.VertexBuffer);
//	    }
//
//	    // Check for indices
//	    if (mesh.HasIndices == true)
//	    {
//	        // Get the index format
//	        IndexBufferFormat format = mesh.GetIndexFormat(subMesh);
//
//	        // Bind the index buffer
//	        BindIndexBuffer(mesh.IndexBuffer, format, 0);
//	    }
//	}
//
//	public unsafe void BindTexture(Texture texture, uint location)
//	{
//	    // Check for null
//	    if (texture == null)
//	        throw new ArgumentNullException(nameof(texture));
//
//	    // Check for render pass begin
//	    RequireActiveRenderPass();
//
//	    // Create the binding
//	    SDL_GPUTextureSamplerBinding bindingInfo = new SDL_GPUTextureSamplerBinding
//	    {
//	        texture = texture.gpuTexture,
//	        sampler = texture.gpuSampler,
//	    };
//
//	    // Bind texture
//	    SDL3.SDL_BindGPUFragmentSamplers(gpuRenderPass, location, &bindingInfo, 1);
//	}
//
//	public unsafe void DrawPrimitives(uint vertexCount, uint instanceCount, uint firstVertex = 0, uint firstInstance = 0)
//	{
//	    // Check for render pass begin
//	    RequireActiveRenderPass();
//
//	    // Draw the primitive
//	    SDL3.SDL_DrawGPUPrimitives(gpuRenderPass, vertexCount, instanceCount, firstVertex, firstInstance);
//	}
//
//	public unsafe void DrawIndexedPrimitives(uint indexCount, uint instanceCount, uint firstIndex = 0, uint firstInstance = 0, uint vertexOffset = 0)
//	{
//	    // Check for render pass begin
//	    RequireActiveRenderPass();
//
//	    // Draw the primitive
//	    SDL3.SDL_DrawGPUIndexedPrimitives(gpuRenderPass, indexCount, instanceCount, firstIndex, (int)vertexOffset, firstInstance);
//	}

	[Export, CLink]//, CLink]
	public static int32 Graphics_DrawIndexedPrimitives(SDL_GPURenderPass* renderPass, uint32 indexCount, uint32 instanceCount, uint32 firstIndex, uint32 firstInstance, uint32 vertexOffset)
	{
		// Draw the primitive
		SDL3.SDL_DrawGPUIndexedPrimitives(renderPass, indexCount, instanceCount, firstIndex, (int32)vertexOffset, firstInstance);
		return 0;
	}
#endregion
}