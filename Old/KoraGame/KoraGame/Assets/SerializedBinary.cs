using Assimp.Configs;
using System.Collections;

namespace KoraGame
{
    public static class SerializedBinary
    {
        // Type
        internal enum SerializedType : byte
        {
            Null = 0,

            Object,
            Array,

            LocalReference,
            ExternalReference,
        }

        internal class SerializedReferenceContext
        {
            // Private
            private List<object> localIds;
            private Dictionary<int, BindElement> localRefElements;
            private Dictionary<int, BindElement> externalRefElements;
            private Queue<IAssetSerialize> deserializeCallbacks;

            // Methods
            public virtual int GetTypeId(Type type)
            {
                return -1;
            }

            public virtual Type ResolveTypeId(int typeId)
            {
                return null;
            }

            public virtual int GetExternalObjectId(GameElement instance, Type asType)
            {
                return -1;
            }

            public virtual Task<object> ResolveExternalObjectAsync(int externalId, Type asType)
            {
                return Task.FromResult<object>(null);
            }

            internal bool GetLocalObject(object obj, out int localIndex)
            {
                // Create locals
                if (localIds == null)
                    localIds = new();

                // Check for existing
                int index = localIds.IndexOf(obj);

                // Check for found
                if (index == -1)
                {
                    localIndex = localIds.Count;
                    localIds.Add(obj);
                    return false;
                }
                else
                {
                    localIndex = index;
                    return true;
                }
            }

            public async Task<object> PerformLateBindingsAndCallbacksAsync(object instance)
            {
                //// Wait for completed
                //if (externalIdObjects != null)
                //    await Task.WhenAll(externalIdObjects.Values);

                return instance;
            }
        }

        // Methods
        #region Write
        internal static async Task WriteRootObject(SerializedReferenceContext context, BinaryWriter writer, Type type, object instance)
        {
            try
            {
                // Write the root object
                WriteObject(context, writer, type, instance, true);
            }
            catch(Exception e)
            {
                Debug.LogException(e);
            }
        }

        private static void WriteAny(SerializedReferenceContext context, BinaryWriter writer, Type type, object instance)
        {
            // Check for array
            if (IsArray(type) == true)
            {
                // Write as array
                WriteArray(context, writer, type, instance);
            }
            // Check for object
            else if (IsObject(type) == true)
            {
                // Write as object
                WriteObject(context, writer, type, instance, false);
            }            
            // Must be a property
            else
            {
                // Write the value
                WriteValue(context, writer, type, instance);
            }
        }

        private static void WriteObject(SerializedReferenceContext context, BinaryWriter writer, Type type, object instance, bool isRoot)
        {
            // Get the layout
            SerializedLayout layout = SerializedLayout.GetSerializeLayout(type);

            // Check for null or not serializable
            if (instance == null || layout == null)
            {
                WriteNull(writer);
                return;
            }

            // Check for local
            if(context.GetLocalObject(instance, out int localIndex) == true)
            {
                // Write local reference
                writer.Write((byte)SerializedType.LocalReference);

                // Write the local reference index
                writer.Write(localIndex);
                return;
            }
            // Check for external
            else if(isRoot == false && instance is GameElement ge && ge.IsAsset == true)
            {
                // Define the external object
                int externalIndex = context.GetExternalObjectId(ge, type);

                // Write external reference
                writer.Write((byte)SerializedType.ExternalReference);

                // Write the external reference index
                writer.Write(externalIndex);
                return;
            }

            // Write object header
            writer.Write((byte)SerializedType.Object);

            // Write reference index
            writer.Write(localIndex);

            // Write type
            if(IsTypeExplicit(type) == false)
                WriteType(context, writer, type);            

            // Process all serialize fields
            foreach(SerializedProperty element in layout.SerializeProperties)
            {
                // Get the value
                object value = element.GetValue(instance);

                // Write value
                WriteAny(context, writer, element.PropertyType, value);
            }
        }

