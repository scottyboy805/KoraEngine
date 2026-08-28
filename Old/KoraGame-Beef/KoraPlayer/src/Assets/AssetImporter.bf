using System;
using System.IO;
using internal KoraPlayer;

namespace KoraPlayer.Assets;

[AttributeUsage(.Class, .ReflectAttribute | .NotInherited, ReflectUser=.Methods, AlwaysIncludeUser=.AssumeInstantiated | .IncludeAllMethods)]
internal struct CustomAssetImporterAttribute : Attribute
{
	// Public
	public readonly String Extension;

	// Constructor
	public this(String ext)
	{
		this.Extension = ext;
	}
}

internal interface IAssetImporter
{
	// Methods
	Result<GameElement, AssetLoadError> ImportAsset(in AssetLoadContext context, Stream stream);
}