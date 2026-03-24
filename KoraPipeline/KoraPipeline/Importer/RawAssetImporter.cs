
namespace KoraPipeline.Importer
{
    [AssetImporterFor(".txt")]
    [AssetImporterFor(".text")]
    [AssetImporterFor(".bytes")]
    [AssetImporterFor(".spv")]
    internal sealed class RawAssetImporter : AssetImporter
    {
        // Private
        private MemoryStream buffer;
        
        // Methods
        public override Task ImportAssetAsync(AssetImportContext context, Stream inputStream, CancellationToken cancellationToken)
        {
            // Copy to memory
            buffer = new MemoryStream();

            // Copy to buffer
            return inputStream.CopyToAsync(buffer, cancellationToken);
        }

        public override Task BuildAssetAsync(AssetImportContext context, Stream outputStream, CancellationToken cancellationToken)
        {
            // Write to output stream
            return buffer.CopyToAsync(outputStream, cancellationToken);
        }
    }
}
