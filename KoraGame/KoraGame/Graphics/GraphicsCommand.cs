using SDL;
using System.Runtime.InteropServices;

namespace KoraGame.Graphics
{
    public sealed class GraphicsCommand
    {
        // Type
        [StructLayout(LayoutKind.Sequential)]
        internal struct TransformUniform
        {
            public Matrix4F ViewMatrix;
            public Matrix4F ProjectionMatrix;
            public Matrix4F ModelMatrix;
        }

        // Internal
        private readonly Thread graphicsThread;
        private readonly GraphicsDevice graphicsProvider;
        private unsafe SDL_GPUCommandBuffer* gpuCommandBuffer;
        private unsafe SDL_GPUCopyPass* gpuCopyPass;
        private unsafe SDL_GPURenderPass* gpuRenderPass;
        private GraphicsBatch batch;
        private Matrix4F viewMatrix;
        private Matrix4F projectionMatrix;    

        // Properties
        public unsafe bool IsCopyPass => gpuCopyPass != null;
        public unsafe bool IsRenderPass => gpuRenderPass != null;

        // Constructor
        internal unsafe GraphicsCommand(Thread graphicsThread, GraphicsDevice graphicsProvider)
        {
            this.graphicsThread = graphicsThread;
            this.graphicsProvider = graphicsProvider;
        }

        // Methods
        internal unsafe void Begin(SDL_GPUCommandBuffer* gpuCommandBuffer)
        {
            this.gpuCommandBuffer = gpuCommandBuffer;
        }

        public unsafe void Submit()
        {
            // Check thread
            RequireCommandBuffer(nameof(SubmitAsync));

            // Check for pass active
            if (gpuRenderPass != null)
                throw new InvalidOperationException("Render pass must be ended first");

            // Check for copy pass
            if (gpuCopyPass != null)
                throw new InvalidOperationException("Copy pass must be ended first");

            if (gpuCommandBuffer != null)
            {
                // Submit the buffer
                SDL3.SDL_SubmitGPUCommandBuffer(gpuCommandBuffer);
                gpuCommandBuffer = null;
            }
        }

