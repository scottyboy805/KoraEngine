using System;
using System.IO;
using System.Text;

namespace KoraPlayer;

public sealed class RawAsset : NativeElement
{
	// Private
	private MemoryStream stream;

	// Properties
	public uint32 Length => stream != null ? (uint32)stream.Length : 0;

	// Constructor
	internal this(MemoryStream stream, StringView name)
	{
		this.stream = stream;
	}

	public ~this()
	{
		if(stream != null)
		{
			delete stream;
			stream = null;
		}
	}

	// Methods
	public Span<uint8> GetBytes()
	{
		// Check for none
		if(stream == null)
			return Span<uint8>();

		// Get span
		return Span<uint8>(stream.Memory.Ptr, stream.Length);
	}

	public void GetText(String outStr, Encoding encoding = null)
	{
		// Check for none
		if(stream == null)
		{
			outStr.Set(String.Empty);
			return;
		}

		// Check for encoding
		Encoding e = encoding == null
			? Encoding.UTF8
			: encoding;

		// Get span
		Span<uint8> span = Span<uint8>(stream.Memory.Ptr, stream.Length);

		// Convert bytes
		e.DecodeToUTF8(span, outStr);
	}
}