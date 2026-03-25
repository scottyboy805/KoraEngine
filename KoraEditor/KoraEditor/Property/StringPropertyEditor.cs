using KoraEditor.UI;

namespace KoraEditor
{
    internal sealed class StringPropertyEditor : PropertyEditor
    {
        // Private
        private string stringValue;

        // Methods
        protected override void OnCreate()
        {
            // Try to get string
            stringValue = Element.GetValue<string>(out bool isMixed);

            // Check for mixed
            if (isMixed == true)
                stringValue = "---";
        }

        protected override void OnGui()
        {
            Gui.BeginLayout(GuiLayout.Horizontal);
            {
                Gui.Label(Element.ElementName);
                if (Gui.Input(ref stringValue) == true)
                {
                    // Set value
                    Element.SetValue(stringValue);

                    // Repaint
                    Repaint();
                }
            }
            Gui.EndLayout();
        }
    }
}
