using System.Runtime.Serialization;

namespace KoraGame.Graphics
{
    public sealed class Sprite : GameElement
    {
        // Private
        [DataMember(Name = "Texture")]
        private Texture texture;
        [DataMember(Name = "Pivot")]
        private Vector2F pivot;
        [DataMember(Name = "SourcePosition")]
        private Vector2F sourcePosition;
        [DataMember(Name = "SourceSize")]
        private Vector2F sourceSize;

        // Properties
        public Texture Texture
        {
            get => texture;
            set
            {
                texture = value;
            }
        }

        public Vector2F Pivot
        {
            get => pivot;
            set
            {
                pivot = value;
            }
        }

        public Vector2F SourcePosition
        {
            get => sourcePosition;
            set
            {
                sourcePosition = value;
            }
        }

        public Vector2F SourceSize
        {
            get => sourceSize;
            set
            {
                sourceSize = value;
            }
        }
    }
}
