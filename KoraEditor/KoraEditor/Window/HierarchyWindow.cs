using KoraEditor.UI;
using KoraGame;
using KoraGame.Graphics;

namespace KoraEditor
{
    internal sealed class HierarchyWindow : EditorWindow
    {
        // Private
        private Texture plusIcon;

        // Constructor
        public HierarchyWindow()
        {
            Title = "Hierarchy";
        }

        // Methods
        protected async override void OnOpen()
        {
            // Load icons
            plusIcon = await EditorAssets.LoadAsync<Texture>("Icon/Plus.png");
        }

        protected override void OnGui()
        {
            // Draw header
            OnHierarchyHeaderGui();

            // Check for scene
            if (Editor.IsSceneOpen == false)
                return;

            // Display the scene tree
            foreach(GameObject go in EditorScene.GameObjects)
                OnGameElementTreeGui(go);
        }

        private void OnHierarchyHeaderGui()
        {
            Gui.BeginLayout(GuiLayout.Horizontal);
            {
                // New scene button
                Gui.ImageButton(plusIcon, new Vector2F(32, 32), OnNewScene);
            }
            Gui.EndLayout();
        }

        private void OnGameElementTreeGui(GameObject obj)
        {
            GuiTreeOptions options = 0;

            // Check selected
            if (Selection.IsSelected(obj) == true)
                options |= GuiTreeOptions.Selected;

            // Check for leaf
            if (obj.HasChildren == false)
                options |= GuiTreeOptions.NoArrow;

            // Display the node
            if (Gui.BeginTreeNode(obj.Name, options, () =>
            {
                // Select the object
                Selection.Select(obj);
            }) == true)
            {
                // Display children
                foreach (var child in obj.Children)
                    OnGameElementTreeGui(child);

                // End the node
                Gui.EndTreeNode();
            }
        }

        private void OnNewScene()
        {
            Editor.NewScene();
        }

        [Menu("Window/Hierarhcy")]
        public static void Open()
        {
            Open<HierarchyWindow>();
        }
    }
}
