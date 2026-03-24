using SDL;

namespace KoraGame.Graphics
{
    public struct GraphicsCommand
    {
        // Private
        private readonly GraphicsDevice device;

        // Internal
        internal unsafe readonly SDL_GPUCommandBuffer* gpuCommandBuffer;
        internal unsafe SDL_GPUCopyPass* gpuCopyPass;
        internal unsafe SDL_GPURenderPass* gpuRenderPass;        
        internal uint renderWidth, renderHeight;

        // Properties
        public uint RenderWidth
        {
            get
            {
                RequireActiveRenderPass();
                return renderWidth;
            }
        }

        public uint RenderHeight
        {
            get
            {
                RequireActiveRenderPass();
                return renderHeight;
            }
        }

        // Constructor
        internal unsafe GraphicsCommand(GraphicsDevice device, SDL_GPUCommandBuffer* gpuCommandBuffer)
        {
            // Check for null
            if (device == null)
                throw new ArgumentNullException(nameof(device));

            this.device = device;
            this.gpuCommandBuffer = gpuCommandBuffer;
        }

        // Methods
        public unsafe void Submit()
        {
            // Submit the buffer
            SDL3.SDL_SubmitGPUCommandBuffer(gpuCommandBuffer);
        }

        public async Task SubmitAsync()
        {
            IntPtr ptr = IntPtr.Zero;

            unsafe
            {
                // Submit and get the fence
                ptr = (IntPtr)SDL3.SDL_SubmitGPUCommandBufferAndAcquireFence(gpuCommandBuffer);
            }

            // Wait for fence
            while (true)
            {
                bool isComplete;
                unsafe
                {
                    SDL_GPUFence* fence = (SDL_GPUFence*)ptr;
                    isComplete = SDL3.SDL_QueryGPUFence(device.gpuDevice, fence);
                }

                // Check for complete
                if (isComplete)
                    break;

                // Wait some time
                await Task.Delay(1);
            }
        }

        public unsafe void BindUniform<T>(T data, uint location, ShaderStage stage) where T : unmanaged
        {
            // Check stage
            switch (stage)
            {
                case ShaderStage.Vertex:
                    {
                        SDL3.SDL_PushGPUVertexUniformData(gpuCommandBuffer, location, (IntPtr)(&data), (uint)sizeof(T));
                        break;
                    }
                case ShaderStage.Fragment:
                    {
                        SDL3.SDL_PushGPUFragmentUniformData(gpuCommandBuffer, location, (IntPtr)(&data), (uint)sizeof(T));
                        break;
                    }
            }
        }

        #region CopyPass
        public unsafe void BeginCopyPass()
        {
            // Check for any active pass
            CheckActivePass();

            // Start the copy
            gpuCopyPass = SDL3.SDL_BeginGPUCopyPass(gpuCommandBuffer);
        }

        public unsafe void EndCopyPass()
        {
            // Check for any
            if(gpuCopyPass == null)
                throw new InvalidOperationException("No copy pass to end");

            // End the pass
            SDL3.SDL_EndGPUCopyPass(gpuCopyPass);
            gpuCopyPass = null;
        }

        public unsafe void UploadBuffer(GraphicsBuffer buffer)
        {
            // Check for null
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            // Check for copy
            RequireActiveCopyPass();

            // Setup transfer location
            SDL_GPUTransferBufferLocation bufferLocationInfo = new SDL_GPUTransferBufferLocation
            {
                transfer_buffer = buffer.gpuUploadBuffer,
                offset = 0,
            };

            // Setup upload region
            SDL_GPUBufferRegion bufferRegionInfo = new SDL_GPUBufferRegion
            {
                buffer = buffer.gpuBuffer,
                offset = 0,
                size = buffer.Size,
            };

            // Upload to GPU
            SDL3.SDL_UploadToGPUBuffer(gpuCopyPass, &bufferLocationInfo, &bufferRegionInfo, true);
        }

        public void UploadMesh(Mesh mesh)
        {
            // Check for null
            if(mesh == null)
                throw new ArgumentNullException(nameof(mesh));

            // Check for copy
            RequireActiveCopyPass();

            // Copy index
            if (mesh.HasIndices == true)
                UploadBuffer(mesh.IndexBuffer);

            // Copy vertex
            if (mesh.HasVertices == true)
                UploadBuffer(mesh.VertexBuffer);
        }

