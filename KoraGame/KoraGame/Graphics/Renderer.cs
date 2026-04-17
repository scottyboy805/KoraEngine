
namespace KoraGame.Graphics
{
    public abstract class Renderer : Component
    {
        // Properties
        public GraphicsDevice GraphicsDevice => Game?.GraphicsDevice;

        // Methods
        internal override void RegisterSubSystems()
        {
            Debug.Log("Register: " + gameObject.Name);
            Scene?.activeRenderers.Add(this);
        }

        internal override void UnregisterSubSystems()
        {
            Debug.Log("Unregister: " + gameObject.Name);
            Scene?.activeRenderers.Remove(this);
        }

        public abstract void Draw(GraphicsBatch graphics);
    }
}
