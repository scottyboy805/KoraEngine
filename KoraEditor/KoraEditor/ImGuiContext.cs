using ImGuiNET;
using KoraGame;
using KoraGame.Graphics;
using SDL;
using System.Numerics;

namespace KoraEditor
{
    internal sealed class ImGuiContext
    {
        // Private
        private GraphicsDevice graphics;
        private GraphicsBuffer vertexBuffer;
        private GraphicsBuffer indexBuffer;
        private Texture fontTexture;
        private Shader shader;
        private Material material;

        // Public
        public const uint DefaultVertexBufferSize = sizeof(float) * 5 * 4098;
        public const uint DefaultIndexBufferSize = sizeof(ushort) * 2048;

        // Methods
        public async void Initialize(GraphicsDevice graphics, AssetProvider assets)
        {
            this.graphics = graphics;

            ImGui.CreateContext();
            ImGuiIOPtr io = ImGui.GetIO();
            io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;

            // Go to dark style
            ImGui.StyleColorsDark();


            // Create buffers
            vertexBuffer = new GraphicsBuffer(graphics, GraphicsBufferUsage.Vertex, DefaultVertexBufferSize);
            indexBuffer = new GraphicsBuffer(graphics, GraphicsBufferUsage.Index, DefaultIndexBufferSize);

            unsafe
            {
                // Get font texture info
                io.Fonts.GetTexDataAsRGBA32(out byte* pixels, out int width, out int height);

                // Create texture
                int size = sizeof(byte) * width * height;

                fontTexture = new Texture(graphics, (uint)width, (uint)height, TextureFormat.R8G8B8A8Unorm);
                fontTexture.MapMemory((ptr) =>
                {
                    // Copy to texture
                    Buffer.MemoryCopy(pixels, (void*)ptr, size, size);
                });

                // Upload the font texture to the GPU so the sampler will read valid contents
                // The pixel data was written into the texture's upload buffer by MapMemory above,
                // but we must perform an explicit copy pass to transfer it to the device-local texture.
                {
                    GraphicsCommand uploadCmd = graphics.AcquireCommandBuffer();
                    uploadCmd.BeginCopyPass();
                    uploadCmd.UploadTexture(fontTexture);
                    uploadCmd.EndCopyPass();
                    uploadCmd.Submit();
                }

                io.Fonts.SetTexID((IntPtr)fontTexture.gpuTexture);
            }

            // Load shader
            shader = await assets.LoadAsync<Shader>("Shader/imgui.shader.json");

            // Create material
            material = new Material();
            material.Name = "ImGui Material";
            material.Shader = shader;
            material.SetTexture("FontTexture", fontTexture);
        }

        public void BeginFrame()
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.DisplaySize = new Vector2(graphics.DefaultRenderTarget.Width, graphics.DefaultRenderTarget.Height);
            io.DeltaTime = 1f / 60f;

            ImGui.NewFrame();
            ImGui.ShowDemoWindow();
        }

        public void EndFrame()
        {
            ImGui.Render();
            ImDrawDataPtr drawPtr = ImGui.GetDrawData();

            float bufferWidth = drawPtr.DisplaySize.X * drawPtr.FramebufferScale.X;
            float bufferHeight = drawPtr.DisplaySize.Y * drawPtr.FramebufferScale.Y;

            // Don't render if minimized
            if (bufferWidth <= 0f || bufferHeight <= 0f)
                return;

            // Get command buffer
            GraphicsCommand cmd = graphics.AcquireCommandBuffer();
            // Scale clip rects now (done before uploading)
            drawPtr.ScaleClipRects(ImGui.GetIO().DisplayFramebufferScale);

            // Render (this will perform any required buffer uploads and the render pass)
            RenderDrawData(drawPtr, cmd);

            // Submit the command buffer
            cmd.Submit();
        }

