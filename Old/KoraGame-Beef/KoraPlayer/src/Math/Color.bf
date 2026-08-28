using System;
using SDL3;

namespace KoraPlayer;

public struct Color
{
	// Public
	public float r, g, b, a;

	public const Color White = Color(1f, 1f, 1f);
	public const Color Black = Color(0f, 0f, 0f);
	public const Color Gray = Color(0.2f, 0.2f, 0.2f);
	public const Color Red = Color(1f, 0f, 0f);
	public const Color Green = Color(0f, 1f, 0f);
	public const Color Blue = Color(0f, 0f, 1f);

	// Constructor
	public this(float r, float g, float b, float a = 1f)
	{
		this.r = r;
		this.g = g;
		this.b = b;
		this.a = a;
	}

	// Methods
	[Inline]
	internal SDL_FColor SDL()
	{
		// Convert to SDL
		return SDL_FColor{r=r, g=g, b=b, a=a};
	}
}