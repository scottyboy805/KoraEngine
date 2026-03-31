using KoraGame;

namespace KoraEditor.Element
{
    [ElementEditorFor(typeof(GameElement), true)]
    internal sealed class GameElementEditor : ElementEditor
    {
        // Private
        private PropertyEditor[] propertyEditors = null;

        // Methods
        protected override void OnCreate()
        {
            // Create drawers
            propertyEditors = GetPropertyEditors(true).ToArray();
        }

        protected override void OnGui()
        {
            // Display all
            foreach (PropertyEditor editor in propertyEditors)
            {
                // Display gui
                bool modified = editor.DrawEditorGui();

                // Check for modified
                if (modified == true)
                    ;// Layout.
            }
        }
    }
}
