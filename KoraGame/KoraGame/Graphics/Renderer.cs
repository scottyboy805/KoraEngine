
namespace KoraGame.Graphics
{
    public abstract class Renderer : Component
    {
        // Properties
        public GraphicsProvider Graphics => Game?.Graphics;

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

        public abstract void Draw(GraphicsBatch graphics);
    }
}
