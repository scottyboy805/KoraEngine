
namespace KoraGame.Assets
{
    [AssetImporter(AssetExtension)]
    internal sealed class KoraAssetReader : IAssetImporter
    {
        // Public
        public const string AssetExtension = ".asset";
        public const int AssetMagic = (('K' & 0xFF))            // Kora asset
                                    | (('O' & 0xFF) << 8)
                                    | (('R' & 0xFF) << 16)
                                    | (('A' & 0xFF) << 24);

        public const int FileVersion = 100;

        // Methods
        public async Task<GameElement> ImportAsync(AssetReadContext context, Stream stream, CancellationToken cancellationToken)
        {
            // Check header
            KoraAssetReader.CheckS3DFormat(stream, AssetMagic);

            // Create reader
            BinaryReader reader = new BinaryReader(stream);

            // Try to read header
            PackedAssetHeader header = KoraAssetReader.ReadHeader(reader, AssetMagic);

            // Try to read asset entry
            PackedAssetEntryHeader assetHeader = KoraAssetReader.ReadAssetHeader(reader);

            // Try to read the dependency table
            ReadStringTable(reader, context.ReferencedExternalObjects);

            // Try to read the type table
            ReadStringTable(reader, context.ReferencedTypes);


            // Create sub stream
            Stream assetStream = new SubStream(stream, assetHeader.StreamOffset, assetHeader.StreamLength);
            assetStream.Seek(0, SeekOrigin.Begin);


            // Get the type
            Type type = Type.GetType(context.ReferencedTypes[0], false);

            // Get the reader
            if (context.assets.GetAssetReader(type, out IAssetReader assetReader) == false)
                return null;

            // Read the element
            return await assetReader.ReadAsync(context, assetStream, cancellationToken);
        }

        public static void CheckS3DFormat(Stream stream, int expectedMagic)
        {
            try
            {
                // Read 4 bytes
                byte[] bytes = new byte[sizeof(int)];

                // Not enough bytes
                if (stream.Read(bytes, 0, bytes.Length) != bytes.Length)
                    throw new FormatException();

                // Get int
                if(BitConverter.ToInt32(bytes) != expectedMagic)
                    throw new FormatException();
            }
            catch
            {
                throw new FormatException("Not a valid S3D format");
            }
        }

        public static PackedAssetHeader ReadHeader(BinaryReader reader, int expectedMagic)
        {
            // Read the header
            return new PackedAssetHeader
            {
                Magic = expectedMagic,
                Version = reader.ReadInt32(),
                Flags = (PackedAssetFlags)reader.ReadUInt32(),
                Name = reader.ReadString(),
                AssetCount = reader.ReadUInt32(),
                AssetTableStreamOffset = reader.ReadUInt32(),
                DependencyTableOffset = reader.ReadUInt32(),
                TypeTableStreamOffset = reader.ReadUInt32(),
            };
        }

        public static PackedAssetEntryHeader[] ReadAssetHeaders(BinaryReader reader)
        {
            // Read count
            uint count = reader.ReadUInt32();

            // Allocate array
            PackedAssetEntryHeader[] assetHeaders = new PackedAssetEntryHeader[count];

            // Read all
            for (int i = 0; i < count; i++)
            {
                // Read the header
                assetHeaders[i] = ReadAssetHeader(reader);
            }

            return assetHeaders;
        }

        public static PackedAssetEntryHeader ReadAssetHeader(BinaryReader reader)
        {
            return new PackedAssetEntryHeader
            {
                TypeID = reader.ReadUInt16(),
                AssetPath = reader.ReadString(),
                StreamOffset = reader.ReadUInt32(),
                StreamLength = reader.ReadUInt32(),
            };
        }

        //public static PackedAssetType[] ReadAssetTypeTable(BinaryReader reader)
        //{
        //    // Read count
        //    uint count = reader.ReadUInt32();

        //    // Allocate array
        //    PackedAssetType[] typeTable = new PackedAssetType[count];

        //    // Read all
        //    for (int i = 0; i < count; i++)
        //    {
        //        // Read the type
        //        typeTable[i] = ReadAssetType(reader);
        //    }

        //    return typeTable;
        //}

        //public static PackedAssetType ReadAssetType(BinaryReader reader)
        //{
        //    return new PackedAssetType
        //    {
        //        TypeName = reader.ReadString(),
        //    };
        //}

        public static void ReadStringTable(BinaryReader reader, IList<string> table)
        {
            // Read count
            int count = reader.ReadInt32();

            // Allocate array
            string[] array = new string[count];

            // Read all elements
            for(int i = 0; i < count; i++)
                table.Add(reader.ReadString());
        }
    }
}
