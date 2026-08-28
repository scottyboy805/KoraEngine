using System;
using System.IO;
using internal KoraPlayer;

namespace KoraPlayer.Assets;

[CustomAssetImporter(".txt")]
[CustomAssetImporter(".text")]
[CustomAssetImporter(".bytes")]
[CustomAssetImporter(".dat")]
[CustomAssetImporter(".spv")]
internal sealed class RawAssetReader : IAssetImporter
{
	// Methods
	public Result<GameElement, AssetLoadError> ImportAsset(in AssetLoadContext context, Stream stream)
	{
		// Create stream
		MemoryStream memory = new .();

		// Copy data
		stream.CopyTo(memory);

		// Create the raw asset
		RawAsset asset = new .(memory, context.AssetName);

		// Get the asset
		return .Ok(asset);
	}
}