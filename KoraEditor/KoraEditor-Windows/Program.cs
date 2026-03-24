using SDL;
using KoraGame;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using KoraEditor;

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
        Editor editor = new Editor();

        // Initialize the game
        editor.DoInitialize();

        // Pin the game
        GCHandle editorHandle = GCHandle.Alloc(editor, GCHandleType.Normal);
        *appState = (IntPtr)editorHandle;

        // Continue the game
        return SDL_AppResult.SDL_APP_CONTINUE;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    internal static SDL_AppResult AppEvent(IntPtr appState, SDL_Event* eventPtr)
    {
        // Get the handle
        GCHandle editorHandle = (GCHandle)appState;
        Editor editor = (Editor)editorHandle.Target;

        // Handle the event
        editor.DoEvent(*eventPtr);

        // Check for quit
        if (editor.Quit == true)
            return SDL_AppResult.SDL_APP_SUCCESS;

        // Continue the game
        return SDL_AppResult.SDL_APP_CONTINUE;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    internal static SDL_AppResult AppIterate(IntPtr appState)
    {
        // Get the handle
        GCHandle editorHandle = (GCHandle)appState;
        Editor editor = (Editor)editorHandle.Target;

        // Update the game
        editor.DoUpdate();

        // Continue the game
        return SDL_AppResult.SDL_APP_CONTINUE;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    internal static void AppQuit(IntPtr appState, SDL_AppResult result)
    {
        // Get the handle
        GCHandle editorHandle = (GCHandle)appState;
        Editor editor = (Editor)editorHandle.Target;

        // Shutdown the game
        editor.DoShutdown();

        // Free the handle
        editorHandle.Free();
    }
}