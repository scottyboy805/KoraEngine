using KoraGame.Graphics;
using System.Runtime.Serialization;

namespace KoraGame.UI
{
    public class UIImage : UIComponent
    {
        // Private
        [DataMember(Name = "Color")]
        private Color color;
        [DataMember(Name = "Texture")]
        private Texture texture;

        private Mesh mesh;

        // Properties
        public Color Color
        {
            get => color;
            set
            {
                color = value;
            }
        }

        public Texture Texture
        {
            get => texture;
            set
            {
                texture = value;
            }
        }

        protected override void OnEnable()
        {
            mesh = Mesh.PrimitiveQuad(Graphics, Vector2F.One);
        }

        public override void Draw(GraphicsBatch graphics)
        {
            
        }
    }
}
