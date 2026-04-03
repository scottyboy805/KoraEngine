using System.Collections;
using System.Text;
using System.Text.Json;

namespace KoraGame
{
    public static class SerializedJson
    {
        // Type
        private enum SerializeRefType
        {
            None = 0,
            LocalRef,
            ExternRef,
        }

        internal class SerializedReferenceContext
        {
            // Private
            private Dictionary<string, object> fileIdObjects;
            private Dictionary<string, Task<object>> externalIdObjects;
            private Dictionary<string, BindElement> localRefElements;
            private Dictionary<string, BindElement> externalRefElements;
            private Queue<IAssetSerialize> deserializeCallbacks;

            // Methods
            public virtual Type ResolveType(string typeId)
            {
                return Type.GetType(typeId, false);
            }

            public virtual string GetTypeId(Type type)
            {
                return type.AssemblyQualifiedName;
            }

            public virtual Task<object> ResolveExternalObjectAsync(string id, Type asType)
            {
                return Task.FromResult<object>(null);
            }

            public void DefineLocalReference(string localRefId, object localInstance)
            {
                // Create local
                if (fileIdObjects == null)
                    fileIdObjects = new();

                // Add reference
                fileIdObjects.Add(localRefId, localInstance);
            }

            public void AddLocalReference(string localRefId, BindElement binding)
            {
                // Create binding
                if (localRefElements == null)
                    localRefElements = new();

                // Add binding
                localRefElements.Add(localRefId, binding);
            }

            public void AddExternalReference(string externRefId, BindElement binding, Type asType)
            {
                // Create binding
                if (externalRefElements == null)
                    externalRefElements = new();

                // Create external objects
                if(externalIdObjects == null) 
                    externalIdObjects = new();

                // Add binding
                externalRefElements.Add(externRefId, binding);

                // Add external object request
                externalIdObjects.Add(externRefId, ResolveExternalObjectAsync(externRefId, asType));
            }

            public void AddDeserializationCallback(object instance)
            {
                if(instance is IAssetSerialize serialize)
                {
                    // Create collection
                    if (deserializeCallbacks == null)
                        deserializeCallbacks = new();

                    // Push callback
                    deserializeCallbacks.Enqueue(serialize);
                }
            }

            public async Task<object> PerformLateBindingsAndCallbacksAsync(object instance)
            {
                // Wait for completed
                if(externalIdObjects != null)
                    await Task.WhenAll(externalIdObjects.Values);

                // Bind local file ids
                if (localRefElements != null && fileIdObjects != null)
                {
                    foreach (var fileId in localRefElements)
                    {
                        // Lookup the object
                        if (fileIdObjects.TryGetValue(fileId.Key, out object value) == true)
                            fileId.Value.Bind(value);
                    }
                }

                // Bind external asset paths
                if (externalRefElements != null && externalIdObjects != null)
                {
                    foreach (var externalId in externalRefElements)
                    {
                        // Lookup the object
                        if (externalIdObjects.TryGetValue(externalId.Key, out Task<object> elementTask) == true && elementTask.IsCompletedSuccessfully == true)
                            externalId.Value.Bind(elementTask.Result);
                    }
                }

                // Do callbacks
                if (deserializeCallbacks != null)
                {
                    foreach (IAssetSerialize serialize in deserializeCallbacks)
                    {
                        try
                        {
                            serialize.OnDeserialize();
                        }
                        catch (Exception e)
                        {
                            Debug.LogException(e);
                        }
                    }
                }

                // Get the instance
                return instance;
            }
        }

        // Private
        private const string idDiscriminator = "$id";
        private const string typeDiscriminator = "$type";
        private const string localReferenceDiscriminator = "$refid";
        private const string externalReferenceDiscriminator = "$ref";

        // Methods
        public static async Task<T> DeserializeAsync<T>(string json)
        {
            // Get bytes
            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

            // Create reader
            Utf8JsonReader reader = new Utf8JsonReader(jsonBytes);

            try
            {
                // Read async
                return (T)await ReadRootObject(null, ref reader, typeof(T), null);
            }
            catch(InvalidCastException)
            {
                return default;
            }
        }

        public static async Task PopulateAsync<T>(string json, T instance) where T : class
        {
            // Get bytes
            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

            // Create reader
            Utf8JsonReader reader = new Utf8JsonReader(jsonBytes);

            // Read async
            _ = await ReadRootObject(null, ref reader, typeof(T), instance);
        }

        #region Read
        internal static Task<object> ReadRootObject(SerializedReferenceContext context, ref Utf8JsonReader reader, Type type, object existingInstance = null)
        {
            // Read first token
            if (reader.Read() == false)
                return null;

            try
            {                
                // Try to read
                object obj = ReadObject(context, ref reader, type, existingInstance, null);

                // Wait for late binding
                return context != null
                    ? context.PerformLateBindingsAndCallbacksAsync(obj)
                    : Task.FromResult(obj);
            }
            catch(Exception e)
            {
                Debug.LogException(e);
                return Task.FromResult<object>(null);
            }
        }