        public unsafe void UploadTexture(Texture texture)
        {
            // Check for null
            if (texture == null)
                throw new ArgumentNullException(nameof(texture));

            // Check for copy
            RequireActiveCopyPass();

            // Setup transfer
            SDL_GPUTextureTransferInfo transferInfo = new SDL_GPUTextureTransferInfo
            {
                offset = 0,
                pixels_per_row = texture.Width,
                rows_per_layer = texture.Height,
                transfer_buffer = texture.gpuUploadBuffer,
            };

            // Setup upload region
            SDL_GPUTextureRegion regionInfo = new SDL_GPUTextureRegion
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
        public unsafe void BeginRenderPass(Color clearColor, Texture renderTarget = null, Texture depthTarget = null)
        {
            // Check for any active pass
            CheckActivePass();

            // Get the render target
            SDL_GPUTexture* target = null;
            SDL_GPUTexture* depth = null;
            uint width = 0, height = 0; 

            // Check for target
            if(renderTarget == null)
            {
                // Try to get swap chain texture
                bool result = SDL3.SDL_WaitAndAcquireGPUSwapchainTexture(gpuCommandBuffer, device.DefaultRenderTarget.sdlWindow, &target, &width, &height);

                // Check for error
                if (result == false)
                    throw new Exception("Could not acquire swap chain texture");
            }
            else
            {
                // Check for flag
                if ((renderTarget.Usage & TextureUsage.ColorTarget) == 0)
                    throw new InvalidOperationException("Render texture usage must be ColorTarget");

                target = renderTarget.gpuTexture;
                width = renderTarget.Width;
                height = renderTarget.Height;
            }

            // Check for depth
            if(depthTarget == null)
            {
                // Get the default texture
                depth = device.DefaultDepthTarget.gpuTexture;
            }
            else
            {
                // Check for flags
                if ((depthTarget.Usage & TextureUsage.DepthStencilTarget) == 0)
                    throw new InvalidOperationException("Depth texture usage must be DepthStencil");

                // Check format - currently only 32 bit supported
                if (depthTarget.Format != TextureFormat.D32Float)
                    throw new InvalidOperationException("Depth texture format must be D32Float");

                // Check dimensions
                if (depthTarget.Shape != TextureShape.Texture2D || depthTarget.Width != width || depthTarget.Height != height)
                    throw new InvalidOperationException("Depth texture shape and size must match the render target");

                depth = depthTarget.gpuTexture;
            }

            // Create the color target
            SDL_GPUColorTargetInfo colorTargetInfo = new SDL_GPUColorTargetInfo
            {
                clear_color = clearColor.SDL(),
                load_op = SDL_GPULoadOp.SDL_GPU_LOADOP_CLEAR,
                store_op = SDL_GPUStoreOp.SDL_GPU_STOREOP_STORE,
                texture = target,
            };

            // Create the depth target
            SDL_GPUDepthStencilTargetInfo depthTargetInfo = new SDL_GPUDepthStencilTargetInfo
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
        }

        public unsafe void EndRenderPass()
        {
            // Check for none
            if (gpuRenderPass == null)
                throw new InvalidOperationException("No render pass to end");

            // End the pass
            SDL3.SDL_EndGPURenderPass(gpuRenderPass);
            gpuRenderPass = null;
            renderWidth = 0;
            renderHeight = 0;
        }

        public unsafe void BindIndexBuffer(GraphicsBuffer buffer, IndexBufferFormat format, uint offset = 0)
        {
            // Check for null
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            // Check for render pass begin
            RequireActiveRenderPass();

            // Check for index buffer
            if ((buffer.Usage & GraphicsBufferUsage.Index) == 0)
                throw new InvalidOperationException("The specified buffer does not support index buffer usage");

            // Create buffer binding
            SDL_GPUBufferBinding bindingInfo = new SDL_GPUBufferBinding
            {
                buffer = buffer.gpuBuffer,
                offset = offset,
            };

            // Bind the buffer
            SDL3.SDL_BindGPUIndexBuffer(gpuRenderPass, &bindingInfo, (SDL_GPUIndexElementSize)format);
        }

        public unsafe void BindVertexBuffer(GraphicsBuffer buffer, uint offset = 0)
        {
            // Check for null
            if(buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            // Check for render pass begin
            RequireActiveRenderPass();

            // Check for vertex buffer
            if ((buffer.Usage & GraphicsBufferUsage.Vertex) == 0)
                throw new InvalidOperationException("The specified buffer does not support vertex buffer usage");

            // Create buffer binding
            SDL_GPUBufferBinding bindingInfo = new SDL_GPUBufferBinding
            {
                buffer = buffer.gpuBuffer,
                offset = offset,
            };

            // Bind the buffer
            SDL3.SDL_BindGPUVertexBuffers(gpuRenderPass, 0, &bindingInfo, 1);
        }

        public unsafe void BindShader(Shader shader, MeshVertexElements elements)
        {
            // Check for null
            if (shader == null)
                throw new ArgumentNullException(nameof(shader));

            // Check for render pass begin
            RequireActiveRenderPass();

            // Get the pipeline
            SDL_GPUGraphicsPipeline* pipeline = shader.GetOrCreatePipeline(elements);

            // Bind the pipeline
            SDL3.SDL_BindGPUGraphicsPipeline(gpuRenderPass, pipeline);
        }

        public void BindMesh(Mesh mesh, uint subMesh = 0)
        {
            // Check for null
            if(mesh == null)
                throw new ArgumentNullException(nameof(mesh));

            // Check for render pass begin
            RequireActiveRenderPass();

            // Check for vertices
            if (mesh.HasVertices == true)
            {
                // Bind the vertex buffer
                BindVertexBuffer(mesh.VertexBuffer);
            }

            // Check for indices
            if (mesh.HasIndices == true)
            {
                // Get the index format
                IndexBufferFormat format = mesh.GetIndexFormat(subMesh);

                // Bind the index buffer
                BindIndexBuffer(mesh.IndexBuffer, format, 0);
            }
        }

        public unsafe void BindTexture(Texture texture, uint location)
        {
            // Check for null
            if(texture == null)
                throw new ArgumentNullException(nameof(texture));

            // Check for render pass begin
            RequireActiveRenderPass();

            // Create the binding
            SDL_GPUTextureSamplerBinding bindingInfo = new SDL_GPUTextureSamplerBinding
            {
                texture = texture.gpuTexture,
                sampler = texture.gpuSampler,
            };

            // Bind texture
            SDL3.SDL_BindGPUFragmentSamplers(gpuRenderPass, location, &bindingInfo, 1);
        }

        public unsafe void DrawPrimitives(uint vertexCount, uint instanceCount, uint firstVertex = 0, uint firstInstance = 0)
        {
            // Check for render pass begin
            RequireActiveRenderPass();

            // Draw the primitive
            SDL3.SDL_DrawGPUPrimitives(gpuRenderPass, vertexCount, instanceCount, firstVertex, firstInstance);
        }

        public unsafe void DrawIndexedPrimitives(uint indexCount, uint instanceCount, uint firstIndex = 0, uint firstInstance = 0, uint vertexOffset = 0)
        {
            // Check for render pass begin
            RequireActiveRenderPass();

            // Draw the primitive
            SDL3.SDL_DrawGPUIndexedPrimitives(gpuRenderPass, indexCount, instanceCount, firstIndex, (int)vertexOffset, firstInstance);
        }
        #endregion

        private unsafe void RequireActiveCopyPass()
        {
            if (gpuCopyPass == null)
                throw new InvalidOperationException("Can only be called while in a copy pass");
        }

        private unsafe void RequireActiveRenderPass()
        {
            if (gpuRenderPass == null)
                throw new InvalidOperationException("Can only be called while in a render pass");
        }

        private unsafe void CheckActivePass()
        {
            // Check for already in a copy pass
            if (gpuCopyPass != null)
                throw new InvalidOperationException("A copy pass is already in progress. End the copy pass before starting a new pass");

            // Check for already in render pass
            if (gpuRenderPass != null)
                throw new InvalidOperationException("A render pass is already in progress. End the render pass before starting a new pass");
        }
    }
}
