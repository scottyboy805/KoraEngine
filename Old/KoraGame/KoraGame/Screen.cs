using SDL;

namespace KoraGame
{
    public enum VSyncMode
    {
        Disabled = 0,
        Adaptive = -1,
        Sync = 1,
        SyncAlternate = 2,
    }

    public unsafe readonly struct DisplayMode
    {
        // Properties
        public readonly uint Width;
        public readonly uint Height;
        public readonly float RefreshRate;

        // Constructor
        public DisplayMode(uint width, uint height, float refreshRate = 0f)
        {
            this.Width = width;
            this.Height = height;
            this.RefreshRate = refreshRate;
        }

        internal DisplayMode(SDL_DisplayMode* sdlMode)
        {
            //this.sdlMode = sdlMode;
            this.Width = (uint)sdlMode->w;
            this.Height = (uint)sdlMode->h;
            this.RefreshRate = sdlMode->refresh_rate;
        }

        // Methods
        public override string ToString()
        {
            if(RefreshRate > 0)
                return $"{Width} X {Height} @ {RefreshRate}hz";

            return $"{Width} X {Height}";
        }
    }

    public unsafe sealed class Screen
    {
        // Internal
        internal readonly SDL_Window* sdlWindow;

        // Properties
        public int Width
        {
            get
            {
                // Get size
                int w, h;
                SDL3.SDL_GetWindowSize(sdlWindow, &w, &h);

                // Get width
                return w;
            }
        }

        public int Height
        {
            get
            {
                // Get size
                int w, h;
                SDL3.SDL_GetWindowSize(sdlWindow, &w, &h);

                // Get height
                return h;
            }
        }

        public bool Fullscreen => (SDL3.SDL_GetWindowFlags(sdlWindow) & SDL_WindowFlags.SDL_WINDOW_FULLSCREEN) != 0;

        public VSyncMode VSync
        {
            get
            {
                // Try to get mode
                int mode;
                SDL3.SDL_GetWindowSurfaceVSync(sdlWindow, &mode);
                
                // Get as enum
                return (VSyncMode)mode;
            }
            set
            {
                // Set mode
                SDL3.SDL_SetWindowSurfaceVSync(sdlWindow, (int)value);
            }
        }

        public string Title
        {
            get => SDL3.SDL_GetWindowTitle(sdlWindow);
            set
            {
                SDL3.SDL_SetWindowTitle(sdlWindow, value);
            }
        }

        // Constructor
        internal Screen(string title, uint width, uint height, bool fullScreen)
        {
            SDL_WindowFlags flags = 0;

            // Check for full screen
            if (fullScreen == true) flags |= SDL_WindowFlags.SDL_WINDOW_FULLSCREEN;

            // Create the window
            this.sdlWindow = SDL3.SDL_CreateWindow(title, (int)width, (int)height, flags);
        }

        ~Screen()
        {
            SDL3.SDL_DestroyWindow(sdlWindow);
        }

        // Methods
        public void Resize(int width, int height)
        {
            // Make windowed
            SDL3.SDL_SetWindowFullscreen(sdlWindow, false);
            SDL3.SDL_SetWindowSize(sdlWindow, width, height);

            Debug.Log($"Resize screen: '{width}, {height}'", LogFilter.Graphics);
        }

        public void Resize(DisplayMode mode)
        {
            // Get display mod
            SDL_DisplayMode sdlDisplayMode = default;
            SDL3.SDL_GetClosestFullscreenDisplayMode(0, (int)mode.Width, (int)mode.Height, mode.RefreshRate, false, &sdlDisplayMode);

            // Make full screen
            SDL3.SDL_SetWindowFullscreen(sdlWindow, true);
            SDL3.SDL_SetWindowFullscreenMode(sdlWindow, &sdlDisplayMode);

            Debug.Log($"Resize full screen: '{mode.Width}, {mode.Height}, {mode.RefreshRate}'", LogFilter.Graphics);
        }

        public static DisplayMode GetCurrentDisplayMode(int displayId = 0)
        {
            // Get the display mode
            SDL_DisplayMode* sdlMode = SDL3.SDL_GetCurrentDisplayMode((SDL_DisplayID)displayId);

            // Create the mode
            return new DisplayMode(sdlMode);
        }

        public static void GetDisplayModes(List<DisplayMode> outDisplayModes, uint displayId = 0)
        {
            // Get the modes
            int count = 0;
            SDL_DisplayMode** modes = SDL3.SDL_GetFullscreenDisplayModes((SDL_DisplayID)displayId, &count);

            // Add all items
            for (int i = 0; i < count; i++)
            {
                // Add the mode
                outDisplayModes.Add(new DisplayMode(modes[i]));
            }

            // Free the list
            SDL3.SDL_free(modes);
        }
    }
}
