using KoraGame.Graphics;
using StbImageSharp;

namespace KoraGame.Assets
{
    [AssetImporter(".png")]
    [AssetReader(typeof(Texture))]
    internal sealed class TextureReader : IAssetImporter, IAssetReader
    {
        // Type
        internal struct TextureHeader
        {
            // Public
            public TextureFormat Format;
            public TextureShape Shape;
            public uint Width;
            public uint Height;
            public uint Depth;
            public uint MipMapLevels;
            public uint StreamSize;
        }

        // Methods
        public Task<GameElement> ImportAsync(AssetReadContext context, Stream stream, CancellationToken cancellationToken)
        {
            // Try to load image
            ImageResult img = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

            // Create the texture
            Texture texture = new Texture(context.Graphics, (uint)img.Width, (uint)img.Height, TextureFormat.R8G8B8A8Unorm);
            texture.Name = context.AssetName;

            // Write the data
            texture.Write(img.Data);
            
            // Upload the texture
            context.GraphicsCmd.UploadTexture(texture);

            // Create the result
            return Task.FromResult((GameElement)texture);
        }

        public async Task<GameElement> ReadAsync(AssetReadContext context, Stream stream, CancellationToken cancellationToken)
        {
            // Create reader
            BinaryReader reader = new BinaryReader(stream);

            // Read the header
            TextureHeader header = ReadHeader(reader);

            // Initialize texture
            Texture texture = new Texture(context.Graphics,
                header.Width,
                header.Height,
                header.Depth,
                header.Format,
                header.MipMapLevels,
                TextureUsage.Sampler,
                header.Shape);

            // Map the memory and write to it async
            await texture.MapMemoryAsync((IntPtr ptr) =>
            {
                Stream textureStream = null;
                unsafe
                {
                    // Create unmanaged stream
                    textureStream = new UnmanagedMemoryStream((byte*)ptr, header.StreamSize, header.StreamSize, FileAccess.Write);
                }

                // Copy the stream
                return stream.CopyToAsync(textureStream);
            });

            // Upload the texture
            context.GraphicsCmd.UploadTexture(texture);

            // Get the loaded texture
            return texture;
        }

        private TextureHeader ReadHeader(BinaryReader reader)
        {
            return new TextureHeader
            {
                Format = (TextureFormat)reader.ReadUInt32(),
                Shape = (TextureShape)reader.ReadUInt32(),
                Width = reader.ReadUInt32(),
                Height = reader.ReadUInt32(),
                Depth = reader.ReadUInt32(),
                MipMapLevels = reader.ReadUInt32(),
                StreamSize = reader.ReadUInt32(),
            };
        }
    }
}
