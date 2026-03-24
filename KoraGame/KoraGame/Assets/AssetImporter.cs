
namespace KoraGame.Assets
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class AssetImporterAttribute : Attribute
    {
        // Private
        private readonly string extension;

        // Properties
        public string Extension => extension;

        // Constructor
        public AssetImporterAttribute(string extension)
        {
            this.extension = extension;
        }
    }

    public interface IAssetImporter
    {
        // Methods
        Task<GameElement> ImportAsync(AssetReadContext context, Stream stream, CancellationToken cancellationToken);
    }
}
