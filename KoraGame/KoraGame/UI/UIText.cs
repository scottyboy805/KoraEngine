using SDL;
using KoraGame.Graphics;
using System.Runtime.Serialization;

namespace KoraGame.UI
{
    public unsafe sealed class UIText : UIComponent
    {
        // Private
        [DataMember(Name = "Font")]
        private Font font;
        [DataMember(Name = "Text")]
        private string text;

        // Internal
        internal TTF_Text* ttfText;

        // Properties
        public Font Font
        {
            get => font;
            set
            {
                font = value;
                RebuildText();
            }
        }

        public string Text
        {
            get => text;
            set
            {
                text = value;
                RebuildText();
            }
        }

        // Constructor
        ~UIText()
        {
            OnDestroy();
        }

        // Methods
        protected override void OnDestroy()
        {
            // Release existing text
            if (ttfText != null)
            {
                SDL3_ttf.TTF_DestroyText(ttfText);
                ttfText = null;
            }
        }

        public override void Draw(GraphicsBatch graphics)
        {
            TTF_GPUAtlasDrawSequence* s;
            //SDL3_ttf.cre
            //SDL3_ttf.fontsi
        }

        private void RebuildText()
        {
            // Release existing text
            if(ttfText != null)
            {
                SDL3_ttf.TTF_DestroyText(ttfText);
                ttfText = null;
            }

            // Check for no font
            if (font == null)
                return;

            // Create the text
            ttfText = SDL3_ttf.TTF_CreateText(Graphics.ttfTextEngine, font.ttfFont, text, (UIntPtr)text.Length);
        }
    }
}