        private unsafe void RenderDrawData(ImDrawDataPtr drawPtr, GraphicsCommand cmd)
        {
            // Check for no data or pipeline
            if (drawPtr.CmdListsCount == 0 || shader == null)
                return;

            // Check size
            if (drawPtr.TotalVtxCount > vertexBuffer.Size)
            {
                // TODO - recreate buffer
                return;
            }

            if (drawPtr.TotalIdxCount > indexBuffer.Size)
            {
                // TODO - recreate buffer
                return;
            }

            Matrix4F mat = Matrix4F.Orthographic(0.0f,
                ImGui.GetIO().DisplaySize.X,
                ImGui.GetIO().DisplaySize.Y,
                0.0f,
                -1.0f,
                1.0f);

            // Upload buffer data into the upload buffers first
            int vertexOffset = 0;
            int indexOffset = 0;

            for (int i = 0; i < drawPtr.CmdListsCount; i++)
            {
                // Get list ptr
                ImDrawListPtr listPtr = drawPtr.CmdLists[i];

                // Upload into the mapped upload buffer memory
                vertexBuffer.MapMemory((ptr) =>
                {
                    // Get list offset and size
                    byte* basePtr = ((byte*)ptr) + (vertexOffset * sizeof(ImDrawVert));
                    int size = listPtr.VtxBuffer.Size * sizeof(ImDrawVert);

                    // Copy memory
                    Buffer.MemoryCopy((void*)listPtr.VtxBuffer.Data, basePtr, size, size);
                });

                indexBuffer.MapMemory((ptr) =>
                {
                    // Get list offset and size
                    byte* basePtr = ((byte*)ptr) + (indexOffset * sizeof(ushort));
                    int size = listPtr.IdxBuffer.Size * sizeof(ushort);

                    // Copy memory
                    Buffer.MemoryCopy((void*)listPtr.IdxBuffer.Data, basePtr, size, size);
                });

                indexOffset += listPtr.IdxBuffer.Size;
                vertexOffset += listPtr.VtxBuffer.Size;
            }

            // Perform a copy pass to upload the transfer buffers to GPU-resident buffers
            cmd.BeginCopyPass();
            {
                cmd.UploadBuffer(vertexBuffer);
                cmd.UploadBuffer(indexBuffer);
            }
            cmd.EndCopyPass();

            // Begin render pass now that buffers are on the GPU
            cmd.BeginRenderPass(Color.CornflowerBlue);
            {
                // Set the viewport
                unsafe
                {
                    SDL_GPUViewport viewport = new SDL_GPUViewport
                    {
                        x = 0,
                        y = 0,
                        w = ImGui.GetIO().DisplaySize.X,
                        h = ImGui.GetIO().DisplaySize.Y,
                        min_depth = 0f,
                        max_depth = 1f
                    };
                    SDL3.SDL_SetGPUViewport(cmd.gpuRenderPass, &viewport);
                }

                // Bind buffers and pipeline
                cmd.BindVertexBuffer(vertexBuffer);
                cmd.BindIndexBuffer(indexBuffer, IndexBufferFormat.Int16);
                material.Bind(cmd, MeshVertexElements.Position2 | MeshVertexElements.UV | MeshVertexElements.Color32);

                // Use a System.Numerics matrix for the orthographic projection to match common shader expectations
                Matrix4x4 mvp = Matrix4x4.CreateOrthographicOffCenter(
                    0.0f,
                    ImGui.GetIO().DisplaySize.X,
                    ImGui.GetIO().DisplaySize.Y,
                    0.0f,
                    -1.0f,
                    1.0f
                );

                cmd.BindUniform(mvp, 0, ShaderStage.Vertex);

                // Draw from the GPU buffers
                vertexOffset = 0;
                indexOffset = 0;

                for (int i = 0; i < drawPtr.CmdListsCount; i++)
                {
                    ImDrawListPtr listPtr = drawPtr.CmdLists[i];

                    for (int j = 0; j < listPtr.CmdBuffer.Size; j++)
                    {
                        ImDrawCmdPtr cmdPtr = listPtr.CmdBuffer[j];
                        Vector4 clip = cmdPtr.ClipRect;

                        // Set clip
                        SDL_Rect clipRect = new SDL_Rect
                        {
                            x = (int)clip.X,
                            y = (int)clip.Y,
                            w = (int)(clip.Z - clip.X),
                            h = (int)(clip.W - clip.Y),
                        };
                        SDL3.SDL_SetGPUScissor(cmd.gpuRenderPass, &clipRect);

                        // Bind the texture for this draw call. ImGui uses TextureId to reference the font atlas
                        // (we set it earlier with io.Fonts.SetTexID). If other textures are used they should be
                        // resolved similarly from an application texture registry.
                        if (cmdPtr.TextureId != IntPtr.Zero)
                        {
                            if ((IntPtr)fontTexture.gpuTexture == cmdPtr.TextureId)
                            {
                                cmd.BindTexture(fontTexture, 0);
                            }
                        }

                        // Draw
                        SDL3.SDL_DrawGPUIndexedPrimitives(cmd.gpuRenderPass, cmdPtr.ElemCount, 1, (uint)(cmdPtr.IdxOffset + indexOffset), (int)(cmdPtr.VtxOffset + vertexOffset), 0);
                    }

                    indexOffset += listPtr.IdxBuffer.Size;
                    vertexOffset += listPtr.VtxBuffer.Size;
                }
            }
            // End render pass
            cmd.EndRenderPass();
        }
    }
}
