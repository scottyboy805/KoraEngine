
namespace KoraEditor.UI
{
    public struct GuiContent
    {
        // Private
        private string text;
        private string tooltip;

        // Properties
        public string Text
        {
            get => text;
            set => text = value != null ? value : string.Empty;
        }

        public string Tooltip
        {
            get => tooltip;
            set => tooltip = value;
        }

        // Constructor
        public GuiContent(string text, string tooltip = "")
        {
            this.Text = text;
            this.tooltip = tooltip;
        }

        // Methods
        public static implicit operator GuiContent(string text)
        {
            return new GuiContent
            {
                Text = text,
            };
        }
    }
}
