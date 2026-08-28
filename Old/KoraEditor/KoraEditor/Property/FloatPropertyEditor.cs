using KoraEditor.UI;

namespace KoraEditor
{
    [PropertyEditorFor(typeof(float))]
    internal sealed class FloatPropertyEditor : PropertyEditor
    {
        // Private
        private float floatValue;
        private bool useSlider;

        // Methods
        protected override void OnCreate()
        {
            // Get initial value
            floatValue = Property.GetValue<float>();

            // Check for slider
            useSlider = Property.RangeValue != null;
        }

        protected override void OnValueGui()
        {
            if (useSlider == true)
            {
                if (Gui.Slider(ref floatValue, Property.RangeValue.Value.Item1, Property.RangeValue.Value.Item2) == true)
                {
                    // Update value
                    Property.SetValue(floatValue);

                    // Mark modified
                    SetModified();
                }
            }
            else
            {
                if (Gui.InputNumber(ref floatValue) == true)
                {
                    // Validate min
                    if (Property.MinValue != null && floatValue < Property.MinValue.Value)
                        floatValue = Property.MinValue.Value;

                    // Validate max
                    if (Property.MaxValue != null && floatValue > Property.MaxValue.Value)
                        floatValue = Property.MaxValue.Value;

                    // Update value
                    Property.SetValue(floatValue);

                    // Mark modified
                    SetModified();
                }
            }
        }
    }
}
