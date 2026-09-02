using SDL;

namespace KoraGame.Graphics
{
    public sealed class GraphicsDevice
    {
        // Private
        private readonly Screen screenRenderTarget;
        private readonly Texture depthRenderTarget;        
        private readonly TextureFormat preferredFormat = TextureFormat.B8G8R8A8Unorm;
        private readonly ThreadLocal<GraphicsCommand> graphicsCommands;
        
        private Texture whiteTexture = null;
        private Material errorMaterial = null;

        // Internal
        internal unsafe readonly SDL_GPUDevice* gpuDevice;
        internal unsafe readonly TTF_TextEngine* ttfTextEngine;

        // Properties
        public uint RenderWidth => screenRenderTarget != null ? (uint)screenRenderTarget.Width : 0;
        public uint RenderHeght => screenRenderTarget != null ? (uint)screenRenderTarget.Height : 0;
        public TextureFormat PreferredFormat => preferredFormat;
        public Texture WhiteTexture => whiteTexture;
        public Material ErrorMaterial => errorMaterial;

        internal Screen DefaultRenderTarget => screenRenderTarget;
        internal Texture DefaultDepthTarget => depthRenderTarget;

        // Constructor
        public unsafe GraphicsDevice(Screen screenRenderTarget = null)
        {
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

            // Init commands
            this.graphicsCommands = new(CreateCommand);
        }

        unsafe ~GraphicsDevice()
        {
            // Shutdown graphics
            SDL3_ttf.TTF_DestroyGPUTextEngine(ttfTextEngine);
            SDL3.SDL_DestroyGPUDevice(gpuDevice);
        }

        // Methods
        public unsafe string GetDeviceDriverName()
        {
            return SDL3.SDL_GetGPUDeviceDriver(gpuDevice);
        }

        public unsafe GraphicsCommand Acquire()
        {
            // Try to get command buffer
            SDL_GPUCommandBuffer* gpuCommandBuffer = SDL3.SDL_AcquireGPUCommandBuffer(gpuDevice);

            if (gpuCommandBuffer == null)
            {
                Debug.LogError("Unable to acquire command buffer");
                return default;
            }

            // Get command for current thread
            GraphicsCommand command = graphicsCommands.Value;

            // Begin the command
            command.Begin(gpuCommandBuffer);
            return command;
        }

        private GraphicsCommand CreateCommand()
        {
            return new GraphicsCommand(Thread.CurrentThread, this);
        }

        internal async Task InitializeDefaultAssetsAsync(AssetProvider assets)
        {
            try
            {
                // Create white texture
                this.whiteTexture = new Texture(this, 1, 1);
                Color32 white = Color32.White;
                whiteTexture.Write(new Color32[,] { { white } });

                // Load error shader parts
                RawAsset vertexSource = await assets.LoadAsync<RawAsset>("DefaultAssets/Error.vert.spv");
                RawAsset fragmentSource = await assets.LoadAsync<RawAsset>("DefaultAssets/Error.frag.spv");

                // Create error shader
                Shader errorShader = new Shader(this, vertexSource.GetBytes(), fragmentSource.GetBytes(), ShaderFormat.Spirv);

                // Create error material
                errorMaterial = new Material
                {
                    Name = "Error Material",
                    Shader = errorShader,
                    MainTexture = whiteTexture,
                };

                // Upload the assets
                GraphicsCommand graphics = Acquire();
                graphics.BeginCopyPass();
                {
                    graphics.UploadTexture(whiteTexture);
                }
                graphics.EndCopyPass();
                graphics.Submit();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
