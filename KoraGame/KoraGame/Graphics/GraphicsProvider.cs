using SDL;

namespace KoraGame.Graphics
{
    public unsafe sealed class GraphicsProvider
    {
        // Private
        private readonly Thread thread;
        private readonly Screen screenRenderTarget;
        private readonly Texture depthRenderTarget;        
        private readonly TextureFormat preferredFormat = TextureFormat.B8G8R8A8Unorm;
        private uint renderWidth;
        private uint renderHeight;

        private Texture whiteTexture = null;
        private Shader defaultShader = null;

        // Internal
        internal readonly SDL_GPUDevice* gpuDevice;
        internal readonly TTF_TextEngine* ttfTextEngine;
        internal SDL_GPUCommandBuffer* gpuCommandBuffer;
        internal SDL_GPUCopyPass* gpuCopyPass;
        internal SDL_GPURenderPass* gpuRenderPass;

        // Properties
        public TextureFormat PreferredFormat => preferredFormat;
        public uint RenderWidth => renderWidth;
        public uint RenderHeight => renderHeight;

        public Texture WhiteTexture => whiteTexture;
        public Shader DefaultShader => defaultShader;

        public bool IsRenderPass => gpuRenderPass != null;
        public bool IsCopyPass => gpuCopyPass != null;

        // Constructor
        public GraphicsProvider(Screen screenRenderTarget = null, Thread thread = null)
        {
            this.thread = thread == null ? Thread.CurrentThread : thread;
            this.screenRenderTarget = screenRenderTarget;

            // Create the device
            this.gpuDevice = SDL3.SDL_CreateGPUDevice(SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_SPIRV | SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_MSL, true, (byte*)null);
            string err = SDL3.SDL_GetError();

            // Create the text engine
            this.ttfTextEngine = SDL3_ttf.TTF_CreateGPUTextEngine(gpuDevice);

            if (screenRenderTarget != null)
            {
                // Claim the window
                SDL3.SDL_ClaimWindowForGPUDevice(gpuDevice, screenRenderTarget.sdlWindow);

                // Get the preferred format
                this.preferredFormat = (TextureFormat)SDL3.SDL_GetGPUSwapchainTextureFormat(gpuDevice, screenRenderTarget.sdlWindow);

                // Create depth texture
                this.depthRenderTarget = new Texture(this, (uint)screenRenderTarget.Width, (uint)screenRenderTarget.Height, TextureFormat.D32Float, 1, TextureUsage.DepthStencilTarget);
            }

            // Create default assets
            InitializeDefaultAssets();
        }

        ~GraphicsProvider()
        {
            SDL3_ttf.TTF_DestroyGPUTextEngine(ttfTextEngine);
            SDL3.SDL_DestroyGPUDevice(gpuDevice);
        }

        // Methods
        public string GetDeviceDriverName()
        {
            return SDL3.SDL_GetGPUDeviceDriver(gpuDevice);
        }

        public void Submit()
        {
            // Check for pass active
            if (gpuRenderPass != null)
                throw new InvalidOperationException("Render pass must be ended first");

            // Check for copy pass
            if (gpuCopyPass != null)
                throw new InvalidOperationException("Copy pass must be ended first");

            if(gpuCommandBuffer != null)
            {
                // Submit the buffer
                SDL3.SDL_SubmitGPUCommandBuffer(gpuCommandBuffer);
                gpuCommandBuffer = null;
            }
        }

        public Task SubmitAsync()
        {
            // Clear buffer
            if (gpuCommandBuffer == null)
                return Task.CompletedTask;

            Submit();
            return Task.CompletedTask;

            //return Task.Run(() =>
            //{
            //    IntPtr ptr = GetFence();
            //    gpuCommandBuffer = null;

            //    // Wait for fence
            //    while (QueryFence(ptr) == false)
            //    {
            //        // Wait some time
            //        await Task.Delay(1);
            //    }
            //});

            //IntPtr ptr = GetFence();
            //gpuCommandBuffer = null;

            //// Wait for fence
            //while (QueryFence(ptr) == false)
            //{
            //    // Wait some time
            //    await Task.Delay(1);
            //}

            //IntPtr GetFence() => (IntPtr)SDL3.SDL_SubmitGPUCommandBufferAndAcquireFence(gpuCommandBuffer);
            //bool QueryFence(IntPtr fence) => SDL3.SDL_QueryGPUFence(gpuDevice, (SDL_GPUFence*)fence);
        }

