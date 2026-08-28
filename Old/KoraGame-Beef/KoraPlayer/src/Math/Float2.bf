using System;

namespace KoraPlayer;

[CRepr]
public struct float2
{
	// Public
	public float x, y;

	// Constructor
	public this(float value)
	{
		this.x = value;
		this.y = value;
	}

	public this(float x, float y)
	{
		this.x = x;
		this.y = y;
	}
}