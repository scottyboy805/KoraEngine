
namespace KoraGame.Graphics
{
    public abstract class Renderer : Component
    {
        // Properties
        public GraphicsDevice Graphics => Game?.Graphics;

        // Methods
        internal override void RegisterSubSystems()
        {
            Scene?.activeRenderers.Add(this);
        }

        internal override void UnregisterSubSystems()
        {
            Scene?.activeRenderers.Remove(this);
        }

        public abstract void Draw(GraphicsBatch renderBatch);
    }
}
