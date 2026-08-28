using KoraEditor.UI;

namespace KoraEditor
{
    [PropertyEditorFor(typeof(int))]
    internal sealed class IntPropertyEditor : PropertyEditor
    {
        // Private
        private int intValue;

        // Methods
        protected override void OnCreate()
        {
            // Get initial value
            intValue = Property.GetValue<int>();
        }

        protected override void OnValueGui()
        {
            if(Gui.InputNumber(ref intValue) == true)
            {
                // Update value
                Property.SetValue(intValue);

                // Mark modified
                SetModified();
            }
        }
    }
}
