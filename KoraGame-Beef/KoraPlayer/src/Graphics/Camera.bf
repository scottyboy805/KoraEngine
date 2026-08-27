using internal KoraPlayer;

namespace KoraPlayer.Graphics;

public sealed class Camera : Component
{
	// Private
	private Color clearColor = Color.Gray;
	private float fieldOfView = 60f;
	private float nearPlane = 0.1f;
	private float farPlane = 1000f;

	private readonly GraphicsBatch graphics = new .(256) ~delete _;

	// Properties
	public Color ClearColor
	{
		get => clearColor;
		set
		{
			clearColor = value;
		}
	}

	public float FieldOfView
	{
		get => fieldOfView;
		set
		{
			fieldOfView = value;
		}
	}

	public float NearPlane
	{
		get => nearPlane;
		set
		{
			nearPlane = value;
		}
	}

	public float FarPlane
	{
		get => farPlane;
		set
		{
			farPlane = value;
		}
	}

	// Methods
	protected override void OnEnable()
	{
		// Register camera
		if(Scene != null)
			Scene.activeCameras.Add(this);
	}

	protected override void OnDisable()
	{
		// Unregister camera
		if(Scene != null)
			Scene.activeCameras.Remove(this);
	}

	public matrix4 GetProjectionMatrix(float aspect)
	{
	    // Get projection
	    return matrix4.Perspective(fieldOfView, aspect, nearPlane, farPlane);
	}

	public void Render(Texture renderTexture = null, matrix4? viewProjectionMatrix = null)
	{
		// Get command buffer
		GraphicsCommand cmd = game.Graphics.AcquireCommandBuffer();

		// Begin render pass
		cmd.BeginRenderPass(clearColor, renderTexture);
		{
			// Render the camera view
			Render(cmd, viewProjectionMatrix);
		}
		// End the pass
		cmd.EndRenderPass();

		// Submit the command buffer
		cmd.Submit();
	}

	public void Render(GraphicsCommand cmd, matrix4? viewMatrix = null, matrix4? projectionMatrix = null)
	{
		// Get the aspect
		float aspect = cmd.RenderWidth / (float)cmd.RenderHeight;

		// IMPORTANT - Use WorldToLocal as the inverse for camera
		matrix4 view = viewMatrix == null
		    ? GameObject != null ? GameObject.WorldToLocalMatrix : matrix4.Identity
		    : viewMatrix.Value;

		// Create projection matrix
		matrix4 projection = projectionMatrix == null
		    ? GetProjectionMatrix(aspect)
		    : projectionMatrix.Value;

		// Begin batch
		graphics.Begin(cmd, view, projection);
		{
			// Process all renderers
			Scene.Draw(graphics);
		}
		graphics.End();
	}
}