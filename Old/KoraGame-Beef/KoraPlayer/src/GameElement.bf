using System;
using internal KoraPlayer;

namespace KoraPlayer;

public abstract class GameElement : ScriptBinding
{
	// Type
	private enum GameElementFlags
	{
		Destroying = 1 << 1,
		Destroyed = 1 << 2,
		Instance = 1 << 3,
	}

	// Internal
	internal Game game;

	// Private
	private String name = new .() ~delete _;
	private GameElementFlags flags = 0;

	// Properties
	public StringView Name => name;
	public bool IsDestroying => (flags & .Destroying) != 0;
	public bool IsDestroyed => (flags & .Destroyed) != 0;
	public bool IsAsset => (flags & .Instance) == 0;

	// Constructor
	protected this()
	{
		game = Game.Instance;
	}

	protected this(StringView name)
		: this()
	{
		// Update name
		this.name.Set(name);
	}

	// Methods
	protected virtual GameElement OnInstantiate()
	{
		return null;
	}

	protected virtual void OnCreate() {}
	protected virtual void OnDestroy() {}

	public static void Destroy(GameElement element)
	{
		// Check for null
		if(element == null || element.IsDestroyed == true)
			return;

		// Check for asset
		if(element.IsAsset == true)
		{
			Debug.LogError("Destroying assets is not permitted. Use `Assets.Unload` if you really want to destroy the asset");
			return;
		}

		// Set destroying
		element.flags |= .Destroying | .Destroyed;

		// Add to destroy queue
		element.game.DestroyDelayed(element);
	}

	public static void DestroyImmediate(GameElement element)
	{
		// Check for null
		if(element == null || element.IsDestroyed == true)
			return;

		// Check for asset
		if(element.IsAsset == true)
		{
			Debug.LogError("Destroying assets is not permitted. Use `Assets.Unload` if you really want to destroy the asset");
			return;
		}

		// Set destroying
		element.flags &= ~.Destroying;
		element.flags |= .Destroyed;

		// Trigger destroy
		element.OnDestroy();
	}
}