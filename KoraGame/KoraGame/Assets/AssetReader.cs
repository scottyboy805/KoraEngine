using KoraGame.Graphics;

namespace KoraGame.Assets
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class AssetReaderAttribute : Attribute
    {
        // Private
        private readonly Type type;
        private readonly bool subType;

        // Properties
        public Type Type => type;
        public bool SubType => subType;

        // Constructor
        public AssetReaderAttribute(Type type, bool subType = false)
        {
            this.type = type;
            this.subType = subType;
        }
    }

    public readonly struct AssetReadContext
    {
        // Public
        public readonly Type AssetType;
        public readonly string AssetName;
        public readonly string AssetExtension;
        public readonly int DependencyDepth;
        public readonly IList<string> SearchDirectories = new List<string>();
        public readonly IList<string> ReferencedTypes = new List<string>();
        public readonly IList<string> ReferencedExternalObjects = new List<string>();
        public readonly GraphicsCommand GraphicsCmd;

        // Internal
        internal readonly AssetProvider assets;

        // Properties
        public bool IsDependency => DependencyDepth > 0;
        public ScriptableProvider Scriptable => assets?.Scriptable;
        public GraphicsDevice Graphics => assets?.Graphics;

        // Constructor
        internal AssetReadContext(AssetProvider assets, Type assetType, string assetNameAndExtension, AssetReadContext? parent)
        {
            // Check null
            if (assets == null)
                throw new ArgumentNullException(nameof(assets));

            // Check type
            if (assetType == null)
                assetType = typeof(GameElement);

            this.assets = assets;
            this.AssetType = assetType;
            this.AssetName = Path.GetFileNameWithoutExtension(assetNameAndExtension);
            this.AssetExtension = Path.GetExtension(assetNameAndExtension);
            this.DependencyDepth = parent != null ? parent.Value.DependencyDepth + 1 : 0;

            if (IsDependency == true)
            {
                this.GraphicsCmd = parent.Value.GraphicsCmd;

                foreach (string searchDirectory in parent.Value.SearchDirectories)
                    SearchDirectories.Add(searchDirectory);
            }
            else
            {
                this.GraphicsCmd = assets.Graphics.AcquireCommandBuffer();
                this.GraphicsCmd.BeginCopyPass();
            }
        }

        // Methods
        public async Task<T> LoadDependencyAsync<T>(string assetRelativePath, string assetOrPakName = null, CancellationToken cancellationToken = default) where T : GameElement
        {
            // Check for asset name
            string name = Path.GetFileName(assetRelativePath);

            // Create context
            AssetReadContext context = new AssetReadContext(assets, typeof(T), name, this);

            // Report the request
            Debug.Log($"{new string('\t', context.DependencyDepth)}Load asset dependency: '{assetRelativePath}' - {context.AssetType.FullName}", LogFilter.Assets);

            // Load from context
            return await assets.LoadContextAsync(context, assetRelativePath, assetOrPakName, cancellationToken) as T;
        }

        public async Task<GameElement> LoadDependencyAsync(string assetRelativePath, Type assetType = null, string assetOrPakName = null, CancellationToken cancellationToken = default)
        {
            // Check for asset name
            string name = Path.GetFileName(assetRelativePath);

            // Create context
            AssetReadContext context = new AssetReadContext(assets, assetType, name, this);

            // Report the request
            Debug.Log($"{new string('\t', context.DependencyDepth)}Load asset dependency: '{assetRelativePath}' - {context.AssetType.FullName}", LogFilter.Assets);

            // Load from context
            return await assets.LoadContextAsync(context, assetRelativePath, assetOrPakName, cancellationToken);
        }

        internal Task SubmitAsync()
        {
            // Check for not ours
            if (IsDependency == true)
                return Task.CompletedTask;

            // End phase
            GraphicsCmd.EndCopyPass();

            // Wait for completed
            return GraphicsCmd.SubmitAsync();
        }
    }

    public interface IAssetReader
    {
        // Methods
        Task<GameElement> ReadAsync(AssetReadContext context, Stream stream, CancellationToken cancellationToken);
    }
}
