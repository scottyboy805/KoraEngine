using KoraGame.Graphics;
using KoraGame.Physics;
using SDL;
using System.Runtime.InteropServices;

namespace KoraGame
{
    [Serializable]
    internal class GameHost
    {
        // Private
        private static GameHost instance;

        private GCHandle handle;
        private Game game = null;

        // Properties
        public static Game Game => instance?.game;

        public IntPtr Handle => (IntPtr)handle;
        public bool Quit => game.Quit;

        // Properties
        private string RuntimeAssetsDirectory
        {
            get
            {
#if DEBUG
                // Example project folder
                return Path.Combine(Path.GetFullPath("../../../../../"), "ExampleProject", "Assets");
#else
                // Assets folder next to executable
                return Path.Combine(Environment.CurrentDirectory, "Assets");
#endif
            }
        }

        // Methods
        protected virtual Game CreateGame()
        {
            // Init SDL
            Debug.Log("Initialize SDL", LogFilter.Game);
            if (SDL3.SDL_Init(SDL_InitFlags.SDL_INIT_VIDEO | SDL_InitFlags.SDL_INIT_GAMEPAD) == false)
            {
                Debug.LogError("Failed to initialize SDL", LogFilter.Game);
                Environment.Exit(1);
            }

            // Init SDL audio
            if (SDL3_mixer.MIX_Init() == false)
            {
                Debug.LogError("Failed to initialize SDL mixer", LogFilter.Audio);
            }

            // Init SDL font
            if (SDL3_ttf.TTF_Init() == false)
            {
                Debug.LogError("Failed to initialize SDL ttf", LogFilter.Graphics);
            }


            // Create scripting
            Debug.Log("Initialize scripting", LogFilter.Script);
            ScriptableProvider scriptable = new ScriptableProvider();

            // Create the screen
            Debug.Log("Initialize graphics", LogFilter.Graphics);
            Screen screen = new Screen("Testing", 1280, 720, false);

            Debug.Log($"Use screen resolution: '{screen.Width} x {screen.Height}', FullScreen = '{screen.Fullscreen}'", LogFilter.Graphics);

            // Create graphics            
            GraphicsDevice graphics = new GraphicsDevice(screen);

            Debug.Log($"Use graphics API: '{graphics.GetDeviceDriverName()}'", LogFilter.Graphics);

            // Create assets
            Debug.Log($"Initialize assets", LogFilter.Assets);
            AssetProvider assets = new AssetProvider(scriptable, graphics, RuntimeAssetsDirectory, false);

            Debug.Log($"Use assets directory: '{assets.AssetDirectory}'", LogFilter.Assets);

            // Create physics
            Debug.Log("Initialize physics", LogFilter.Physics);
            PhysicsWorld physics = new PhysicsWorld();
            physics.Initialize();

            // Load default assets
            _ = graphics.InitializeDefaultAssetsAsync(assets);

            // Create the game
            return new Game(null, screen, graphics, assets, scriptable, physics, false, true);
        }

        internal void DoInitialize()
        {
            // Get instance
            instance = this;

            // Pin host memory
            this.handle = GCHandle.Alloc(this, GCHandleType.Normal);

            // Create the game
            this.game = CreateGame();

            // Initialize the game
            game.Initialize();
        }

        internal void DoUpdate()
        {
            // Update the game
            game.Update();
        }

        internal void DoEvent(in SDL_Event evt)
        {
            // Handle event
            game.HandleEvent(evt);
        }

        internal void DoShutdown()
        {
            // Quit the game
            game.Shutdown();

            // Free instance
            if(instance == this)
                instance = null;

            // Free handle
            handle.Free();
            handle = default;

            // Quit ttf
            SDL3_ttf.TTF_Quit();

            // Quit mixer
            SDL3_mixer.MIX_Quit();

            // Quit SDL
            SDL3.SDL_Quit();
        }

        internal static GameHost Get(IntPtr handle)
        {
            // Get the gc handle
            GCHandle gcHandle = (GCHandle)handle;

            // Get the target as host
            return (GameHost)gcHandle.Target;
        }
    }
}
