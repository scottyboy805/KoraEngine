using System;
using System.IO;
using KoraPlayer.Graphics;
using internal KoraPlayer;

namespace KoraPlayer.Assets;

[CustomAssetReader(typeof(Texture))]
internal sealed class TextureReader : IAssetReader
{
	// Type
	internal struct TextureHeader
	{
		// Public
		public TextureFormat Format;
		public TextureShape Shape;
		public uint32 Width;
		public uint32 Height;
		public uint32 Depth;
		public uint32 MipMapLevels;
		public uint32 StreamSize;
	}

	// Methods
	public Result<GameElement, AssetLoadError> ReadAsset(in AssetLoadContext context, Stream stream)
	{
		// Read the header
		TextureHeader header = ReadHeader(stream);

		// Initialize texture
		Texture texture = new Texture(
			context.Graphics,
			header.Width,
			header.Height,
			header.Format,
			header.Shape,
			.Sampler,
			header.MipMapLevels);

		// Create the temp buffer
		uint8[] buffer = scope uint8[header.Width * header.Height * header.Depth];

		// Create temp copy stream
		FixedMemoryStream bufferStream = scope .(buffer);

		// Copy texture data
		stream.CopyTo(bufferStream);

		// Write to texture memory
		texture.Write(Span<uint8>(buffer));

		// Upload the texture
		context.GraphicsCmd.UploadTexture(texture);

		// Get the result
		return .Ok(texture);
	}

	private TextureHeader ReadHeader(Stream stream)
	{
		return TextureHeader
		{
		  	Format = (TextureFormat)stream.Read<uint32>(),
			Shape = (TextureShape)stream.Read<uint32>(),
			Width = stream.Read<uint32>(),
			Height = stream.Read<uint32>(),
			Depth = stream.Read<uint32>(),
			MipMapLevels = stream.Read<uint32>(),
			StreamSize = stream.Read<uint32>(),
		};
	}
}