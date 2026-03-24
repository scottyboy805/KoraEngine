using System.Text;

namespace KoraGame
{
    public class RawAsset : GameElement
    {
        // Private
        private MemoryStream stream;

        // Properties
        public uint Length => stream != null ? (uint)stream.Length : 0;

        // Constructor
        private RawAsset() { }

        internal RawAsset(MemoryStream stream)
        {
            this.stream = stream;
        }

        // Methods
        protected override void OnDestroy()
        {
            if(stream != null)
            {
                stream.Dispose();
                stream = null;
            }
        }

        internal override void CloneInstantiate(GameElement element)
        {
            base.CloneInstantiate(element);

            // Get the clone
            RawAsset clone = (RawAsset)element;

            // Copy the stream
            clone.stream = new MemoryStream();
            this.stream.CopyTo(clone.stream);
        }

        public ReadOnlySpan<byte> GetBytes()
        {
            // Check for none
            if (stream == null)
                return Array.Empty<byte>();

            // Get as span to avoid copy
            return new ReadOnlySpan<byte>(stream.GetBuffer(), 0, (int)stream.Length);
        }

        public string GetText(Encoding encoding = null)
        {
            // Check for none
            if (stream == null)
                return string.Empty;

            // Check for encoding
            if (encoding == null)
                encoding = Encoding.UTF8;

            // Convert bytes
            return encoding.GetString(stream.GetBuffer(), 0, (int)stream.Length);
        }
    }
}
