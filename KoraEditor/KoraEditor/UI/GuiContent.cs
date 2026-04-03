using KoraGame.Graphics;

namespace KoraEditor.UI
{
    public struct GuiContent
    {
        // Public
        public string Text;        
        public string Tooltip;
        public Texture Icon;
        public string Id;

        // Public
        public static readonly GuiContent Empty = new GuiContent(string.Empty);

        // Properties
        internal IntPtr IconPtr => Icon != null ? Icon.WeakPtr : IntPtr.Zero;

        // Constructor
        public GuiContent(string text, string tooltip = null, Texture icon = null, string id = null)
        {
            this.Text = text;
            this.Tooltip = tooltip;
            this.Icon = icon;
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

        public static implicit operator GuiContent(Texture icon)
        {
            return new GuiContent
            {
                Icon = icon,
            };
        }
    }
}
