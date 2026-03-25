
namespace KoraEditor.UI
{
    public struct GuiContent
    {
        // Public
        public string Text;
        public string Tooltip;
        public string Id;

        // Public
        public static readonly GuiContent Empty = new GuiContent(string.Empty);

        // Constructor
        public GuiContent(string text, string tooltip = null, string id = null)
        {
            this.Text = text;
            this.Tooltip = tooltip;
            this.Id = id;
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
