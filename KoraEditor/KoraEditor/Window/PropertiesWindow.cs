using KoraEditor.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace KoraEditor
{
    internal sealed class PropertiesWindow : EditorWindow
    {
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
                Gui.Label(Selection.GetSelectedElement().ToString());
            else
                Gui.Label("No Selection");
        }

        private void RebuildSelection()
        {

        }

        [Menu("Window/Properties")]
        public static void Open()
        {
            Open<PropertiesWindow>();
        }
    }
}
