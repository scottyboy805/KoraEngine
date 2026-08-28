using KoraGame.Graphics;
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

        // Methods
        protected virtual Game CreateGame()
        {
            // Create scripting
            Debug.Log("Initialize scripting", LogFilter.Script);
            ScriptableProvider scriptable = new ScriptableProvider();

            // Create the screen
            Debug.Log("Initialize graphics", LogFilter.Graphics);
            Screen screen = new Screen("Testing", 1280, 720, false);

            Debug.Log($"Use screen resolution: '{screen.Width} x {screen.Height}', FullScreen = '{screen.Fullscreen}'", LogFilter.Graphics);

            // Create graphics            
            GraphicsProvider graphics = new GraphicsProvider(screen);

            Debug.Log($"Use graphics API: '{graphics.GetDeviceDriverName()}'", LogFilter.Graphics);

            // Create assets
            Debug.Log($"Initialize assets", LogFilter.Assets);
            AssetProvider assets = new AssetProvider(scriptable, graphics, Environment.CurrentDirectory + "/Assets", false);

            Debug.Log($"Use assets directory: '{assets.AssetDirectory}'", LogFilter.Assets);

            // Create the game
            return new Game(null, screen, graphics, assets, scriptable, false, true);
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
