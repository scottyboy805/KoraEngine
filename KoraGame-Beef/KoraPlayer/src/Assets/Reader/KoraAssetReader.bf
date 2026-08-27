using System;
using System.IO;
using internal KoraPlayer;

namespace KoraPlayer.Assets.Reader;

internal enum KoraAssetFlags : uint
{
    TypeTable = 1 << 1,
}

internal struct KoraAssetHeader
{
	// Public
	public int32 Magic;
	public int32 Version;
	public KoraAssetFlags Flags;
	public String Name;
	public uint32 TypeTableIndex;
	public uint32 AssetStreamOffset;
	public uint32 AssetStreamSize;
}

internal sealed class KoraAssetReader : IAssetImporter
{
	// Public
	public const String AssetExtension = ".kasset";
	public const int32 AssetMagic = (((int)'K' & 0xFF))            // Kora asset
                                    | (((int)'O' & 0xFF) << 8)
                                    | (((int)'R' & 0xFF) << 16)
                                    | (((int)'A' & 0xFF) << 24);

	public const int32 FileVersion = 100;

	// Public
	public Result<GameElement, AssetLoadError> ImportAsset(in AssetLoadContext context, Stream stream)
	{
		// Check file kind
		let magic = CheckKoraFormat(stream);

		// Check for file format error
		if(magic case .Err(var e))
			return .Err(e);

		// Read the header
		String name = scope .();
		let header = ReadKoraHeader(stream, magic, name);

		// Check for header error
		if(header case .Err)
			return .Err(.InvalidHeader);

		// Check asset version
		if(header.Value.Version > FileVersion)
			return .Err(.InvalidVersion);


		// Get sub stream
		Stream assetStream = scope SubStream(stream, header.Value.AssetStreamOffset, header.Value.AssetStreamSize);
		assetStream.Seek(0, .Absolute);

		return .Err(	.InvalidData);
	}

	// Methods
	public static Result<int32, AssetLoadError> CheckKoraFormat(Stream stream)
	{
		// Try to read magic
		let magic = stream.Read<int32>();

		// Check for error
		if(magic case .Err)
			return .Err(.NotEnoughData);

		// Check for match
		if(magic != AssetMagic)
			return .Err(.InvalidFormat);

		return .Ok(magic);
	}

	public static Result<KoraAssetHeader> ReadKoraHeader(Stream stream, int32 magic, String name)
	{
		// Read version
		let version = Try!(stream.Read<int32>());

		// Read flags
		let flags = Try!(stream.Read<uint32>());

		// Read name
		Try!(stream.ReadStrSized32(name));

		// Read asset type index
		uint32 typeTableIndex = Try!(stream.Read<uint32>());

		// Read asset stream offset
		uint32 assetStreamOffset = Try!(stream.Read<uint32>());

		// Read asset stream size
		uint32 assetStreamSize = Try!(stream.Read<uint32>());

		// Create header
		return KoraAssetHeader
		{
		  	Magic = magic,
			Version = version,
			Flags = (KoraAssetFlags)flags,
			Name = name,
			TypeTableIndex = typeTableIndex,
			AssetStreamOffset = assetStreamOffset,
			AssetStreamSize = assetStreamSize,
		};
	}

}