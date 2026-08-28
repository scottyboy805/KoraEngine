using System;
using System.Collections;
using System.Interop;
using System.IO;
using KoraPlayer.Assets;
using KoraPlayer.Graphics;
using KoraPlayer.Physics;
using KoraPlayer.Scripting;
using internal KoraPlayer;
using internal KoraPlayer.Graphics;
using internal KoraPlayer.Scripting;
using internal KoraPlayer.Screen;

namespace KoraPlayer;

static
{
	[Export, CLink]
	public static void* CreateGame()
	{
		// Create game instance
		Game game = new Game();

		// Initialize the game
		game.DoInitialize();

		// Get pointer
		return Internal.UnsafeCastToPtr(game);
	}

	[Export, CLink]
	public static c_int GameEvent(void* appState, SDL3.SDL_Event* evt)
	{
		// Get the game
		Game game = (Game)Internal.UnsafeCastToObject(appState);

		// Do event
		game.DoEvent(evt);

		return 1;
	}

	[Export, CLink]
	public static c_int GameIterate(void* appState)
	{
		// Get the game
		Game game = (Game)Internal.UnsafeCastToObject(appState);

		// Update game
		game.DoUpdate();

		// Check for quit
		if(game.Quit == true)
			return 0;

		return 1;
	}

	[Export, CLink]
	public static void GameQuit(void* appState)
	{
		// Get the game
		Game game = (Game)Internal.UnsafeCastToObject(appState);

		// Shutdown game
		game.DoShutdown();

		// Cleanup memory
		delete game;
	}
}

internal class Game
{
	// Private
	private static Game instance;
	private readonly Queue<GameElement> destroyElements = new .();

	private bool quit = false;
	private Screen screen = null;
	private GraphicsDevice graphics;
	private AssetProvider assets = null;
	private World world;
	private Scene scene;

	private uint64 lastFrameTime = 0;
	private uint64 performanceFrequency = 0;

	// Properties
	internal static Game Instance => instance;

	public bool Quit => quit;
	public Screen Screen => screen;
	public GraphicsDevice Graphics => graphics;
	public World World => world;

	// Constructor
	internal this()
	{
		instance = this;
	}

	internal ~this()
	{
		// Delete elements
		for(let element in destroyElements)
			delete element;

		delete destroyElements;

		if(instance == this)
			instance = null;
	}

