
using KoraGame;
using KoraGame.Graphics;
using KoraPipeline;

namespace KoraEditor
{
    public abstract class EditorContext
    {
        // Private
        private readonly Editor editor = null;

        // Properties
        public Editor Editor => editor;
        public Project Project => editor?.Project;
        public Selection Selection => editor?.Selection;
        public AssetDatabase AssetDatabase => editor?.AssetDatabase;
        public Scene EditorScene => editor?.EditorScene;
        public Screen EditorScreen => editor?.EditorScreen;
        public GraphicsDevice EditorGraphics => editor?.GraphicsDevice;
        public AssetProvider EditorAssets => editor?.EditorAssets;

        // Constructor
        protected EditorContext()
        {
            // Try to get editor
            editor = Editor.EditorInstance;
        }
    }
}
