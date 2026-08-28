using System;
using System.IO;
using internal KoraPlayer;
using KoraPlayer.Graphics;

namespace KoraPlayer.Assets;

[AttributeUsage(.Class, .ReflectAttribute | .NotInherited, ReflectUser=.Methods, AlwaysIncludeUser=.AssumeInstantiated | .IncludeAllMethods)]
internal struct CustomAssetReaderAttribute : Attribute
{
	// Public
	public readonly Type Type;
	public readonly bool SubType;

	// Constructor
	public this(Type type, bool subType = false)
	{
	    this.Type = type;
	    this.SubType = subType;
	}
}

internal interface IAssetReader
{
	// Methods
	Result<GameElement, AssetLoadError> ReadAsset(in AssetLoadContext context, Stream stream);
}