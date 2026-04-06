using KoraEditor.UI;

namespace KoraEditor
{
    [PropertyEditorFor(typeof(bool))]
    internal sealed class BoolPropertyEditor : PropertyEditor
    {
        // Private
        private bool boolValue;

        // Methods
        protected override void OnCreate()
        {
            // Try to get bool value
            boolValue = Property.GetValue<bool>();
        }

        protected override void OnValueGui()
        {
            if (Gui.Toggle(ref boolValue) == true)
            {
                // Set value
                Property.SetValue(boolValue);

                // Set modified
                SetModified();
            }
        }
    }
}
