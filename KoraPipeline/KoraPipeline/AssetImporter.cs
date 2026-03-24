using KoraGame;

namespace KoraPipeline
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class AssetImporterForAttribute : Attribute
    {
        // Private
        private string extension;

        // Properties
        public string Extension => extension;

        // Constructor
        public AssetImporterForAttribute(string extension)
        {
            this.extension = extension;
        }
    }

    public sealed class AssetImportContext
    {
        // Type
        private sealed class AssetImportReferenceContext : SerializedBinary.SerializedReferenceContext
        {
            // Private
            private ScriptableProvider scriptable;

            // Internal
            internal readonly List<string> typeIds = new();
            internal readonly List<(GameElement, string)> externalIds = new();

            // Constructor
            public AssetImportReferenceContext(ScriptableProvider scriptable)
            {
                this.scriptable = scriptable;

                // Add main type
                typeIds.Add(null);
            }

            // Methods
            public void SetMainType(Type type)
            {
                // Get the type id
                typeIds[0] = scriptable.GetTypeId(type);
            }

            public override int GetTypeId(Type type)
            {
                // Get the type id
                string typeId = scriptable.GetTypeId(type);

                // Check for null
                if (typeId == null)
                    return -1;

                // Check for existing
                int index = typeIds.IndexOf(typeId);

                // Check for found
                if (index == -1)
                {
                    index = typeIds.Count;
                    typeIds.Add(typeId);
                }
                return index;
            }

            public override int GetExternalObjectId(GameElement instance, Type asType)
            {
                // Check for existing
                int externalIndex = externalIds.FindIndex(o => o.Item1 == instance);

                // Check for found
                if (externalIndex == -1)
                {
#warning This will need to be fixed
                    // Get external object
                    string externalId = instance.Name; //GetExternalObject(instance, asType);

                    externalIndex = externalIds.Count;
                    externalIds.Add((instance, externalId));
                }

                return externalIndex;
            }
        }

        // Private
        private Type mainType = null;
        private AssetImportReferenceContext referenceContext;

        // Public
        public string AssetPath;
        public string AssetName;
        public string AssetExtension;

        // Internal
        internal readonly AssetBuildContext assetsContext;

        // Properties
        public Type MainType
        {
            get => mainType;
            set
            {
                referenceContext.SetMainType(value);
                mainType = value;
            }
        }

        public IReadOnlyList<string> ReferencedExternalDependencies => referenceContext.externalIds.Select(e => e.Item2).ToArray();
        public IReadOnlyList<string> ReferencedTypeIds => referenceContext.typeIds;

        // Constructor
        internal AssetImportContext(AssetBuildContext context, string assetPath)
        {
            this.referenceContext = new(new ScriptableProvider());
            this.assetsContext = context;
            this.AssetPath = assetPath;
            this.AssetName = Path.GetFileNameWithoutExtension(assetPath);
            this.AssetExtension = Path.GetExtension(assetPath);
        }

        // Methods
        public int AddTypeReference(Type type)
        {
            return referenceContext.GetTypeId(type);
        }

        public int AddExternalReference(GameElement asset, Type asType)
        {
            return referenceContext.GetExternalObjectId(asset, asType);
        }
    }

    public abstract class AssetImporter
    {
        // Methods
        public abstract Task ImportAssetAsync(AssetImportContext context, Stream inputStream, CancellationToken cancellationToken);
        public abstract Task BuildAssetAsync(AssetImportContext context, Stream outputStream, CancellationToken cancellationToken);
    }
}
