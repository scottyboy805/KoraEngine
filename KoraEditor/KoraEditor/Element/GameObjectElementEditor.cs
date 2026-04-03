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
            EditorSerializedElement componentsElement = Layout.FindElement("Components", true);

            // Get all children
            componentElementEditors = componentsElement.ChildElements
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
                    bool expanded = Gui.BeginTreeNode(editor.Layout.SerializeType.ToString(), GuiTreeOptions.Framed);

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
