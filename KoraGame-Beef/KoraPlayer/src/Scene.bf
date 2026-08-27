using System;
using System.Collections;
using KoraPlayer.Graphics;
using internal KoraPlayer;

namespace KoraPlayer;

public sealed class Scene : GameElement
{
	// Private
	private bool active = false;

	// Internal
	internal readonly List<GameObject> gameObjects = new .();
	internal readonly List<Camera> activeCameras = new .() ~delete _;
	internal readonly List<Renderer> activeRenderers = new .() ~delete _;

	// Properties
	public bool Active => active;

	// Constructor
	public this() {}

	public this(StringView name)
		: base(name)
	{
	}

	internal ~this()
	{
		// Cleanup game objects
		for(let go in gameObjects)
			delete go;

		delete gameObjects;

		// Cleanup subsystem collections
		delete activeCameras;
		delete activeRenderers;
	}

	// Methods
	protected override void OnDestroy()
	{
		for(let go in gameObjects)
			GameElement.DestroyImmediate(go);
	}

	internal void Update()
	{

	}

	internal void Draw(GraphicsBatch graphics)
	{
		// Draw all renderers
		for(let renderer in activeRenderers)
		{
			renderer.Draw(graphics);
		}
	}

	internal void SetActive(bool on)
	{
		active = on;

		// Update all objects
		if(gameObjects != null)
		{
			for(let object in gameObjects)
				GameObject.DoGameObjectEnabledEvent(object, on);
		}
	}

	public static Scene CreateEmpty(StringView name = String.Empty)
	{
		// Create the scene
		Scene scene = new .(name);

		// Create a camera
		GameObject camObject = new .("Cam");
		camObject.AddComponent<Camera>();

		// Set scene
		camObject.Scene = scene;

		return scene;
	}
}