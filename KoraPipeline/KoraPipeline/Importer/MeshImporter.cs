
namespace KoraPipeline.Importer
{
    [AssetImporterFor(".fbx")]
    internal sealed class MeshImporter : AssetImporter
    {       
        // Methods
        public override Task ImportAssetAsync(AssetImportContext context, Stream inputStream, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override Task BuildAssetAsync(AssetImportContext context, Stream outputStream, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
