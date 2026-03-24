
namespace KoraGame.Assets
{
    [AssetImporter(PakExtension)]
    internal sealed class S3DPakReader : IAssetImporter
    {
        // Public
        public const string PakExtension = ".s3dpak";
        public const int PakMagic = (('S' & 0xFF))            // Simple 3d pak
                                  | (('3' & 0xFF) << 8)
                                  | (('D' & 0xFF) << 16)
                                  | (('P' & 0xFF) << 24);

        public const int FileVersion = 100;

        // Methods
        public async Task<GameElement> ImportAsync(AssetReadContext context, Stream stream, CancellationToken cancellationToken)
        {
            // Check header
            S3DAssetReader.CheckS3DFormat(stream, PakMagic);

            // Load the pak into memory
            MemoryStream pakStream = new MemoryStream((int)stream.Length);

            // Wait for read
            await stream.CopyToAsync(pakStream);

            // Create reader
            BinaryReader reader = new BinaryReader(pakStream);

            // Try to read header
            PackedAssetHeader header = S3DAssetReader.ReadHeader(reader, PakMagic);


            return null;
        }
    }
}
