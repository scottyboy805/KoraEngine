using KoraGame.Assets;

namespace KoraPipeline
{
    internal sealed class KoraPackedWriter : AssetImporter
    {
        // Private
        private readonly AssetImporter mainAssetImporter;

        // Constructor
        public KoraPackedWriter(AssetImporter mainAssetImporter)
        {
            this.mainAssetImporter = mainAssetImporter;
        }

        // Methods
        public override Task ImportAssetAsync(AssetImportContext context, Stream inputStream, CancellationToken cancellationToken)
        {
            return mainAssetImporter.ImportAssetAsync(context, inputStream, cancellationToken);
        }

        public override async Task BuildAssetAsync(AssetImportContext context, Stream outputStream, CancellationToken cancellationToken)
        {
            // Open binary writer
            BinaryWriter writer = new BinaryWriter(outputStream);            

            // Create the header
            PackedAssetHeader header = new PackedAssetHeader
            {
                Magic = KoraAssetReader.AssetMagic,
                Version = KoraAssetReader.FileVersion,
                Flags = 0,
                Name = context.AssetName,
                AssetCount = 1,
                AssetTableStreamOffset = 0,
                DependencyTableOffset = 0,
                TypeTableStreamOffset = 0,
            };

            // Write the header
            WriteHeader(writer, header);

            // Create the asset header            
            PackedAssetEntryHeader assetHeader = new PackedAssetEntryHeader
            {
                TypeID = 0,
                AssetPath = context.AssetPath,
                StreamOffset = 0,
                StreamLength = 0,
            };            

            // Write asset header
            header.AssetTableStreamOffset = (uint)outputStream.Position;
            WriteAssetHeader(writer, assetHeader);            

            // Write actual asset to temp stream
            Stream stagingStream = new MemoryStream();
            {
                // Write the main asset
                await mainAssetImporter.BuildAssetAsync(context, stagingStream, cancellationToken);
            }

            // Check main type set
            if (context.MainType == null)
                throw new InvalidOperationException("AssetImportContext.MainType must be set during import or build: " + mainAssetImporter.GetType());

            // Write the dependency table
            header.DependencyTableOffset = (uint)outputStream.Position;
            WriteStringTable(writer, context.ReferencedExternalDependencies);

            // Write the type table
            header.TypeTableStreamOffset = (uint)outputStream.Position;
            WriteStringTable(writer, context.ReferencedTypeIds);

            long contentStart = outputStream.Position;

            // Write the actual data
            stagingStream.Position = 0;
            await stagingStream.CopyToAsync(outputStream);

            // Update header
            assetHeader.StreamOffset = (uint)contentStart;
            assetHeader.StreamLength = (uint)stagingStream.Length;

            // Overwrite the header
            outputStream.Seek(0, SeekOrigin.Begin);
            WriteHeader(writer, header);
            WriteAssetHeader(writer, assetHeader);
        }

        private void WriteHeader(BinaryWriter writer, PackedAssetHeader header)
        {
            writer.Write(header.Magic);
            writer.Write(header.Version);
            writer.Write((uint)header.Flags);
            writer.Write(header.Name);
            writer.Write(header.AssetCount);
            writer.Write(header.AssetTableStreamOffset);
            writer.Write(header.DependencyTableOffset);
            writer.Write(header.TypeTableStreamOffset);
        }

        private void WriteAssetHeader(BinaryWriter writer, PackedAssetEntryHeader header)
        {
            writer.Write((ushort)header.TypeID);
            writer.Write(header.AssetPath);
            writer.Write(header.StreamOffset);
            writer.Write(header.StreamLength);
        }

        private void WriteStringTable(BinaryWriter writer, IReadOnlyList<string> values)
        {
            // Write count
            writer.Write(values.Count);

            // Write all
            foreach (string value in values)
                writer.Write(value);
        }
    }
}
