using System;
using SDL3;
using internal KoraPlayer;

namespace KoraPlayer;

public sealed class Screen : ScriptBinding
{
	// Internal
	internal readonly SDL_Window* window;

	// Properties
	public uint32 Width
	{
		get
		{
			// Get size
			int32 w = 0, h = 0;
			SDL3.SDL_GetWindowSize(window, &w, &h);

			// Report width
			return (uint32)w;
		}
	}

	public uint32 Height
	{
		get
		{
			// Get size
			int32 w = 0, h = 0;
			SDL3.SDL_GetWindowSize(window, &w, &h);

			// Report height
			return (uint32)h;
		}
	}

	public bool Fullscreen
	{
		get => SDL3.SDL_GetWindowFlags(window) & .SDL_WINDOW_FULLSCREEN != 0;
	}

	// Constructor
	internal this(String title, uint32 width, uint32 height, bool fullscreen)
	{
		SDL_WindowFlags flags = 0;

		// Check for full screen
		if(fullscreen == true)
			flags |= .SDL_WINDOW_FULLSCREEN;

		// Create the window
		window = SDL3.SDL_CreateWindow(title, (int32)width, (int32)height, flags);
	}

	internal ~this()
	{
		SDL3.SDL_DestroyWindow(window);
	}
}

static
{
	[Export, CLink]
	public static uint32 ScreenGetWidth(NativeObject screen)
	{
		return ScriptBinding.GetNativeObject<Screen>(screen)?.Width ?? 0;
	}
}