        public async Task SubmitAsync()
        {
            // Check thread
            RequireCommandBuffer(nameof(SubmitAsync));

            IntPtr ptr = IntPtr.Zero;

            unsafe
            {
                // Submit and get the fence
                ptr = (IntPtr)SDL3.SDL_SubmitGPUCommandBufferAndAcquireFence(gpuCommandBuffer);
                gpuCommandBuffer = null;
            }            

            // Wait for fence
            while (true)
            {
                bool isComplete;
                unsafe
                {
                    SDL_GPUFence* fence = (SDL_GPUFence*)ptr;
                    isComplete = SDL3.SDL_QueryGPUFence(graphicsProvider.gpuDevice, fence);
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
            RequireCommandBuffer(nameof(BindUniform));

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
            CheckActivePass(nameof(BeginCopyPass));

            // Start the copy
            gpuCopyPass = SDL3.SDL_BeginGPUCopyPass(gpuCommandBuffer);
        }

        public unsafe void EndCopyPass()
        {
            // Check thread
            RequireCommandBuffer(nameof(EndCopyPass));

            // Check for any
            if (gpuCopyPass == null)
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
            RequireActiveCopyPass(nameof(UploadBuffer));

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
            RequireActiveCopyPass(nameof(UploadMesh));

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
            RequireActiveCopyPass(nameof(UploadTexture));

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
        public unsafe void BeginRenderPass(Color clearColor, Matrix4F? viewMatrix = null, Matrix4F? projectionMatrix = null, Texture renderTarget = null, Texture depthTarget = null, GraphicsBatch batch = null)
        {
            // Check for any active pass
            CheckActivePass(nameof(BeginRenderPass));

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
                bool result = SDL3.SDL_WaitAndAcquireGPUSwapchainTexture(gpuCommandBuffer, graphicsProvider.DefaultRenderTarget.sdlWindow, &target, &width, &height);

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
                depth = graphicsProvider.DefaultDepthTarget.gpuTexture;
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
            this.batch = batch;
            this.viewMatrix = viewMatrix != null ? viewMatrix.Value : Matrix4F.Identity;
            this.projectionMatrix = projectionMatrix != null ? projectionMatrix.Value : Matrix4F.Identity;
        }

        public unsafe void EndRenderPass()
        {
            // Check for buffer
            RequireCommandBuffer(nameof(EndRenderPass));

            // Check for none
            if (gpuRenderPass == null)
                throw new InvalidOperationException("No render pass to end");

            // End the batch
            if (batch != null && batch.IsEmpty == false)
                batch.Execute(this, viewMatrix, projectionMatrix);

            // End the pass
            SDL3.SDL_EndGPURenderPass(gpuRenderPass);
            gpuRenderPass = null;
            batch = null;
        }

        public unsafe void BindIndexBuffer(GraphicsBuffer buffer, IndexBufferFormat format, uint offset = 0)
        {
            // Check for null
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            // Check for render pass begin
            RequireActiveRenderPass(nameof(BindIndexBuffer));

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
            RequireActiveRenderPass(nameof(BindVertexBuffer));

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
            RequireActiveRenderPass(nameof(BindStorageBuffer));

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
            RequireActiveRenderPass(nameof(BindShader));

            // Get the pipeline
            SDL_GPUGraphicsPipeline* pipeline = shader.GetOrCreatePipeline(elements);

            // Bind the pipeline
            SDL3.SDL_BindGPUGraphicsPipeline(gpuRenderPass, pipeline);
        }

        public unsafe void BindMesh(Mesh mesh, uint subMesh = 0)
        {
            // Check for null
            if (mesh == null)
                throw new ArgumentNullException(nameof(mesh));

            // Check for render pass begin
            RequireActiveRenderPass(nameof(BindMesh));

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
            RequireActiveRenderPass(nameof(BindTexture));

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
            RequireActiveRenderPass(nameof(DrawPrimitives));

            // Draw the primitive
            SDL3.SDL_DrawGPUPrimitives(gpuRenderPass, vertexCount, instanceCount, firstVertex, firstInstance);
        }

        public unsafe void DrawIndexedPrimitives(uint indexCount, uint instanceCount, uint firstIndex = 0, uint firstInstance = 0, uint vertexOffset = 0)
        {
            // Check for render pass begin
            RequireActiveRenderPass(nameof(DrawIndexedPrimitives));

            // Draw the primitive
            SDL3.SDL_DrawGPUIndexedPrimitives(gpuRenderPass, indexCount, instanceCount, firstIndex, (int)vertexOffset, firstInstance);
        }

        public void Draw(Matrix4F matrix, Material material, GraphicsBuffer vertexBuffer, MeshVertexElements elements, uint offset, uint size)
        {
            // Check null
            if (vertexBuffer == null)
                throw new ArgumentNullException(nameof(vertexBuffer));

            // Check for render pass begin
            RequireActiveRenderPass(nameof(Draw));

            // Check material
            if (material == null)
            {
                // Select error material
#if DEBUG
                material = graphicsProvider.ErrorMaterial;
#else
                return;
#endif
            }

            // Check for batch
            if (batch != null)
            {
                // Push the command
                batch.PushDraw(matrix, material, vertexBuffer, elements, offset, size);

                // Check for execute
                if (batch.IsFull == true)
                    batch.Execute(this, viewMatrix, projectionMatrix);
            }
            else
            {
                // Bind transform
                TransformUniform transform = new ()
                {
                    ViewMatrix = viewMatrix,
                    ProjectionMatrix = projectionMatrix,
                    ModelMatrix = matrix,
                };

                // Bind transform
                BindUniform(transform, 0, ShaderStage.Vertex);

                // Bind material
                material.Bind(this, elements);

                // Bind vertex
                BindVertexBuffer(vertexBuffer);

                // Draw primitives
                DrawPrimitives(size, 1, offset);
            }
        }

        public void Draw(Matrix4F matrix, Material material, Mesh mesh, uint subMeshOffset = 0, uint subMeshCount = 1)
        {
            // Check null
            if (mesh == null)
                throw new ArgumentNullException(nameof(mesh));

            // Check for render pass begin
            RequireActiveRenderPass(nameof(Draw));

            // Check for no vertices
            if (mesh.HasVertices == false)
                return;

            // Check material
            if (material == null)
            {
                // Select error material
#if DEBUG
                material = graphicsProvider.ErrorMaterial;
#else
                return;
#endif
            }

            // Draw all submesh
            for (uint i = subMeshOffset; i < subMeshOffset + subMeshCount; i++)
            {
                // Get elements
                MeshVertexElements elements = mesh.GetVertexElements(i);

                // Get index format
                IndexBufferFormat indexFormat = mesh.GetIndexFormat(i);

                // Get elements
                mesh.GetElements(out uint indexOffset, out uint vertexOffset, out uint elementCount, i);

                // Check for batch
                if (batch != null)
                {
                    // Push the command
                    if (mesh.HasIndices == true)
                    {
                        batch.PushDrawIndexed(matrix, material, mesh.VertexBuffer, elements, mesh.IndexBuffer, indexFormat, indexOffset, vertexOffset, elementCount);
                    }
                    else
                    {
                        batch.PushDraw(matrix, material, mesh.VertexBuffer, elements, vertexOffset, elementCount);
                    }

                    // Check for execute
                    if (batch.IsFull == true)
                        batch.Execute(this, viewMatrix, projectionMatrix);
                }
                else
                {
                    // Bind transform
                    TransformUniform transform = new()
                    {
                        ViewMatrix = viewMatrix,
                        ProjectionMatrix = projectionMatrix,
                        ModelMatrix = matrix,
                    };

                    // Bind transform
                    BindUniform(transform, 0, ShaderStage.Vertex);

                    // Bind material
                    material.Bind(this, elements);

                    // Bind index 
                    if (mesh.HasIndices == true)
                        BindIndexBuffer(mesh.IndexBuffer, indexFormat, indexOffset);

                    // Bind vertex
                    BindVertexBuffer(mesh.VertexBuffer);

                    // Draw primitives
                    if(mesh.HasIndices == true)
                    {
                        DrawIndexedPrimitives(elementCount, 1, indexOffset, 0, vertexOffset);
                    }
                    else
                    {
                        DrawPrimitives(elementCount, 1, vertexOffset);
                    }
                }
            }
        }
#endregion

        private unsafe void CheckActivePass(string name)
        {
            // Check thread and buffer
            RequireCommandBuffer(name);

            // Check for already in a copy pass
            if (gpuCopyPass != null)
                throw new InvalidOperationException("A copy pass is already in progress. End the copy pass before starting a new pass");

            // Check for already in render pass
            if (gpuRenderPass != null)
                throw new InvalidOperationException("A render pass is already in progress. End the render pass before starting a new pass");
        }

        private unsafe void RequireActiveCopyPass(string name)
        {
            // Check thread and buffer
            RequireCommandBuffer(name);

            if (gpuCopyPass == null)
                throw new InvalidOperationException("Can only be called while in a copy pass");
        }

        private unsafe void RequireActiveRenderPass(string name)
        {
            // Check thread and buffer
            RequireCommandBuffer(name);

            if (gpuRenderPass == null)
                throw new InvalidOperationException("Can only be called while in a render pass");
        }

        private unsafe void RequireCommandBuffer(string name)
        {
            // Check thread
            CheckThread(name);

            if (gpuCommandBuffer == null)
                throw new InvalidOperationException("The graphics were already submitted");
        }

        private void CheckThread(string name)
        {
            return;
            if (graphicsThread?.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
                throw new InvalidOperationException($"{name} can only be executed from the thread that acquired the graphics");
        }
    }
}
