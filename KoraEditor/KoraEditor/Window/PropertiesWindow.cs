using KoraEditor.UI;

namespace KoraEditor
{
    internal sealed class PropertiesWindow : EditorWindow
    {
        // Private
        private EditorSerializedLayout displayedLayout = null;
        private ElementEditor displayedEditor = null;

        // Constructor
        public PropertiesWindow()
        {
            Title = "Properties";
        }

        // Methods
        protected override void OnOpen()
        {
            RebuildSelection();

            // Add listener
            Selection.OnSelectionChanged += RebuildSelection;
        }

        protected override void OnClose()
        {
            // Remove listener
            Selection.OnSelectionChanged -= RebuildSelection;
        }

        protected override void OnGui()
        {
            if (Selection.HasAnySelection == true)
            {
                Gui.Label(Selection.GetSelectedElement().ToString());

                // Display the editor
                if (displayedEditor != null && displayedLayout != null)
                {
                    // Display the drawer
                    bool modified = displayedEditor.DrawEditorGui(displayedLayout);

                    // Check for modified
                    if (modified == true)
                        Repaint();
                }
            }
            else
                Gui.Label("No Selection");
        }

        private void RebuildSelection()
        {
            // Clear drawer
            displayedEditor = null;
            displayedLayout = null;

            // Check for any
            if (Selection.HasAnySelection == false)
                return;

            // Create editor from selection
            Type mainType = Selection.SelectedType;

            // Try to get drawer
            ElementEditor editor = ElementEditor.ForType(mainType);

            // Check for any
            if(editor != null)
            {
                this.displayedEditor = editor;
            }

            // Create serialized layout from selection
            this.displayedLayout = new EditorSerializedLayout(mainType, Selection.GetSelected().ToArray());
        }

        [Menu("Window/Properties")]
        public static void Open()
        {
            Open<PropertiesWindow>();
        }
    }
}
