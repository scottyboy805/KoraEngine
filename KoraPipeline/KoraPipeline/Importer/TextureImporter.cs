using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using KoraGame.Assets;
using KoraGame.Graphics;
using StbImageSharp;
using System.Runtime.Serialization;

namespace KoraPipeline.Importer
{
    [AssetImporterFor(".png")]
    internal sealed class TextureImporter : AssetImporter
    {
        // Private
        private ImageResult image;

        // Public
        [DataMember]
        public bool GenerateMipMaps = true;
        [DataMember]
        public int MipMapLevels = -1;
        [DataMember]
        public CompressionQuality Quality = CompressionQuality.Balanced;

        // Methods
        public override async Task ImportAssetAsync(AssetImportContext context, Stream inputStream, CancellationToken cancellationToken)
        {
            // Run load async
            await Task.Run(() =>
            {
                // Load the image
                image = ImageResult.FromStream(inputStream, ColorComponents.RedGreenBlueAlpha);
            });

            // Assign main type
            context.MainType = typeof(Texture);
        }

        public override async Task BuildAssetAsync(AssetImportContext context, Stream outputStream, CancellationToken cancellationToken)
        {
            // Get the image bytes
            byte[] imageBytes = image.Data;

            // Create encoder
            BcEncoder encoder = new BcEncoder();
            encoder.OutputOptions.GenerateMipMaps = GenerateMipMaps;
            encoder.OutputOptions.MaxMipMapLevel = MipMapLevels;
            encoder.OutputOptions.Quality = Quality;
            encoder.OutputOptions.Format = CompressionFormat.Bc7;

            // Encode image bytes to stream
            byte[][] encodedMipMaps = await encoder.EncodeToRawBytesAsync(imageBytes, image.Width, image.Height, PixelFormat.Rgba32, cancellationToken);

            // Create the header
            TextureReader.TextureHeader header = new TextureReader.TextureHeader
            {
                Format = TextureFormat.Sbc7RgbaUnorm,
                Shape = TextureShape.Texture2D,
                Width = (uint)image.Width,
                Height = (uint)image.Height,
                Depth = 1,
                MipMapLevels = (uint)encodedMipMaps.Length,
                StreamSize = (uint)imageBytes.Length,
            };

            // Create the writer
            BinaryWriter writer = new BinaryWriter(outputStream);

            // Write the header
            WriteHeader(writer, header);

            // Write all mip maps
            foreach (byte[] mipMap in encodedMipMaps)
            {
                outputStream.Write(mipMap);


                // ONLY WRITE FIRST FOR TESTING
                break;
            }
        }

        private void WriteHeader(BinaryWriter writer, TextureReader.TextureHeader header)
        {
            writer.Write((uint)header.Format);
            writer.Write((uint)header.Shape);
            writer.Write(header.Width);
            writer.Write(header.Height);
            writer.Write(header.Depth);
            writer.Write(header.MipMapLevels);
            writer.Write(header.StreamSize);
        }
    }
}