	// Methods
	internal void DoInitialize()
	{
		// Init sdl
		if(SDL3.SDL_Init(.SDL_INIT_VIDEO | .SDL_INIT_GAMEPAD) == false)
		{
			return;
		}

		// Init screen
		Debug.Log("Initialize graphics", LogFilter.Graphics);
		screen = new Screen("Testing", 1920, 1080, false);

		// Create graphics
		Debug.Log(scope $"Use screen resolution: '{screen.Width} x {screen.Height}', FullScreen = '{screen.Fullscreen}'", LogFilter.Graphics);
		graphics = new GraphicsDevice(screen);


		// Init assets
		String exePath = scope .();
		String exeFolder = scope .();
		Environment.GetExecutableFilePath(exePath);
		Path.GetDirectoryPath(exePath, exeFolder);

		String assetDirectory = scope .();
		Path.Combine(assetDirectory, exeFolder, "Assets");
		assets = new AssetProvider(graphics, assetDirectory);


		ScriptHost host = scope .();
		host.LoadRuntime("runtimeconfig.json");

		System.Collections.List<uint8> bytes = scope .();
		String path = "D:\\VisualStudio\\KoraEngine\\KoraGame-Beef\\build\\Debug_Win64\\KoraPlayer\\ScriptEngine.dll";

		bool exists = System.IO.File.Exists(path);
		System.IO.File.ReadAll(path, bytes);

		//host.LoadAssembly("D:\\VisualStudio\\KoraEngine\\KoraGame-Beef\build\\Debug_Win64\\KoraPlayer\\ScriptEngine.dll");

		host.LoadAssembly("D:\\VisualStudio\\KoraEngine\\KoraGame-Beef\\build\\Debug_Win64\\KoraPlayer\\ScriptEngine.dll");

		void* call = null;
		host.GetFunctionPointer("KoraGame.Scripting.ScriptableBehaviour, KoraGame", "ScriptableBehaviour_CreateManaged", &call);
		Console.WriteLine(call);



		// Create physics
		Debug.Log("Initialize physics", LogFilter.Physics);
		world = new .();

		RigidBody body = scope .();
		Component.DoComponentEnabledEvent(body, true);


		// Initialize timing
		performanceFrequency = SDL3.SDL_GetPerformanceFrequency();
		lastFrameTime = SDL3.SDL_GetPerformanceCounter();


		
		RawAsset vert = assets.LoadAsset<RawAsset>("pbr.vert.spv");
		RawAsset frag = assets.LoadAsset<RawAsset>("pbr.frag.spv");

		ShaderSource vertSource = new .(vert, "main", 2, 0);
		ShaderSource fragSource = new .(frag, "main", 0, 5);

		// Create shader
		Shader shader = new .(graphics, vertSource, fragSource, .Spirv);
		shader.Properties.Add(new .("Texture", 0, .Fragment, .Texture));
		shader.Properties.Add(new .("Normal", 1, .Fragment, .Texture));
		shader.Properties.Add(new .("Metallic", 2, .Fragment, .Texture));
		shader.Properties.Add(new .("Roughness", 3, .Fragment, .Texture));
		shader.Properties.Add(new .("Occlusion", 4, .Fragment, .Texture));

		// Material
		Material mat = new .();
		mat.Shader = shader;

		Texture white = new .(graphics, 1, 1, .R8G8B8A8_UNORM);
		int32 col = int32.MaxValue;
		white.Write(Span<int32>(&col, 1));

		mat.texture = white;


		Scene scene = Scene.CreateEmpty("My new scene");
		GameObject go = GameObject.CreatePrimitiveCube(graphics);
		go.GetComponent<MeshRenderer>().SetMaterial(mat, 0);
		go.LocalPosition = float3(0, 0, -10);
		go.Scene = scene;
		ChangeScene(scene);
	}

	internal void DoUpdate()
	{
		// Calculate frame time
		uint64 currentTime = SDL3.SDL_GetPerformanceCounter();
		float deltaTime = (float)(currentTime - lastFrameTime) / performanceFrequency;
		lastFrameTime = currentTime;

		// Update time system
		Time.UpdateTime(deltaTime);

		// Update and render scene
		if(scene != null)
		{
			// Do scene update
			scene.Update();

			// Render all active cameras in the scene
			for(let camera in scene.activeCameras)
				camera.Render();
		}


		// Destroy elements at the end of the frame
		while (destroyElements.Count > 0)
			GameElement.DestroyImmediate(destroyElements.PopFront());
	}

	internal void DoEvent(SDL3.SDL_Event* evt)
	{
		switch((SDL3.SDL_EventType)evt.type)
		{
		case .SDL_EVENT_QUIT: quit = true;

			// Do nothing
		default:
		}
	}

	internal void DoShutdown()
	{
		// Destroy screen
		delete screen;

		// Quit sdl
		SDL3.SDL_Quit();
	}

	internal void DestroyDelayed(GameElement element)
	{
		// Will be destroyed at the end of the frame
		destroyElements.Add(element);
	}

	public void ChangeScene(Scene scene)
	{
		// Deactivate current scene
		if(this.scene != null)
			this.scene.SetActive(false);

		// Update scene
		this.scene = scene;

		// Active new scene
		if(scene != null)
			scene.SetActive(true);

		Debug.Log(scope $"Change current scene: '{(scene != null ? scene.Name : null)}'", LogFilter.Game);
	}
}