        public void BindUniform<T>(T data, uint location, ShaderStage stage) where T : unmanaged
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

        #region RenderPass
        public void BeginRenderPass(Color clearColor, Texture renderTarget = null, Texture depthTarget = null)
        {
            // Check for any active pass
            CheckActivePass();

            // Create command buffer on demand
            if (gpuCommandBuffer == null)
                gpuCommandBuffer = SDL3.SDL_AcquireGPUCommandBuffer(gpuDevice);

            // Check for render pass
            if (gpuRenderPass != null)
                throw new InvalidOperationException("A render pass is already in progress");

            // Get the render target
            SDL_GPUTexture* target = null;
            SDL_GPUTexture* depth = null;
            uint width = 0, height = 0;

            // Check for target
            if (renderTarget == null)
            {
                // Try to get swap chain texture
                bool result = SDL3.SDL_WaitAndAcquireGPUSwapchainTexture(gpuCommandBuffer, screenRenderTarget.sdlWindow, &target, &width, &height);

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
            if (depthTarget == null)
            {
                // Get the default texture
                depth = depthRenderTarget.gpuTexture;
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
            if (buffer == null)
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

        public unsafe void BindStorageBuffer(GraphicsBuffer buffer, ShaderStage stage)
        {
            // Check for null
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            // Check for render pass begin
            RequireActiveRenderPass();

            // Check for storage buffer
            if ((buffer.Usage & GraphicsBufferUsage.GraphicsRead) == 0)
                throw new InvalidOperationException("The specified buffer does not support storage buffer usage");

            // Create the buffer array with single element
            SDL_GPUBuffer** buffers = stackalloc SDL_GPUBuffer*[1];
            buffers[0] = buffer.gpuBuffer;

            // Check stage
            if (stage == ShaderStage.Vertex)
            {
                SDL3.SDL_BindGPUVertexStorageBuffers(gpuRenderPass, 0, buffers, 1);
            }
            else
            {
                SDL3.SDL_BindGPUFragmentStorageBuffers(gpuRenderPass, 0, buffers, 1);
            }
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
            if (mesh == null)
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
            if (texture == null)
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

        #region CopyPass
        public void BeginCopyPass()
        {
            // Check for any active pass
            CheckActivePass();

            // Create command buffer on demand
            if (gpuCommandBuffer == null)
                gpuCommandBuffer = SDL3.SDL_AcquireGPUCommandBuffer(gpuDevice);

            // Start the copy
            gpuCopyPass = SDL3.SDL_BeginGPUCopyPass(gpuCommandBuffer);
        }

        public void EndCopyPass()
        {
            // Check for any
            if (gpuCopyPass == null)
                throw new InvalidOperationException("No copy pass to end");

            // End the pass
            SDL3.SDL_EndGPUCopyPass(gpuCopyPass);
            gpuCopyPass = null;
        }

        public void UploadBuffer(GraphicsBuffer buffer)
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
            if (mesh == null)
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

        public void UploadTexture(Texture texture)
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

        private void RequireActiveCopyPass()
        {
            if (gpuCopyPass == null)
                throw new InvalidOperationException("Can only be called while in a copy pass");
        }

        private void RequireActiveRenderPass()
        {
            if (gpuRenderPass == null)
                throw new InvalidOperationException("Can only be called while in a render pass");
        }

        private void CheckActivePass()
        {
            // Check for already in a copy pass
            if (gpuCopyPass != null)
                throw new InvalidOperationException("A copy pass is already in progress. End the copy pass before starting a new pass");

            // Check for already in render pass
            if (gpuRenderPass != null)
                throw new InvalidOperationException("A render pass is already in progress. End the render pass before starting a new pass");
        }

        private void CheckThread()
        {
            if (thread != Thread.CurrentThread)
                throw new InvalidOperationException("Must be called from the same thread that ");
        }

        private void InitializeDefaultAssets()
        {
            try
            {
                // Create white texture
                this.whiteTexture = new Texture(this, 1, 1);
                Color32 white = Color32.White;
                whiteTexture.Write(new Color32[,] { { white } });

                // Create default shader
                byte[] vertexSource = File.ReadAllBytes("vertex.spv");
                byte[] fragmentSource = File.ReadAllBytes("fragment.spv");
                defaultShader = new Shader(this, vertexSource, fragmentSource, ShaderFormat.Spirv);

                // Upload the assets
                BeginCopyPass();
                {
                    UploadTexture(whiteTexture);
                }
                EndCopyPass();
                Submit();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
