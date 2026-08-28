using KoraGame;

namespace KoraEditor.Element
{
    [ElementEditorFor(typeof(GameElement), true)]
    internal class GameElementEditor : ElementEditor
    {
        // Private
        private PropertyEditor[] propertyEditors = null;

        // Properties
        protected virtual IEnumerable<EditorSerializedProperty> DisplayProperties => Layout.VisibleProperties;

        // Methods
        protected override void OnCreate()
        {
            // Create drawers
            propertyEditors = GetPropertyEditors(DisplayProperties).ToArray();
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