        private static void WriteArray(SerializedReferenceContext context, BinaryWriter writer, Type type, object instance)
        {
            // Get element type
            Type elementType = type.IsArray == true
                ? type.GetElementType()
                : type.GetGenericArguments()[0];

            // Get array
            IList array = (IList)instance;

            // Write header
            writer.Write((byte)SerializedType.Array);

            // Write type info
            if (IsTypeExplicit(type) == false)
                WriteType(context, writer, type);

            // Write length
            writer.Write(array.Count);

            // Write all elements
            for(int i = 0; i < array.Count; i++)
            {
                // Get value
                object value = array[i];

                // Get type
                Type explicitElementType = value != null
                    ? value.GetType()
                    : elementType;

                // Write the value
                WriteAny(context, writer, explicitElementType, value);
            }
        }

        private static void WriteValue(SerializedReferenceContext context, BinaryWriter writer, Type type, object instance)
        {
            // Check enum
            if(type.IsEnum == true)
            {
                // Get enum type
                Type enumType = type.GetEnumUnderlyingType();

                // Convert to underlying type
                WriteValue(context, writer, enumType, Convert.ChangeType(instance, enumType));
                return;
            }

            // Check kind
            switch(instance)
            {
                case bool boolValue: writer.Write(boolValue); break;
                case char charValue: writer.Write(charValue); break;
                case sbyte sbyteValue: writer.Write(sbyteValue); break;
                case byte byteValue: writer.Write(byteValue); break;
                case short shortValue: writer.Write(shortValue); break;
                case ushort ushortValue: writer.Write(ushortValue); break;
                case int intValue: writer.Write(intValue); break;
                case uint uintValue: writer.Write(uintValue); break;
                case long longValue: writer.Write(longValue); break;
                case ulong ulongValue: writer.Write(ulongValue); break;
                case float floatValue: writer.Write(floatValue); break;
                case double doubleValue: writer.Write(doubleValue); break;
                case decimal decimalValue: writer.Write(decimalValue); break;
                case string stringValue: writer.Write(stringValue); break;
                case Type typeValue: WriteType(context, writer, typeValue); break;

                default: throw new NotSupportedException(type.ToString() + ": " + instance);
            }
        }

        private static void WriteNull(BinaryWriter writer)
        {
            writer.Write((byte)SerializedType.Null);
        }

        private static void WriteType(SerializedReferenceContext context, BinaryWriter writer, Type type)
        {
            // Define the type
            int typeIndex = context.GetTypeId(type);

            // Write the type index
            writer.Write(typeIndex);
        }
        #endregion

