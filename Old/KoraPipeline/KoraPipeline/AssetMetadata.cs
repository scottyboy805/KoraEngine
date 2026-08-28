using System.Runtime.Serialization;

namespace KoraPipeline
{
    [Serializable]
    internal sealed class AssetMetadata
    {
        // Private
        [DataMember(Name = "Guid")]
        private string guid = "";

        private string assetPath = "";

        // Public
        public const string AssetMetaExtension = ".kora";

        // Properties
        public string Guid => guid;
        public string AssetPath
        {
            get => assetPath;
            set => assetPath = value;
        }
        public string MetaPath => assetPath + AssetMetaExtension;

        // Constructor
        public AssetMetadata(string contentPath)
        {
            this.assetPath = contentPath;
            this.EnsureGuid();
        }

        internal void EnsureGuid()
        {
            if (string.IsNullOrEmpty(guid) == true)
            {
                guid = System.Guid
                    .NewGuid()
                    .ToString()
                    .Replace("-", "");
            }
        }
    }
}
