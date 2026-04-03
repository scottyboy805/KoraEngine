using ImGuiNET;
using KoraEditor.Element;
using KoraEditor.UI;
using KoraGame;

namespace KoraEditor
{
    [ElementEditorFor(typeof(GameObject))]
    internal sealed class GameObjectElementEditor : GameElementEditor
    {
        // Private
        private ElementEditor[] componentElementEditors = null;

        // Methods
        protected override void OnCreate()
        {
            // Create base ui
            base.OnCreate();

            // Find components
            EditorSerializedProperty componentsElement = Layout.FindProperty("Components", true);

            // Get all children
            componentElementEditors = componentsElement.ChildProperties
                .Select(e => e.CreateEditor())
                .ToArray();
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
                    // Draw foldout
                    bool expanded = Gui.BeginTreeNode(editor.Layout.DisplayName, GuiTreeOptions.Framed);

                    //Gui.BeginLayout(GuiLayout.Horizontal);
                    //{
                    //    Gui.Label("Testing");
                    //}
                    //Gui.EndLayout();

                    if (expanded == true)
                    {
                        Gui.Label("Componeont: " + editor.Layout.SerializeType);
                        editor.DrawEditorGui();

                        Gui.EndTreeNode();
                    }
                }
            }
        }
    }
}
