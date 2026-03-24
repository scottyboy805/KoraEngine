using SDL;

namespace KoraGame.Graphics
{
    public unsafe sealed class GraphicsDevice
    {
        // Private
        private readonly Screen defaultRenderTarget;
        private readonly Texture defaultDepthTarget;
        private readonly TextureFormat preferredFormat = TextureFormat.B8G8R8A8Unorm;

        private Texture whiteTexture = null;
        private Shader defaultShader = null;

        // Internal
        internal readonly SDL_GPUDevice* gpuDevice;
        internal readonly TTF_TextEngine* ttfTextEngine;

        // Properties
        internal Screen DefaultRenderTarget => defaultRenderTarget;
        internal Texture DefaultDepthTarget => defaultDepthTarget;
        public TextureFormat PreferredFormat => preferredFormat;

        public Texture WhiteTexture => whiteTexture;
        public Shader DefaultShader => defaultShader;

        // Constructor
        public GraphicsDevice(Screen defaultRenderTarget = null)
        {
            this.defaultRenderTarget = defaultRenderTarget;

            // Create the device
            this.gpuDevice = SDL3.SDL_CreateGPUDevice(SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_SPIRV | SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_MSL, true, (byte*)null);
            string err = SDL3.SDL_GetError();

            // Create the text engine
            this.ttfTextEngine = SDL3_ttf.TTF_CreateGPUTextEngine(gpuDevice);

            // Attach the device
            if(defaultRenderTarget != null)
            {
                // Claim the window
                SDL3.SDL_ClaimWindowForGPUDevice(gpuDevice, defaultRenderTarget.sdlWindow);

                // Get the preferred format
                this.preferredFormat = (TextureFormat)SDL3.SDL_GetGPUSwapchainTextureFormat(gpuDevice, defaultRenderTarget.sdlWindow);

                // Create depth texture
                this.defaultDepthTarget = new Texture(this, (uint)defaultRenderTarget.Width, (uint)defaultRenderTarget.Height, TextureFormat.D32Float, 1, TextureUsage.DepthStencilTarget);
            }

            // Create default assets
            InitializeDefaultAssets();
        }

        ~GraphicsDevice()
        {
            SDL3_ttf.TTF_DestroyGPUTextEngine(ttfTextEngine);
            SDL3.SDL_DestroyGPUDevice(gpuDevice);
        }

        // Methods
        public GraphicsCommand AcquireCommandBuffer()
        {
            // Try to get command buffer
            SDL_GPUCommandBuffer* gpuCommandBuffer = SDL3.SDL_AcquireGPUCommandBuffer(gpuDevice);

            // Create new
            return new GraphicsCommand(this, gpuCommandBuffer);
        }

        public string GetDeviceDriverName()
        {
            return SDL3.SDL_GetGPUDeviceDriver(gpuDevice);
        }

        private unsafe void InitializeDefaultAssets()
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
                GraphicsCommand cmd = this.AcquireCommandBuffer();
                cmd.BeginCopyPass();
                {
                    cmd.UploadTexture(whiteTexture);
                }
                cmd.EndCopyPass();
                cmd.Submit();
            }
            catch(Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
