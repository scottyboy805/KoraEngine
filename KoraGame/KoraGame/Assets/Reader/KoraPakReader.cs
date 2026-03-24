
namespace KoraGame.Assets
{
    [AssetImporter(PakExtension)]
    internal sealed class KoraPakReader : IAssetImporter
    {
        // Public
        public const string PakExtension = ".kpak";
        public const int PakMagic = (('K' & 0xFF))            // Kora pak file
                                  | (('P' & 0xFF) << 8)
                                  | (('A' & 0xFF) << 16)
                                  | (('K' & 0xFF) << 24);

        public const int FileVersion = 100;

        // Methods
        public async Task<GameElement> ImportAsync(AssetReadContext context, Stream stream, CancellationToken cancellationToken)
        {
            // Check header
            KoraAssetReader.CheckS3DFormat(stream, PakMagic);

            // Load the pak into memory
            MemoryStream pakStream = new MemoryStream((int)stream.Length);

            // Wait for read
            await stream.CopyToAsync(pakStream);

            // Create reader
            BinaryReader reader = new BinaryReader(pakStream);

            // Try to read header
            PackedAssetHeader header = KoraAssetReader.ReadHeader(reader, PakMagic);


            return null;
        }
    }
}
