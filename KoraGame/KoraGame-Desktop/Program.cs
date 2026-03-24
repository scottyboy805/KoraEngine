using SDL;
using KoraGame;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

internal unsafe static class Program
{
    // Methods
    static void Main(string[] args)
    {
        // Convert args to native UTF-8 **argv** array
        byte** argv = stackalloc byte*[args.Length];
        for (int i = 0; i < args.Length; i++)
        {
            argv[i] = (byte*)Marshal.StringToHGlobalAnsi(args[i]);
        }

        int result = SDL3.SDL_EnterAppMainCallbacks(
            args.Length,
            argv,
            &AppInit,
            &AppIterate,
            &AppEvent,
            &AppQuit
        );

        // Clean up argv allocations
        for (int i = 0; i < args.Length; i++)
        {
            Marshal.FreeHGlobal((IntPtr)argv[i]);
        }

        Environment.Exit(result);
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    internal static SDL_AppResult AppInit(IntPtr* appState, int argc, byte** argv)
    {
        // Create the game
        Game game = new Game();

        // Initialize the game
        game.DoInitialize();

        // Pin the game
        GCHandle gameHandle = GCHandle.Alloc(game, GCHandleType.Normal);
        *appState = (IntPtr)gameHandle;

        // Continue the game
        return SDL_AppResult.SDL_APP_CONTINUE;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    internal static SDL_AppResult AppEvent(IntPtr appState, SDL_Event* eventPtr)
    {
        // Get the handle
        GCHandle gameHandle = (GCHandle)appState;
        Game game = (Game)gameHandle.Target;

        // Handle the event
        game.DoEvent(*eventPtr);

        // Check for quit
        if (game.Quit == true)
            return SDL_AppResult.SDL_APP_SUCCESS;

        // Continue the game
        return SDL_AppResult.SDL_APP_CONTINUE;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    internal static SDL_AppResult AppIterate(IntPtr appState)
    {
        // Get the handle
        GCHandle gameHandle = (GCHandle)appState;
        Game game = (Game)gameHandle.Target;

        // Update the game
        game.DoUpdate();

        // Continue the game
        return SDL_AppResult.SDL_APP_CONTINUE;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    internal static void AppQuit(IntPtr appState, SDL_AppResult result)
    {
        // Get the handle
        GCHandle gameHandle = (GCHandle)appState;
        Game game = (Game)gameHandle.Target;

        // Shutdown the game
        game.DoShutdown();

        // Free the handle
        gameHandle.Free();
    }
}