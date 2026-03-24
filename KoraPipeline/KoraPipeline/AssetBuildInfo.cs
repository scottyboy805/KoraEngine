
namespace KoraPipeline
{
    public sealed class AssetBuildInfo
    {
        // Private
        private string assetBuildPath;
        //private AssetImportMetadata metadata;
        private ulong assetId;

        // Properties
        public string AssetBuildPath => assetBuildPath;
        //public AssetImportMetadata Metadata => metadata;
        public ulong AssetId => assetId;

        // Constructor
        internal AssetBuildInfo(string assetBuildPath, /*AssetImportMetadata metadata, */ulong assetId)
        {
            this.assetBuildPath = assetBuildPath;          
            //this.metadata = metadata;
            this.assetId = assetId;
        }
    }
}
