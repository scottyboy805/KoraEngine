
namespace KoraGame.Graphics
{
    public abstract class Renderer : Component
    {
        // Properties
        public GraphicsDevice GraphicsDevice => Game?.GraphicsDevice;

        // Methods
        protected override void OnEnable()
        {
            Debug.Log("Register: " + gameObject.Name);
            Scene?.activeRenderers.Add(this);
        }

        protected override void OnDisable()
        {
            Debug.Log("Unregister: " + gameObject.Name);
            Scene?.activeRenderers.Remove(this);
        }

        public abstract void Draw(GraphicsCommand graphics);
    }
}
