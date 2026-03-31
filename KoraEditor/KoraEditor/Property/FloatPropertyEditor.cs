using KoraEditor.UI;

namespace KoraEditor
{
    [PropertyEditorFor(typeof(float))]
    internal sealed class FloatPropertyEditor : PropertyEditor
    {
        // Private
        private float floatValue;

        // Methods
        protected override void OnCreate()
        {
            // Get initial value
            floatValue = Element.GetValue<float>();
        }

        protected override void OnValueGui()
        {
            if (Gui.InputNumber(ref floatValue) == true)
            {
                // Update value
                Element.SetValue(floatValue);

                // Mark modified
                SetModified();
            }
        }
    }
}
