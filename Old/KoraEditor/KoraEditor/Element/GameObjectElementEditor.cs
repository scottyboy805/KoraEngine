using ImGuiNET;
using KoraEditor.Element;
using KoraEditor.UI;
using KoraGame;
using KoraGame.Graphics;

namespace KoraEditor
{
    [ElementEditorFor(typeof(GameObject))]
    internal sealed class GameObjectElementEditor : GameElementEditor
    {
        // Private
        private ElementEditor[] componentElementEditors = null;
        private Texture deleteTexture;

        // Methods
        protected async override void OnCreate()
        {
            // Create base ui
            base.OnCreate();

            // Find components
            EditorSerializedProperty componentsElement = Layout.FindProperty("Components", true);

            // Get all children
            componentElementEditors = componentsElement.ChildProperties
                .Select(e => e.CreateEditor())
                .ToArray();

            // Load ions
            deleteTexture = await Editor.LoadEditorIconAsync("Icon/Close.png");
        }

        protected override void OnGui()
        {
            // Display base ui
            base.OnGui();

            // Display component editors
            if(componentElementEditors != null)
            {
                foreach(ElementEditor editor in componentElementEditors)
                {
                    // Create content
                    GuiContent content = new GuiContent(
                        editor.Layout.DisplayName,
                        editor.Layout.SerializeType.FullName,
                        editor.Icon);

                    // Draw foldout
                    bool expanded = Gui.BeginTreeNode(content, GuiTreeOptions.Framed);

                    // Component header
                    Gui.BeginLayout(GuiLayoutOptions.Horizontal | GuiLayoutOptions.Continue | GuiLayoutOptions.Empty);
                    {
                        //Gui.Label("Testing");

                        // Push to end
                        //Gui.Position += new Vector2F(500, 0f);

                        Gui.ImageButton(deleteTexture, new Vector2F(24, 24));
                    }
                    Gui.EndLayout();

                    if (expanded == true)
                    {
                        // Draw the gui
                        editor.DrawEditorGui();

                        Gui.EndTreeNode();
                    }
                }
            }
        }
    }
}
