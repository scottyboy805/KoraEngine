using KoraEditor.UI;
using KoraGame;
using KoraGame.Graphics;
using System.Runtime.Serialization;

namespace KoraEditor
{
    internal sealed class GameWindow : EditorWindow
    {
        // Type
        private struct GameDisplayMode
        {
            // Public
            public string Label;
            public DisplayMode? DisplayMode;
            public float Aspect;

            // Constructor
            public GameDisplayMode(uint width, uint height, string label = null)
            {
                this.DisplayMode = new DisplayMode(width, height);
                this.Aspect = (float)width / (float)height;
                this.Label = label != null ? label : DisplayMode.ToString();
            }

            public GameDisplayMode(float aspect, string label = null)
            {
                this.DisplayMode = null;
                this.Aspect = aspect;
                this.Label = label != null ? label : DisplayMode.ToString();
            }

            // Methods
            public Vector2F GetRenderSize(Vector2F availableSize)
            {
                float availableWidth = availableSize.X;
                float availableHeight = availableSize.Y;

                // Try fitting to width first
                float width = availableWidth;
                float height = width / Aspect;

                // If it overflows vertically, fit to height instead
                if (height > availableHeight)
                {
                    height = availableHeight;
                    width = height * Aspect;
                }

                return new Vector2F(width, height);
            }

            public Vector2F GetActualRenderSize()
            {
                // Check for display mode
                if(DisplayMode != null)
                    return new Vector2F(DisplayMode.Value.Width, DisplayMode.Value.Height);

                // Get aspect using 1000px as base width
                return new Vector2F(1000f, 1000f / Aspect);
            }

            public override string ToString()
            {
                if (DisplayMode != null)
                    return DisplayMode.Value.ToString() + " (" + Label + ")";

                return Label;
            }
        }

        // Private
        [DataMember]
        private GameDisplayMode[] gameResolutions =
        {
            new(1f, "1:1"),
            new (3f / 2f, "3:2"),
            new (4f / 3f, "4:3"),
            new (16f / 9f, "16:9"),
            new (16f / 10f, "16:10"),
            new (1024, 768),
            new (1280, 720, "720P"),
            new (1920, 1080, "HD"),
            new (3840, 2160, "4K")
        };

        private int gameResolutionSelected = 3;         // 16:9
        private string[] gameResolutionNames = null;
        private Texture renderTexture;

        // Properties
        private GameDisplayMode RenderMode => gameResolutions[gameResolutionSelected];

        // Methods
        protected override void OnOpen()
        {
            // Create resoltions
            gameResolutionNames = gameResolutions.Select(r => r.ToString()).ToArray();

            // Create the texture
            CreateRenderTexture();
        }

        protected override void OnResize()
        {
            // Recreate the texture when the window size changed
            CreateRenderTexture();
        }

        protected override void OnGui()
        {
            Gui.DrawRectangle(Position + new Vector2F(0, 40), Size - new Vector2F(0, 40), Color.Black);

            // Display the toolbar
            OnGameToolbarGui();

            // Render the scene
            if (Editor.IsSceneOpen == false)
                return;            

            // Render the scene
            Camera cam = Editor.EditorScene.activeCameras.FirstOrDefault();

            if (cam != null)
            {
                // Get render size scaled without stretching
                Vector2F renderSize = RenderMode.GetRenderSize(Gui.AvailableSize);

                // Render the scene
                cam.Render(renderTexture);

                Vector2F renderOffset = default;
                renderOffset.X = (Size.X - renderSize.X) / 2f;
                renderOffset.Y = (Size.Y - 48 - renderSize.Y) / 2f;

                // Offset position
                Gui.Position += renderOffset;

                // Display the texture
                Gui.Image(renderTexture, renderSize);
            }
            else
            {
                Gui.Label("No Rendering Camera!");
            }
        }

        private void OnGameToolbarGui()
        {
            Gui.BeginLayout(GuiLayoutOptions.Horizontal);
            {
                // Display resolution prefiew
                Gui.Popup(ref gameResolutionSelected, gameResolutionNames);
            }
            Gui.EndLayout();
        }

        private void CreateRenderTexture()
        {
            // Destroy old texture
            if(renderTexture != null)
            {
                GameElement.DestroyImmediate(renderTexture);
                renderTexture = null;
            }

            // Get the render mode
            Vector2F renderSize = RenderMode.GetActualRenderSize();

            // Create new if we have a valid size
            if (renderSize.X > 0 && renderSize.Y > 0)
                renderTexture = new Texture(EditorGraphics, (uint)renderSize.X, (uint)renderSize.Y, usage: TextureUsage.Sampler | TextureUsage.ColorTarget);
        }

        [Menu("Window/Game")]
        public static void Open()
        {
            Open<GameWindow>();
        }
    }
}
