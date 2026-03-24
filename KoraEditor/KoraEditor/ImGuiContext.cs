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

                io.Fonts.SetTexID((IntPtr)fontTexture.gpuTexture);
            }

            // Load shader
            shader = await assets.LoadAsync<Shader>("Shader/imgui.shader.json");
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

            // Begin render
            cmd.BeginRenderPass(Color.Black);
            {
                RenderDrawData(drawPtr, cmd);
            }
            // End render
            cmd.EndRenderPass();

            // Submit the command buffer
            cmd.Submit();
        }

        private unsafe void RenderDrawData(ImDrawDataPtr drawPtr, GraphicsCommand cmd)
        {
            // Check for no data or pipeline
            if (drawPtr.CmdListsCount == 0 || shader == null)
                return;

            // Check size
            if(drawPtr.TotalVtxCount > vertexBuffer.Size)
            {
                // TODO - recreate buffer
            }

            if(drawPtr.TotalIdxCount > indexBuffer.Size)
            {
                // TODO - recreate buffer

            }


            // Bind the shader
            cmd.BindShader(shader, MeshVertexElements.Position | MeshVertexElements.UV | MeshVertexElements.Color);

            // Upload buffer data
            int vertexOffset = 0;
            int indexOffset = 0;

            for(int i = 0; i < drawPtr.CmdListsCount; i++)
            {
                // Get list ptr
                ImDrawListPtr listPtr = drawPtr.CmdLists[i];


                // Upload buffers
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


                // Draw commands
                for(int j = 0; j < listPtr.CmdBuffer.Size; j++)
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
                    SDL3.SDL_DrawGPUIndexedPrimitives(cmd.gpuRenderPass, cmdPtr.ElemCount, 1, (uint)(cmdPtr.IdxOffset + indexOffset), (int)(cmdPtr.VtxOffset + vertexOffset), 0);
                }

                indexOffset += listPtr.IdxBuffer.Size;
                vertexOffset += listPtr.VtxBuffer.Size;
            }
        }
    }
}
