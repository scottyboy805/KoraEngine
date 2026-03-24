
namespace KoraGame.Assets
{
    [AssetImporter(".txt")]
    [AssetImporter(".text")]
    [AssetImporter(".bytes")]
    [AssetImporter(".spv")]
    [AssetReader(typeof(RawAsset))]
    internal sealed class RawReader : IAssetImporter, IAssetReader
    {
        // Methods
        public async Task<GameElement> ImportAsync(AssetReadContext context, Stream stream, CancellationToken cancellationToken)
        {
            // Read into memory
            MemoryStream memory = new MemoryStream();

            // Copy async
            await stream.CopyToAsync(memory, cancellationToken);

            // Create the raw asset
            RawAsset asset = new RawAsset(memory);
            asset.Name = context.AssetName;

            return asset;
        }

        public Task<GameElement> ReadAsync(AssetReadContext context, Stream stream, CancellationToken cancellationToken)
        {
            // Use the same load behaviour
            return ImportAsync(context, stream, cancellationToken);
        }
    }
}
