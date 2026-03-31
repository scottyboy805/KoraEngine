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
            intValue = Element.GetValue<int>();
        }

        protected override void OnValueGui()
        {
            if(Gui.InputNumber(ref intValue) == true)
            {
                // Update value
                Element.SetValue(intValue);

                // Mark modified
                SetModified();
            }
        }
    }
}
