using ImGuiNET;
using KoraGame;
using KoraGame.Graphics;
using SDL;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("KoraEditor-Windows")]

namespace KoraEditor
{
    public sealed class Editor : Game
    {
        // Private
        private AssetProvider editorAssets = null;
        private ImGuiContext gui = null;        

        private ulong lastFrameTime = 0;
        private ulong performanceFrequency = 0;

        // Properties
        public AssetProvider EditorAssets => editorAssets;
        internal ImGuiContext Gui => gui;

        // Properties
        public string EditorBasePath
        {
            get
            {
#if DEBUG
                return Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "../../../../"));
#else
                // Get path next to executable
                return Environment.CurrentDirectory;
#endif
            }
        }

        public string EditorContentPath
        {
            get
            {
                return Path.Combine(EditorBasePath, "Content");
            }
        }
        // Methods
        internal override void DoInitialize()
        {
            base.DoInitialize();


            // Create scripting
            Debug.Log("Initialize scripting", LogFilter.Script);
            this.scriptable = new ScriptableProvider();

            // Create the screen
            Debug.Log("Initialize graphics", LogFilter.Graphics);
            this.screen = new Screen("KoraEditor", 1280, 720, false);

            Debug.Log($"Use screen resolution: '{screen.Width} x {screen.Height}', FullScreen = '{screen.Fullscreen}'", LogFilter.Graphics);

            // Create graphics            
            this.graphics = new GraphicsDevice(this.screen);

            Debug.Log($"Use graphics API: '{graphics.GetDeviceDriverName()}'", LogFilter.Graphics);

            // Create assets
            Debug.Log($"Initialize assets", LogFilter.Assets);
            this.editorAssets = new AssetProvider(scriptable, graphics, EditorContentPath, false);

            Debug.Log($"Use assets directory: '{editorAssets.AssetDirectory}'", LogFilter.Assets);

            // Init gui
            gui = new ImGuiContext();
            gui.Initialize(graphics, editorAssets);
        }

        internal override void DoUpdate()
        {
            base.DoUpdate();

            // Render the editor
            DoRender();
        }

        internal void DoRender()
        {
            gui.BeginFrame();

            gui.EndFrame();
        }

        internal override void DoShutdown()
        {
            base.DoShutdown();
        }

        internal override void DoEvent(in SDL_Event evt)
        {
            base.DoEvent(evt);
        }
    }
}
