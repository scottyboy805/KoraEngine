using System;
using System.IO;

namespace KoraPlayer.Assets;

internal sealed class SubStream : Stream
{
	// Private
    private Stream baseStream;
    private int64 baseOffset;
    private int64 length;
    private int64 position;
    private int64 cachedBasePos;
    private bool hasCache;

	// Properties
	public override int64 Position
	{
	    get => position;
	    set
	    {
	        if (value < 0 || value > length)
	            Runtime.FatalError();

	        position = value;
	    }
	}

	public override int64 Length => length;
	public override bool CanRead => true;
	public override bool CanWrite => false;
	private int64 AbsolutePos => baseOffset + position;

	// Constructor
    public this(Stream baseStream, int64 offset, int64 length)
    {
        this.baseStream = baseStream;
        this.baseOffset = offset;
        this.length = length;

        this.position = 0;

        this.cachedBasePos = 0;
        this.hasCache = false;
    }

	// Methods
    public override Result<void> Close()
    {
        return .Ok;
    }

    public override Result<int> TryRead(Span<uint8> data)
    {
        if (position >= length)
            return 0;

        int64 remaining = length - position;
        int toRead = (int)Math.Min(data.Length, remaining);

        Try!(SyncBasePosition());

        let slice = Span<uint8>(data.Ptr, toRead);
        let result = baseStream.TryRead(slice);

        switch (result)
        {
        case .Ok(let readCount):
            position += readCount;
            cachedBasePos += readCount;
            return readCount;

        case .Err:
            return result;
        }
    }

    public override Result<int> TryWrite(Span<uint8> data)
    {
        Runtime.FatalError();
    }

	private Result<void> SyncBasePosition()
	{
	    int64 target = AbsolutePos;

	    if (!hasCache || cachedBasePos != target)
	    {
	        Try!(baseStream.Seek(target, .Absolute));
	        cachedBasePos = target;
	        hasCache = true;
	    }

	    return .Ok;
	}
}