using System.Text.Json;

namespace KoraGame.Assets
{
    [AssetImporter(".json")]
    [AssetReader(typeof(GameElement), true)]
    internal sealed class SerializedReader : IAssetImporter, IAssetReader, IReferenceContext<string>, IReferenceContext<int>
    {
        //// Type
        //private sealed class SerializedJsonReferenceContext : SerializedJson.SerializedReferenceContext
        //{
        //    // Private
        //    private readonly AssetReadContext context;

        //    // Constructor
        //    public SerializedJsonReferenceContext(AssetReadContext context)
        //    {
        //        this.context = context;
        //    }

        //    // Methods
        //    public override async Task<object> ResolveExternalObjectAsync(string id, Type asType)
        //    {
        //        // Load the dependency async
        //        return await context.LoadDependencyAsync(id, asType);
        //    }
        //}

        //private sealed class SerializedBinaryReferenceContext : SerializedBinary.SerializedReferenceContext
        //{
        //    // Private
        //    private readonly AssetReadContext context;

        //    // Constructor
        //    public SerializedBinaryReferenceContext(AssetReadContext context)
        //    {
        //        this.context = context;
        //    }

        //    // Methods
        //    public override Type ResolveTypeId(int typeId)
        //    {
        //        // Get the type name
        //        string typeName = context.ReferencedTypes[typeId];

        //        // Resolve the type
        //        return context.Scriptable.ResolveType(typeName);
        //    }

        //    public override async Task<object> ResolveExternalObjectAsync(int externalId, Type asType)
        //    {
                
        //        // Load the dependency async
        //        //return await context.LoadDependencyAsync(id, asType);
        //        return null;
        //    }
        //}

        // Private
        private AssetReadContext context;

        // Methods
        Type IReferenceContext<string>.ResolveType(string typeId)
        {
            // Resolve the type
            return context.Scriptable.ResolveType(typeId);
        }

        Type IReferenceContext<int>.ResolveType(int typeId)
        {
            // Get the type name
            string typeName = context.ReferencedTypes[typeId + 1];

            // Resolve the type
            return context.Scriptable.ResolveType(typeName);
        }

        string IReferenceContext<string>.GetTypeId(Type type)
        {
            // Resolve type id
            return context.Scriptable.GetTypeId(type);
        }

        int IReferenceContext<int>.GetTypeId(Type type)
        {
            if(context.ReferencedTypes != null)
            {
                // Get type name
                string typeName = context.Scriptable.GetTypeId(type);

                // Check for registered
                if (context.ReferencedTypes.Contains(typeName) == false)
                    context.ReferencedTypes.Add(typeName);

                // Get index
                return context.ReferencedTypes.IndexOf(typeName) + 1;
            }
            // No reference
            return 0;
        }

        async Task<object> IReferenceContext<string>.ResolveExternalReference(string id, Type asType)
        {
            GameElement result = await context.LoadDependencyAsync(id, asType);
            return result;
        }

        async Task<object> IReferenceContext<int>.ResolveExternalReference(int id, Type asType)
        {
            // Check for index
            if (id < 0 || id >= context.ReferencedTypes.Count)
                return null;

            // Get external path
            string externalPath = context.ReferencedExternalObjects[id];

            // Try to load
            GameElement result = await context.LoadDependencyAsync(externalPath, asType);
            return result;
        }

        public async Task<GameElement> ImportAsync(AssetReadContext context, Stream stream, CancellationToken cancellationToken)
        {
            // Check for type
            if (context.AssetType.IsAbstract == true)
                throw new InvalidOperationException("Serialized type must be explicit");

            this.context = context;

            // Read into memory
            using (MemoryStream memoryStream = new MemoryStream())
            {
                // Read bytes into memory 
                await stream.CopyToAsync(memoryStream);

                // Create json reader
                Utf8JsonReader reader = new Utf8JsonReader(new ReadOnlySpan<byte>(memoryStream.GetBuffer(), 0, (int)memoryStream.Length));
            
                try
                {
                    // Create context
                    SerializedReference<string> serializedContext = new(this);

                    // Deserialize the root object
                    object result = await SerializedJson.ReadRootObject(ref serializedContext, ref reader, context.AssetType);

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
            this.context = context;

            // Create reader
            BinaryReader reader = new BinaryReader(stream);

            try
            {
                // Create serialized context
                SerializedReference<int> serializedContext = new(this);

                // Get main type
                Type mainType = context.Scriptable.ResolveType(context.ReferencedTypes[0]);

                // Read object
                object result = await SerializedBinary.ReadRootObject(ref serializedContext, reader, mainType);

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
