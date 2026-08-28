using System;

namespace KoraPlayer;

[AttributeUsage(.Field | .All, .ReflectAttribute | .NotInherited | .DisallowAllowMultiple, ReflectUser=.NonStaticFields)]
public struct SerializeFieldAttribute : Attribute
{
	// Public
	public readonly String SerializeName;

	// Constructor
	public this()
	{
		SerializeName = null;
	}

	public this(String name)
	{
		this.SerializeName = name;
	}
}