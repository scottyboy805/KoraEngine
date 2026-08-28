using KoraGame;
using System.Text.Json;

namespace KoraPipeline.Importer
{
    [AssetImporterFor(".json")]
    internal sealed class SerializedImporter : AssetImporter
    {
        // Type
        private sealed class SerializedJsonReferenceContext : SerializedJson.SerializedReferenceContext
        {
            // Methods
            public override async Task<object> ResolveExternalObjectAsync(string id, Type asType)
            {
                return null;
                // Load the dependency async
                //return await context.LoadDependencyAsync(id, asType);
            }
        }

        private sealed class SerializedBinaryReferenceContext : SerializedBinary.SerializedReferenceContext
        {
        }

        // Private
        private GameElement serializedObject;

        // Methods
        public override async Task ImportAssetAsync(AssetImportContext context, Stream inputStream, CancellationToken cancellationToken)
        {
            // Create memory buffer
            MemoryStream memoryStream = new MemoryStream((int)inputStream.Length);

            // Copy to
            await inputStream.CopyToAsync(memoryStream);

            // Create json reader
            Utf8JsonReader reader = new Utf8JsonReader(new ReadOnlySpan<byte>(memoryStream.GetBuffer(), 0, (int)memoryStream.Length));
            
            // Create context
            SerializedJsonReferenceContext serializedContext = new();

            // Import the element
            serializedObject = await SerializedJson.ReadRootObject(serializedContext, ref reader, typeof(GameElement)) as GameElement;

            // Define main type and name
            context.MainType = serializedObject.elementType;
            context.AssetName = serializedObject.Name;
        }

        public override async Task BuildAssetAsync(AssetImportContext context, Stream outputStream, CancellationToken cancellationToken)
        {
            // Create writer
            BinaryWriter writer = new BinaryWriter(outputStream);

            // Create the context
            SerializedBinaryReferenceContext serializedContext = new();

            // Write the root object
            await SerializedBinary.WriteRootObject(serializedContext, writer, serializedObject.elementType, serializedObject);
        }
    }
}