        private static bool ReadAny(SerializedReferenceContext context, ref Utf8JsonReader reader, Type type, object instance, BindElement parent, out object value)
        {
            value = null;

            // Try to read
            if (reader.Read() == false)
                return false;

            // Check for terminating
            if (reader.TokenType == JsonTokenType.EndObject || reader.TokenType == JsonTokenType.EndArray)
                return false;

            // Check for object
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                // Read as object
                value = ReadObject(context, ref reader, type, instance, parent);
            }
            // Check for array
            else if (reader.TokenType == JsonTokenType.StartArray)
            {
                // Read as array
                value = ReadArray(context, ref reader, type, instance, parent);
            }
            // Handle any other value as a property
            else
            {
                // Read as property
                value = ReadProperty(ref reader, type);
            }
            return true;
        }

        private static object ReadObject(SerializedReferenceContext context, ref Utf8JsonReader reader, Type type, object instance, BindElement parent)
        {
            // Expect object start
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new FormatException("Expected object start but got: " + reader.TokenType);

            // Read id and type info
            ReadIdTypeInfo(ref reader, out string fileId, out string typeId);

            // Check for create instance
            if (instance == null)
            {
                // Check for type provided
                if (string.IsNullOrEmpty(typeId) == false)
                {
                    // Try to resolve explicit id
                    type = context.ResolveType(typeId);
                }

                // Try to create fallback instance
                if (type != null && SerializedLayout.IsTypeSerializable(type) == true)
                {
                    // Create instance of normal type
                    instance = Activator.CreateInstance(type, true);

                    // Register the instance if a file id was specified
                    if (string.IsNullOrEmpty(fileId) == false)
                        context?.DefineLocalReference(fileId, instance);
                }
                else
                {
                    ReadSkipObject(ref reader);
                    return instance;
                }
            }

            // Get the layout
            SerializedLayout layout = SerializedLayout.GetSerializeLayout(type);

            // Read until end
            while (reader.Read() == true && reader.TokenType != JsonTokenType.EndObject)
            {
                // Read property name
                if (reader.TokenType != JsonTokenType.PropertyName)
                    throw new FormatException("Expected property name but got: " + reader.TokenType);

                // Read property name
                string propertyName = reader.GetString();

                // Get the layout element
                SerializedProperty element = layout?[propertyName];

                // Check for found
                if (element != null)
                {
                    // Create out binding
                    BindElement bind = new BindElement(instance, element, parent);

                    // Check for reference element
                    // If true, the elements will be added to the lookup tables and data will be bound later
                    SerializeRefType refType = ReadReferenceInfo(ref reader, element.PropertyType, out string refId);

                    // Check for local reference
                    if (refType == SerializeRefType.LocalRef)
                    {
                        // Add reference to local object
                        context?.AddLocalReference(refId, bind);
                    }
                    // Check for external reference
                    else if (refType == SerializeRefType.ExternRef)
                    {
                        // Add reference to external object
                        context?.AddExternalReference(refId, bind, element.PropertyType);
                    }
                    // Just deserialize normally
                    else
                    {
                        // Try to read any
                        bool didRead = ReadAny(context, ref reader, element.PropertyType, null, bind, out object value);

                        // Check for any value
                        if (didRead == true && value != null)
                        {
                            // Update the value
                            element.SetValue(instance, value);
                        }
                    }
                }
                else
                {
                    // Skip to end of the object
                    ReadSkipAny(ref reader);
                    return instance;
                }
            }

            // Expect until object end
            if (reader.TokenType != JsonTokenType.EndObject)
                ReadSkipObject(ref reader);

            // Try to push deserialize callback
            context?.AddDeserializationCallback(instance);

            return instance;
        }

        private static object ReadArray(SerializedReferenceContext context, ref Utf8JsonReader reader, Type type, object instance, BindElement parent)
        {
            // Expect array start
            if (reader.TokenType != JsonTokenType.StartArray)
                throw new FormatException("Expected array start but got: " + reader.TokenType);

            Type elementType = null;

            // Check for create instance
            if (instance == null)
            {
                // Try to create fallback instance
                if (type != null)
                {
                    // Create instance of normal type
                    instance = Activator.CreateInstance(type);

                    // Check for array
                    if (type.IsArray == true)
                    {
                        elementType = type.GetElementType();
                    }
                    // Check for generic list
                    else if (type.IsGenericType == true && type.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        elementType = type.GetGenericArguments()[0];
                    }
                }
                else
                {
                    ReadSkipArray(ref reader);
                    return instance;
                }
            }

            // Check element type
            if (elementType == null)
                throw new InvalidOperationException("Could not determine element type: " + type);

            // Read until end
            while (reader.TokenType != JsonTokenType.EndArray)
            {
                // Try to read any
                bool didRead = ReadAny(context, ref reader, elementType, null, parent, out object value);

                // Check for any value
                if (didRead == true)
                {
                    // Update the value
                    if (instance is Array)
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        // Add to list
                        ((IList)instance).Add(value);
                    }
                }
            }

