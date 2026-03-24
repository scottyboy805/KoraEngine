
namespace KoraGame.Assets
{
    internal sealed class SubStream : Stream
    {
        // Private
        private readonly Stream baseStream;
        private readonly long start;
        private readonly long length;
        private long position;

        // Properties
        public override bool CanRead => baseStream.CanRead;
        public override bool CanSeek => baseStream.CanSeek;
        public override bool CanWrite => false;
        public override long Length => length;

        public override long Position
        {
            get => position;
            set
            {
                if (value < 0 || value > length)
                    throw new ArgumentOutOfRangeException(nameof(value));
                position = value;
                baseStream.Seek(start + position, SeekOrigin.Begin);
            }
        }

        // Constructor
        public SubStream(Stream baseStream, long start, long length)
        {
            if (baseStream == null)
                throw new ArgumentNullException(nameof(baseStream));
            if (!baseStream.CanSeek)
                throw new ArgumentException("Base stream must support seeking.", nameof(baseStream));
            if (start < 0 || length < 0 || start + length > baseStream.Length)
                throw new ArgumentOutOfRangeException("Invalid start or length.");

            this.baseStream = baseStream;
            this.start = start;
            this.length = length;
            position = 0;

            this.baseStream.Seek(this.start, SeekOrigin.Begin);
        }

        // Methods
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (position >= length)
                return 0; // EOF

            long remaining = length - position;
            if (count > remaining)
                count = (int)remaining;

            int bytesRead = baseStream.Read(buffer, offset, count);
            position += bytesRead;
            return bytesRead;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long newPos;
            switch (origin)
            {
                case SeekOrigin.Begin:
                    newPos = offset;
                    break;
                case SeekOrigin.Current:
                    newPos = position + offset;
                    break;
                case SeekOrigin.End:
                    newPos = length + offset;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(origin), origin, null);
            }

            if (newPos < 0 || newPos > length)
                throw new IOException("Attempted to seek outside the substream range.");

            position = newPos;
            baseStream.Seek(start + position, SeekOrigin.Begin);
            return position;
        }

        public override void Flush() => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        public static Stream OpenDedicated(Stream parentStream, long start, long length)
        {
            // Check for file stream - open a new stream so we can read from multiple threads
            if (parentStream is FileStream fs)
                parentStream = File.OpenRead(fs.Name);

            // Create the sub portion
            return new SubStream(parentStream, start, length);
        }
    }
}
