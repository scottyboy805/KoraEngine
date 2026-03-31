using KoraEditor.UI;

namespace KoraEditor
{
    internal sealed class EnumPropertyEditor : PropertyEditor
    {
        // Private
        private Enum value;
        private bool isMixed;

        // Methods
        protected override void OnCreate()
        {
            // Get the value
            value = Element.GetValue<Enum>(out isMixed);
        }

        protected override void OnGui()
        {
            Gui.BeginLayout(GuiLayout.Horizontal);
            {
                Gui.Label(Element.ElementName);
                if (Gui.EnumPopup(ref value) == true)
                {
                    // Set value
                    Element.SetValue(value);

                    // Set modified
                    SetModified();
                }
            }
            Gui.EndLayout();
        }
    }
}
