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
        public const uint DefaultVertexBufferSize = sizeof(float) * 4098;
        public const uint DefaultIndexBufferSize = sizeof(ushort) * 4098;

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
                int size = sizeof(Color32) * width * height;

                fontTexture = new Texture(graphics, (uint)width, (uint)height, TextureFormat.R8G8B8A8Unorm);
                fontTexture.MapMemory((ptr) =>
                {
                    // Copy to texture
                    Buffer.MemoryCopy(pixels, (void*)ptr, size, size);
                });

                GraphicsCommand uploadCmd = graphics.AcquireCommandBuffer();
                {                                        
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

        public void HandleSDLEvent(SDL_Event e)
        {
            var io = ImGui.GetIO();

            switch (e.Type)
            {
                case SDL_EventType.SDL_EVENT_MOUSE_MOTION:
                    io.AddMousePosEvent(e.motion.x, e.motion.y);
                    break;
                case SDL_EventType.SDL_EVENT_MOUSE_BUTTON_DOWN:
                case SDL_EventType.SDL_EVENT_MOUSE_BUTTON_UP:
                    int b = e.button.Button == SDLButton.SDL_BUTTON_LEFT ? 0 :
                            (e.button.Button == SDLButton.SDL_BUTTON_RIGHT) ? 1 : 2;
                    io.AddMouseButtonEvent(b, e.Type == SDL_EventType.SDL_EVENT_MOUSE_BUTTON_DOWN);
                    break;
                case SDL_EventType.SDL_EVENT_MOUSE_WHEEL:
                    io.AddMouseWheelEvent(e.wheel.x, e.wheel.y);
                    break;
                case SDL_EventType.SDL_EVENT_TEXT_INPUT:
                    io.AddInputCharactersUTF8(e.text.GetText());
                    break;
                case SDL_EventType.SDL_EVENT_KEY_DOWN:
                case SDL_EventType.SDL_EVENT_KEY_UP:
                    ImGuiKey k = GetSDLMappedScancode(e.key.scancode);
                    io.AddKeyEvent(k, e.Type == SDL_EventType.SDL_EVENT_KEY_DOWN);

                    SDL_Keymod mod = SDL3.SDL_GetModState();

                    // update modifiers if needed:
                    io.KeyCtrl = (mod & SDL_Keymod.SDL_KMOD_CTRL) != 0;
                    io.KeyShift = (mod & SDL_Keymod.SDL_KMOD_SHIFT) != 0;
                    io.KeyAlt = (mod & SDL_Keymod.SDL_KMOD_ALT) != 0;
                    io.KeySuper = (mod & SDL_Keymod.SDL_KMOD_GUI) != 0;
                    break;
            }
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

            // Check size (compare required bytes, not element counts)
            uint requiredVertexBytes = (uint)(drawPtr.TotalVtxCount * sizeof(ImDrawVert));
            if (requiredVertexBytes > vertexBuffer.Size)
            {
                // Grow buffer with some headroom to avoid frequent reallocations
                uint newSize = Math.Max(requiredVertexBytes, vertexBuffer.Size * 2);
                if (newSize == 0) newSize = requiredVertexBytes;
                vertexBuffer = new GraphicsBuffer(graphics, GraphicsBufferUsage.Vertex, newSize);
            }

            uint requiredIndexBytes = (uint)(drawPtr.TotalIdxCount * sizeof(ushort));
            if (requiredIndexBytes > indexBuffer.Size)
            {
                uint newSize = Math.Max(requiredIndexBytes, indexBuffer.Size * 2);
                if (newSize == 0) newSize = requiredIndexBytes;
                indexBuffer = new GraphicsBuffer(graphics, GraphicsBufferUsage.Index, newSize);
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

        private static ImGuiKey GetSDLMappedScancode(SDL_Scancode scancode)
        {
            switch (scancode)
            {
                case SDL_Scancode.SDL_SCANCODE_TAB: return ImGuiKey.Tab;
                case SDL_Scancode.SDL_SCANCODE_LEFT: return ImGuiKey.LeftArrow;
                case SDL_Scancode.SDL_SCANCODE_RIGHT: return ImGuiKey.RightArrow;
                case SDL_Scancode.SDL_SCANCODE_UP: return ImGuiKey.UpArrow;
                case SDL_Scancode.SDL_SCANCODE_DOWN: return ImGuiKey.DownArrow;
                case SDL_Scancode.SDL_SCANCODE_PAGEUP: return ImGuiKey.PageUp;
                case SDL_Scancode.SDL_SCANCODE_PAGEDOWN: return ImGuiKey.PageDown;
                case SDL_Scancode.SDL_SCANCODE_HOME: return ImGuiKey.Home;
                case SDL_Scancode.SDL_SCANCODE_END: return ImGuiKey.End;
                case SDL_Scancode.SDL_SCANCODE_INSERT: return ImGuiKey.Insert;
                case SDL_Scancode.SDL_SCANCODE_DELETE: return ImGuiKey.Delete;
                case SDL_Scancode.SDL_SCANCODE_BACKSPACE: return ImGuiKey.Backspace;
                case SDL_Scancode.SDL_SCANCODE_SPACE: return ImGuiKey.Space;
                case SDL_Scancode.SDL_SCANCODE_RETURN: return ImGuiKey.Enter;
                case SDL_Scancode.SDL_SCANCODE_ESCAPE: return ImGuiKey.Escape;

                case SDL_Scancode.SDL_SCANCODE_LCTRL:
                case SDL_Scancode.SDL_SCANCODE_RCTRL: return ImGuiKey.ModCtrl;
                case SDL_Scancode.SDL_SCANCODE_LSHIFT:
                case SDL_Scancode.SDL_SCANCODE_RSHIFT: return ImGuiKey.ModShift;
                case SDL_Scancode.SDL_SCANCODE_LALT:
                case SDL_Scancode.SDL_SCANCODE_RALT: return ImGuiKey.ModAlt;
                case SDL_Scancode.SDL_SCANCODE_LGUI:
                case SDL_Scancode.SDL_SCANCODE_RGUI: return ImGuiKey.ModSuper;

                // Letters
                case SDL_Scancode.SDL_SCANCODE_A: return ImGuiKey.A;
                case SDL_Scancode.SDL_SCANCODE_B: return ImGuiKey.B;
                case SDL_Scancode.SDL_SCANCODE_C: return ImGuiKey.C;
                case SDL_Scancode.SDL_SCANCODE_D: return ImGuiKey.D;
                case SDL_Scancode.SDL_SCANCODE_E: return ImGuiKey.E;
                case SDL_Scancode.SDL_SCANCODE_F: return ImGuiKey.F;
                case SDL_Scancode.SDL_SCANCODE_G: return ImGuiKey.G;
                case SDL_Scancode.SDL_SCANCODE_H: return ImGuiKey.H;
                case SDL_Scancode.SDL_SCANCODE_I: return ImGuiKey.I;
                case SDL_Scancode.SDL_SCANCODE_J: return ImGuiKey.J;
                case SDL_Scancode.SDL_SCANCODE_K: return ImGuiKey.K;
                case SDL_Scancode.SDL_SCANCODE_L: return ImGuiKey.L;
                case SDL_Scancode.SDL_SCANCODE_M: return ImGuiKey.M;
                case SDL_Scancode.SDL_SCANCODE_N: return ImGuiKey.N;
                case SDL_Scancode.SDL_SCANCODE_O: return ImGuiKey.O;
                case SDL_Scancode.SDL_SCANCODE_P: return ImGuiKey.P;
                case SDL_Scancode.SDL_SCANCODE_Q: return ImGuiKey.Q;
                case SDL_Scancode.SDL_SCANCODE_R: return ImGuiKey.R;
                case SDL_Scancode.SDL_SCANCODE_S: return ImGuiKey.S;
                case SDL_Scancode.SDL_SCANCODE_T: return ImGuiKey.T;
                case SDL_Scancode.SDL_SCANCODE_U: return ImGuiKey.U;
                case SDL_Scancode.SDL_SCANCODE_V: return ImGuiKey.V;
                case SDL_Scancode.SDL_SCANCODE_W: return ImGuiKey.W;
                case SDL_Scancode.SDL_SCANCODE_X: return ImGuiKey.X;
                case SDL_Scancode.SDL_SCANCODE_Y: return ImGuiKey.Y;
                case SDL_Scancode.SDL_SCANCODE_Z: return ImGuiKey.Z;

                // Numbers (top row)
                case SDL_Scancode.SDL_SCANCODE_0: return ImGuiKey._0;
                case SDL_Scancode.SDL_SCANCODE_1: return ImGuiKey._1;
                case SDL_Scancode.SDL_SCANCODE_2: return ImGuiKey._2;
                case SDL_Scancode.SDL_SCANCODE_3: return ImGuiKey._3;
                case SDL_Scancode.SDL_SCANCODE_4: return ImGuiKey._4;
                case SDL_Scancode.SDL_SCANCODE_5: return ImGuiKey._5;
                case SDL_Scancode.SDL_SCANCODE_6: return ImGuiKey._6;
                case SDL_Scancode.SDL_SCANCODE_7: return ImGuiKey._7;
                case SDL_Scancode.SDL_SCANCODE_8: return ImGuiKey._8;
                case SDL_Scancode.SDL_SCANCODE_9: return ImGuiKey._9;

                // Function keys
                case SDL_Scancode.SDL_SCANCODE_F1: return ImGuiKey.F1;
                case SDL_Scancode.SDL_SCANCODE_F2: return ImGuiKey.F2;
                case SDL_Scancode.SDL_SCANCODE_F3: return ImGuiKey.F3;
                case SDL_Scancode.SDL_SCANCODE_F4: return ImGuiKey.F4;
                case SDL_Scancode.SDL_SCANCODE_F5: return ImGuiKey.F5;
                case SDL_Scancode.SDL_SCANCODE_F6: return ImGuiKey.F6;
                case SDL_Scancode.SDL_SCANCODE_F7: return ImGuiKey.F7;
                case SDL_Scancode.SDL_SCANCODE_F8: return ImGuiKey.F8;
                case SDL_Scancode.SDL_SCANCODE_F9: return ImGuiKey.F9;
                case SDL_Scancode.SDL_SCANCODE_F10: return ImGuiKey.F10;
                case SDL_Scancode.SDL_SCANCODE_F11: return ImGuiKey.F11;
                case SDL_Scancode.SDL_SCANCODE_F12: return ImGuiKey.F12;

                default: return ImGuiKey.None;
            }
        }
    }
}