        #region Read
        internal static Task<object> ReadRootObject(SerializedReferenceContext context, BinaryReader reader, Type type)
        {
            try
            {
                // Try to read
                object obj = ReadObject(context, reader, type, null);

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

        private static object ReadAny(SerializedReferenceContext context, BinaryReader reader, Type type, BindElement parent)
        {
            // Check for array
            if (IsArray(type) == true)
            {
                // Write as array
                return ReadArray(context, reader, type, parent);
            }
            // Check for object
            else if (IsObject(type) == true)
            {
                // Write as object
                return ReadObject(context, reader, type, parent);
            }
            // Must be a property
            else
            {
                // Write the value
                return ReadValue(context, reader, type);
            }
        }

        private static object ReadObject(SerializedReferenceContext context, BinaryReader reader, Type type, BindElement parent)
        {
            // Read type
            SerializedType serializedType = (SerializedType)reader.ReadByte();

            switch(serializedType)
            {
                case SerializedType.Null:
                    {
                        // No object stored
                        return null;
                    }
                case SerializedType.LocalReference:
                    {
                        // Read local id
                        int localId = reader.ReadInt32();

                        // parent.
                        return null;
                    }
                case SerializedType.ExternalReference:
                    {
                        int externalId = reader.ReadInt32();
                        
                        return null;
                    }
                case SerializedType.Object:
                    {
                        // Read the object index
                        int objectId = reader.ReadInt32();

                        // Read the type if it is available
                        Type objectType = IsTypeExplicit(type) == false
                            ? ReadType(context, reader)
                            : type;

                        // Create instance
                        object instance = Activator.CreateInstance(objectType, true);                                               

                        // Get the layout
                        SerializedLayout layout = SerializedLayout.GetSerializeLayout(objectType);

                        // Process all serialize fields
                        foreach (SerializedProperty element in layout.SerializeProperties)
                        {
                            // Create bind
                            BindElement bind = new(instance, element, parent);

                            // Read the object
                            object value = ReadAny(context, reader, element.PropertyType, bind);

                            // Set the value
                            element.SetValue(instance, value);
                        }

                        // Get the instance
                        return instance;
                    }

                default: throw new FormatException("Unexpected serialized type: " + serializedType);
            }
        }

        private static object ReadArray(SerializedReferenceContext context, BinaryReader reader, Type type, BindElement parent)
        {
            // Read type
            SerializedType serializedType = (SerializedType)reader.ReadByte();

            switch(serializedType)
            {
                case SerializedType.Null:
                    {
                        // No array stored
                        return null;
                    }
                case SerializedType.Array:
                    {
                        // Get element type
                        Type elementType = type.IsArray == true
                            ? type.GetElementType()
                            : type.GetGenericArguments()[0];

                        // Read type info
                        Type arrayType = IsTypeExplicit(type) == false
                            ? ReadType(context, reader)
                            : type;

                        // Read length
                        int length = reader.ReadInt32();

                        // Create instance
                        IList array = arrayType.IsArray == true
                            ? Array.CreateInstance(elementType, length)
                            : (IList)Activator.CreateInstance(arrayType);

                        // Read all elements
                        for (int i = 0; i < length; i++)
                        {
                            // Read element
                            object value = ReadAny(context, reader, elementType, parent);

                            // Set value
                            if (array is Array)
                            {
                                array[i] = value;
                            }
                            else
                            {
                                array.Add(value);
                            }
                        }

                        // Get array
                        return array;
                    }

                default: throw new FormatException("Unexpected serialized type: " + serializedType);
            }
        }

        private static object ReadValue(SerializedReferenceContext context, BinaryReader reader, Type type)
        {
            // Check enum
            if (type.IsEnum == true)
            {
                // Get enum type
                Type enumType = type.GetEnumUnderlyingType();

                // Convert to underlying type
                return ReadValue(context, reader, enumType);
            }

            // Check for type
            if (type == typeof(Type))
                return ReadType(context, reader);

            // Get type code
            TypeCode typeCode = Type.GetTypeCode(type);

            // Check kind
            switch (typeCode)
            {
                case TypeCode.Boolean: return reader.ReadBoolean();
                case TypeCode.Char: return reader.ReadChar();
                case TypeCode.SByte: return reader.ReadSByte();
                case TypeCode.Byte: return reader.ReadSByte();
                case TypeCode.Int16: return reader.ReadInt16();
                case TypeCode.UInt16: return reader.ReadUInt16();
                case TypeCode.Int32: return reader.ReadInt32();
                case TypeCode.UInt32: return reader.ReadUInt32();
                case TypeCode.Int64: return reader.ReadInt64();
                case TypeCode.UInt64: return reader.ReadUInt64();
                case TypeCode.Single: return reader.ReadSingle();
                case TypeCode.Double: return reader.ReadDouble();
                case TypeCode.Decimal: return reader.ReadDecimal();
                case TypeCode.String: return reader.ReadString();

                default: throw new NotSupportedException(type.ToString());
            }
        }

        private static Type ReadType(SerializedReferenceContext context, BinaryReader reader)
        {
            // Read type index
            int typeIndex = reader.ReadInt32();

            // Try to resolve type
            return context.ResolveTypeId(typeIndex);
        }
        #endregion

        private static bool IsObject(Type type)
        {
            return type.IsPrimitive == false && type != typeof(string) && type.IsArray == false && type.IsEnum == false;
        }

        private static bool IsArray(Type type)
        {
            return type.IsArray == true || (type.IsGenericType == true && type.GetGenericTypeDefinition() == typeof(List<>));
        }

        private static bool IsTypeExplicit(Type type)
        {
            // Non- explicit types need to be serialized manually
            return type.IsInterface == false
                && type.IsAbstract == false;
        }
    }
}