            return instance;
        }

        private static object ReadProperty(ref Utf8JsonReader reader, Type type)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String: return reader.GetString();
                case JsonTokenType.Number:
                    {
                        switch (Type.GetTypeCode(type))
                        {
                            case TypeCode.SByte: return reader.GetSByte();
                            case TypeCode.Byte: return reader.GetByte();
                            case TypeCode.Int16: return reader.GetInt16();
                            case TypeCode.UInt16: return reader.GetUInt16();
                            case TypeCode.Int32: return reader.GetInt32();
                            case TypeCode.UInt32: return reader.GetUInt32();
                            case TypeCode.Int64: return reader.GetInt64();
                            case TypeCode.UInt64: return reader.GetUInt64();
                            case TypeCode.Single: return reader.GetSingle();
                            case TypeCode.Double: return reader.GetDouble();
                            default: throw new NotSupportedException(type.ToString());
                        }
                    }
                case JsonTokenType.True:
                case JsonTokenType.False: return reader.GetBoolean();
                case JsonTokenType.Null: return null;
                default: throw new FormatException("Expected property value but got: " + reader.TokenType);
            }
        }

        private static void ReadSkipAny(ref Utf8JsonReader reader)
        {
            // Try to read
            if (reader.Read() == false)
                return;

            // Check for object
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                // Skip the object
                ReadSkipObject(ref reader);
            }
            // Check for array
            else if (reader.TokenType == JsonTokenType.StartArray)
            {
                // Skip the array
                ReadSkipArray(ref reader);
            }
            // Must be a property
            else
            {
                // Skip the property value
                reader.Read();
            }
        }

        private static void ReadSkipObject(ref Utf8JsonReader reader)
        {
            Utf8JsonReader peekReader = reader;

            // We cannot deserialize this object - simply skip the data
            int depth = 1;
            while (depth > 0 && peekReader.Read() == true)
            {
                if (peekReader.TokenType == JsonTokenType.StartObject) depth++;
                if (peekReader.TokenType == JsonTokenType.EndObject) depth--;

                if (depth > 0)
                    reader = peekReader;
            }
        }

        private static void ReadSkipArray(ref Utf8JsonReader reader)
        {
            // We cannot deserialize this array - simply skip the data
            int depth = 1;
            while (depth > 0 && reader.Read() == true)
            {
                if (reader.TokenType == JsonTokenType.StartArray) depth++;
                if (reader.TokenType == JsonTokenType.EndArray) depth--;
            }
        }

        private static void ReadIdTypeInfo(ref Utf8JsonReader reader, out string fileId, out string typeId)
        {
            // Set defaults
            fileId = null;
            typeId = null;

            // Don't modify the existing state
            Utf8JsonReader noModifyReader = reader;

            while (true)
            {
                // Try to read
                if (noModifyReader.Read() == false)
                    return;

                // Check for property
                if (noModifyReader.TokenType == JsonTokenType.PropertyName)
                {
                    // Get the string
                    string value = noModifyReader.GetString();

                    switch (value)
                    {
                        case idDiscriminator:
                            {
                                // Read id value
                                noModifyReader.Read();
                                fileId = noModifyReader.GetString();

                                // Update reader
                                reader = noModifyReader;
                                break;
                            }
                        case typeDiscriminator:
                            {
                                // Read value
                                noModifyReader.Read();
                                typeId = noModifyReader.GetString();

                                // Update reader
                                reader = noModifyReader;
                                break;
                            }
                        // Exit because we did not match any value
                        default: return;
                    }
                }
            }
        }

        private static SerializeRefType ReadReferenceInfo(ref Utf8JsonReader reader, Type asType, out string refId)
        {
            refId = null;

            // Don't modify the existing state
            Utf8JsonReader noModifyReader = reader;

            // Check for object start
            if (noModifyReader.Read() == false || noModifyReader.TokenType != JsonTokenType.StartObject)
                return SerializeRefType.None;

            while (true)
            {
                // Try to read
                if (noModifyReader.Read() == false)
                    return SerializeRefType.None;

                // Check for property
                if (noModifyReader.TokenType == JsonTokenType.PropertyName)
                {
                    // Get the string
                    string value = noModifyReader.GetString();

                    switch (value)
                    {
                        case localReferenceDiscriminator:
                            {
                                // Read id value
                                noModifyReader.Read();
                                refId = noModifyReader.GetString();

                                // Read until object end, and then consume the end token
                                ReadSkipObject(ref noModifyReader);
                                noModifyReader.Read();

                                // Update reader
                                reader = noModifyReader;
                                return SerializeRefType.LocalRef;
                            }
                        case externalReferenceDiscriminator:
                            {
                                // Read value
                                noModifyReader.Read();
                                refId = noModifyReader.GetString();

                                // Read until object end, and then consume the end token
                                ReadSkipObject(ref noModifyReader);
                                noModifyReader.Read();

                                // Update reader
                                reader = noModifyReader;
                                return SerializeRefType.ExternRef;
                            }
                        // Exit because we did not match any value
                        default: return SerializeRefType.None;
                    }
                }
            }
        }
        #endregion
    }
}
