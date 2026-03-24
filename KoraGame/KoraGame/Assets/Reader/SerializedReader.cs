using System.Text.Json;

namespace KoraGame.Assets
{
    [AssetImporter(".json")]
    [AssetReader(typeof(GameElement), true)]
    internal sealed class SerializedReader : IAssetImporter, IAssetReader
    {
        // Type
        private sealed class SerializedJsonReferenceContext : SerializedJson.SerializedReferenceContext
        {
            // Private
            private readonly AssetReadContext context;

            // Constructor
            public SerializedJsonReferenceContext(AssetReadContext context)
            {
                this.context = context;
            }

            // Methods
            public override async Task<object> ResolveExternalObjectAsync(string id, Type asType)
            {
                // Load the dependency async
                return await context.LoadDependencyAsync(id, asType);
            }
        }

        private sealed class SerializedBinaryReferenceContext : SerializedBinary.SerializedReferenceContext
        {
            // Private
            private readonly AssetReadContext context;

            // Constructor
            public SerializedBinaryReferenceContext(AssetReadContext context)
            {
                this.context = context;
            }

            // Methods
            public override Type ResolveTypeId(int typeId)
            {
                // Get the type name
                string typeName = context.ReferencedTypes[typeId];

                // Resolve the type
                return context.Scriptable.ResolveType(typeName);
            }

            public override async Task<object> ResolveExternalObjectAsync(int externalId, Type asType)
            {
                
                // Load the dependency async
                //return await context.LoadDependencyAsync(id, asType);
                return null;
            }
        }

        // Methods
        public async Task<GameElement> ImportAsync(AssetReadContext context, Stream stream, CancellationToken cancellationToken)
        {
            // Check for type
            if (context.AssetType.IsAbstract == true)
                throw new InvalidOperationException("Serialized type must be explicit");

            // Read into memory
            using (MemoryStream memoryStream = new MemoryStream())
            {
                // Read bytes into memory 
                await stream.CopyToAsync(memoryStream);

                // Create json reader
                Utf8JsonReader reader = new Utf8JsonReader(new ReadOnlySpan<byte>(memoryStream.GetBuffer(), 0, (int)memoryStream.Length));
            
                try
                {
                    // Create serialized context
                    SerializedJsonReferenceContext serializedContext = new (context);

                    // Deserialize the root object
                    object result = await SerializedJson.ReadRootObject(serializedContext, ref reader, context.AssetType);

                    // Call event
                    if(result is ScriptableAsset asset)
                    {
                        try
                        {
                            // Trigger loaded event
                            asset.DoLoaded();
                        }
                        catch (Exception ex)
                        {
                            Debug.LogException(ex);
                        }
                    }

                    // Get the loaded element
                    return result as GameElement;
                }
                catch(Exception e)
                {
                    Debug.LogException(e);
                    return null;
                }
            }
        }

        public async Task<GameElement> ReadAsync(AssetReadContext context, Stream stream, CancellationToken cancellationToken)
        {
            // Create reader
            BinaryReader reader = new BinaryReader(stream);

            try
            {
                // Create serialized context
                SerializedBinaryReferenceContext serializedContext = new(context);

                // Get main type - main type is always id 0
                Type mainType = serializedContext.ResolveTypeId(0);

                // Read object
                object result = await SerializedBinary.ReadRootObject(serializedContext, reader, mainType);

                // Get the loaded element
                return result as GameElement;
            }
            catch(Exception e)
            {
                Debug.LogException(e);
                return null;
            }
        }
    }
}
