using KoraEditor.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace KoraEditor
{
    public class ConsoleWindow : EditorWindow
    {
        // Methods
        protected override void OnGui()
        {
            base.OnGui();
            Gui.Label(new GuiContent("Testing label", "This should be a tooltip"));
        }

        [Menu("Window/Console", "Ctrl+Shift+C")]
        public static void Open()
        {
            Open<ConsoleWindow>();
        }
    }
}
