using System;
using System.Interop;

namespace KoraPlayer_Desktop;

static
{
	typealias Game = void*;

	// Exports
	[CLink]
	public static extern Game CreateGame();

	[CLink]
	public static extern c_int GameEvent(Game game, SDL3.SDL_Event* evt);

	[CLink]
	public static extern c_int GameIterate(Game game);

	[CLink]
	public static extern void GameQuit(Game game);
}

internal class Program
{
	static void Main(String[] args)
	{
		// Create arguments
		char8*[] argv = scope char8*[args.Count];

		// Initialize arguments
		for(int32 i = 0; i < args.Count; i++)
			argv[i] = args[i].CStr();

		// Enter callbacks
		int32 result = SDL3.SDL_EnterAppMainCallbacks(
			(int32)args.Count, argv,
			=> AppInit,
			=> AppIterate,
			=> AppEvent,
			=> AppQuit);

		// Exit with code
		Environment.Exit(result);
	}

	private static SDL3.SDL_AppResult AppInit(void** appState, int32 argc, char8*[] argv)
	{
		*appState = CreateGame();
		return .SDL_APP_CONTINUE;
	}

	private static SDL3.SDL_AppResult AppEvent(void* appState, SDL3.SDL_Event* evt)
	{
		// Do game event
		c_int result = GameEvent(appState, evt);

		// Check for continue
		if(result == 1)
			return .SDL_APP_CONTINUE;

		// Exit
		return .SDL_APP_SUCCESS;
	}

	private static SDL3.SDL_AppResult AppIterate(void* appState)
	{
		// Do game update
		c_int result = GameIterate(appState);

		// Check for continue
		if(result == 1)
			return .SDL_APP_CONTINUE;

		// Exit
		return .SDL_APP_SUCCESS;
	}

	private static void AppQuit(void* appState, SDL3.SDL_AppResult result)
	{
		// Do game shutdown
		GameQuit(appState);
	}
}