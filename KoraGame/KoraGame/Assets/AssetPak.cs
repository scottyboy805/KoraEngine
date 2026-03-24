using System.Collections.Concurrent;

namespace KoraGame.Assets
{
    internal enum PackedAssetFlags : uint
    {
        TypeTable = 1 << 1,
    }

    internal struct PackedAssetHeader
    {
        // Public
        public int Magic;
        public int Version;
        public PackedAssetFlags Flags;
        public string Name;
        public uint AssetCount;
        public uint AssetTableStreamOffset;
        public uint DependencyTableOffset;
        public uint TypeTableStreamOffset;
    }

    internal struct PackedAssetEntryHeader
    {
        // Public
        public int TypeID;
        public string AssetPath;
        public uint StreamOffset;
        public uint StreamLength;
    }

    internal struct PackedAssetType
    {
        // Public
        public string TypeName;
    }

    public sealed class AssetPak : GameElement
    {
        // Private
        private readonly AssetProvider assetProvider;
        private readonly Stream pakStream;
        private readonly PackedAssetEntryHeader[] assetHeaders;
        private readonly PackedAssetType[] assetTypes;

        // Internal
        internal readonly ConcurrentBag<string> assetPaths = new();
        internal readonly ConcurrentDictionary<string, GameElement> loadedAssets = new();

        // Properties
        public uint AssetCount => (uint)assetHeaders.Length;

        // Constructor
        internal AssetPak(AssetProvider assetProvider, Stream pakStream, string name, PackedAssetEntryHeader[] assetHeaders)
        {
            this.assetProvider = assetProvider;
            this.pakStream = pakStream;
            this.Name = name;
            this.assetHeaders = assetHeaders;

            // Add available assets
            foreach (PackedAssetEntryHeader assetEntry in assetHeaders)
                assetPaths.Add(assetEntry.AssetPath);
        }

        // Methods
        internal override void CloneInstantiate(GameElement element)
        {
            throw new NotSupportedException("Not supported for asset paks");
        }

        public IEnumerable<string> GetAssetPaths()
        {
            return assetPaths;
        }

        internal void GetAssetStream(string assetPath, out Stream stream, out Type assetType)
        {
            // Try to find header
            PackedAssetEntryHeader assetHeader = assetHeaders.FirstOrDefault(a => a.AssetPath == assetPath);

            // Create the stream
            stream = GetAssetStream(assetHeader);

            // Get the type
            assetType = GetAssetType(assetHeader);
        }

        private Stream GetAssetStream(PackedAssetEntryHeader entry)
        {
            return new SubStream(pakStream, entry.StreamOffset, entry.StreamLength);
        }

        private Type GetAssetType(PackedAssetEntryHeader entry)
        {
            // Get the type entry
            PackedAssetType assetType = assetTypes[entry.TypeID];

            // Try to get the type
            return Type.GetType(assetType.TypeName, false);
        }
    }
}
