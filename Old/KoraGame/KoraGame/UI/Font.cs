using SDL;
using System.Drawing;

namespace KoraGame.UI
{
    public unsafe sealed class Font : GameElement
    {
        // Internal
        internal TTF_Font* ttfFont;

        // Properties
        public bool IsFixedWidth => SDL3_ttf.TTF_FontIsFixedWidth(ttfFont);
        public bool IsScalable => SDL3_ttf.TTF_FontIsScalable(ttfFont);

        // Methods
        protected override void OnDestroy()
        {
            if(ttfFont != null)
            {
                SDL3_ttf.TTF_CloseFont(ttfFont);
                ttfFont = null;
            }
        }

        public bool HasCharacter(char c)
        {
            return SDL3_ttf.TTF_FontHasGlyph(ttfFont, c);
        }

        public float MeasureString(string text, float maxWidth = 0)
        {
            // Check for none
            if (string.IsNullOrEmpty(text) == true)
                return 0f;

            // Try to measure
            int width;
            SDL3_ttf.TTF_MeasureString(ttfFont, text, (UIntPtr)text.Length, (int)maxWidth, &width, null);

            // Get the width
            return width;
        }

        public unsafe static Font LoadTTF(string path, float pointSize)
        {
            // Create the font
            Font font = new();

            // Open the font
            font.ttfFont = SDL3_ttf.TTF_OpenFont(path, pointSize);

            // Use file name
            font.Name = Path.GetFileNameWithoutExtension(path);

            return font;
        }
    }
}
