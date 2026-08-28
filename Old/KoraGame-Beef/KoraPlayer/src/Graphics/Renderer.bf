using internal KoraPlayer;

namespace KoraPlayer.Graphics;

public abstract class Renderer : Component
{
	// Properties
	public GraphicsDevice Graphics => game.Graphics;

	// Methods
	protected override void OnEnable()
	{
		// Add to renderers
		if(Scene != null)
			Scene.activeRenderers.Add(this);
	}

	protected override void OnDisable()
	{
		// Remove from renderers
		if(Scene != null)
			Scene.activeRenderers.Remove(this);
	}

	public abstract void Draw(GraphicsBatch graphics);
}