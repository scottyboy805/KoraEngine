using KoraEditor.UI;

namespace KoraEditor
{
    [PropertyEditorFor(typeof(string))]
    internal sealed class StringPropertyEditor : PropertyEditor
    {
        // Private
        private string stringValue;

        // Methods
        protected override void OnCreate()
        {
            // Try to get string
            stringValue = Property.GetValue<string>(out bool isMixed);

            // Check for mixed
            if (isMixed == true)
                stringValue = "---";
        }

        protected override void OnValueGui()
        {
            if (Gui.Input(ref stringValue) == true)
            {
                // Set value
                Property.SetValue(stringValue);

                // Set modified
                SetModified();
            }
        }
    }
}
