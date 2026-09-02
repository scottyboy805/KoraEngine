
namespace KoraGame
{
    internal enum SerializedRefType
    {
        None = 0,
        Local,
        External,
    }


    internal interface IReferenceContext<T>
    {
        // Methods
        Type ResolveType(T typeId);

        T GetTypeId(Type type);

        Task<object> ResolveExternalReference(T id, Type asType);
    }

    internal struct SerializedReference<T>
    {
        // Type
        private struct SerializedKey
        {
            // Public
            public SerializedRefType Scope;
            public T Identifier;

            // Methods
            public readonly override int GetHashCode()
            {
                return HashCode.Combine(Scope, Identifier);
            }
        }

        private struct SerializedInstance
        {
            // Public
            public object Instance;
            public Type Type;
        }

        // Privae
        private IReferenceContext<T> reference;
        private Dictionary<SerializedKey, SerializedInstance> referenceObjects;
        private Dictionary<SerializedKey, BindElement> bindElements;
        private Queue<IAssetSerialize> serializeCallbacks;

        // Constructor
        public SerializedReference(IReferenceContext<T> reference)
        {
            this.reference = reference;
        }

        // Methods
        public readonly Type ResolveType(T typeId)
        {
            // Check for none
            if (typeId == null || reference == null)
                return null;

            // Try to use scriptable
            return reference.ResolveType(typeId);
        }

        public readonly T GetTypeId(Type type)
        {
            // Check for null
            if (type == null || reference == null)
                return default;

            // Try to use reference provider
            return reference.GetTypeId(type);
        }

        public readonly T GetLocalId(object instance, Type asType)
        {
            // Check for none
            if (instance == null || asType == null)
                return default;

            // Try to find extern key
            KeyValuePair<SerializedKey, SerializedInstance> externId = referenceObjects
                .FirstOrDefault(o => o.Key.Scope == SerializedRefType.Local && o.Value.Instance == instance && asType.IsAssignableFrom(o.Value.Type));

            // Get the key id
            return externId.Key.Identifier;
        }

        public readonly T GetExternalId(object instance, Type asType)
        {
            // Check for none
            if (instance == null || asType == null)
                return default;

            // Try to find extern key
            KeyValuePair<SerializedKey, SerializedInstance> externId = referenceObjects
                .FirstOrDefault(o => o.Key.Scope == SerializedRefType.External && o.Value.Instance == instance && asType.IsAssignableFrom(o.Value.Type));

            // Get the key id
            return externId.Key.Identifier;
        }

        public void RegisterLocalObject(T localIdentifier, object localObject, Type localType)
        {
            // Check for null
            if (localObject == null)
                return;

            // Create the key
            SerializedKey key = new SerializedKey
            {
                Scope = SerializedRefType.Local,
                Identifier = localIdentifier,
            };

            // Add local
            if (referenceObjects == null)
                referenceObjects = new();

            // Add object
            referenceObjects[key] = new SerializedInstance
            {
                Instance = localObject,
                Type = localType,
            };
        }

        public void RegisterExternalObject(T externalIdentifier, object externalObject, Type externalType)
        {
            // Check for null
            if (externalObject == null)
                return;

            // Create the key
            SerializedKey key = new SerializedKey
            {
                Scope = SerializedRefType.Local,
                Identifier = externalIdentifier,
            };

            // Add local
            if (referenceObjects == null)
                referenceObjects = new();

            // Add object
            referenceObjects[key] = new SerializedInstance
            {
                Instance = externalObject,
                Type = externalType,                
            };
        }

        public void RegisterLocalReference(T localIdentifier, BindElement binding)
        {
            // Check for null
            if (binding == null)
                return;

            // Create the key
            SerializedKey key = new SerializedKey
            {
                Scope = SerializedRefType.Local,
                Identifier = localIdentifier,
            };

            // Add reference
            if (bindElements == null)
                bindElements = new();

            // Add reference
            bindElements.Add(key, binding);
        }

        public void RegisterExternalReference(T externalIdentifier, BindElement binding, Type asType)
        {
            // Check for null
            if (binding == null)
                return;

            // Create the key
            SerializedKey key = new SerializedKey
            {
                Scope = SerializedRefType.External,
                Identifier = externalIdentifier,
            };

            // Add reference
            if (bindElements == null)
                bindElements = new();

            // Add reference
            bindElements.Add(key, binding);

            // Resolve external
            if(reference != null)
            {
                // Try to resolve external
                Task<object> external = reference.ResolveExternalReference(externalIdentifier, asType);

                // Add external
                if (referenceObjects == null)
                    referenceObjects = new();

                // Add object
                referenceObjects[key] = new SerializedInstance
                {
                    Instance = external,
                    Type = asType,
                };
            }
        }

        public void RegisterSerializeCallbacks(object instance)
        {
            if(instance is IAssetSerialize serialize)
            {
                // Create callbacks
                if (serializeCallbacks == null)
                    serializeCallbacks = new();

                // Register callbacks
                serializeCallbacks.Enqueue(serialize);
            }
        }

        public readonly async Task<object> PerformLateBindingAndDeserializeCallbacks(object instance)
        {
            // Do late binding
            await PerformLateBindingAsync();

            // Do callbacks
            PerformDeserializeCallbacks();

            // Get the instance
            return instance;
        }

        public readonly async Task PerformLateBindingAsync()
        {
            // Check for null
            if (referenceObjects == null || bindElements == null)
                return;

            // Wait for all extenral load
            await Task.WhenAll(referenceObjects.Values
                .Where(v => v.Instance is Task<object>)
                .Select(v => (Task<object>)v.Instance));

            // Bind all elements
            foreach(var reference in bindElements)
            {
                // Get the key
                referenceObjects.TryGetValue(reference.Key, out SerializedInstance value);

                // Check for task
                if(value.Instance is Task<object> task)
                    value.Instance = task.Result;

                // Bind the result
                reference.Value.Bind(value.Instance);
            }
        }

        public readonly void PerformSerializeCallbacks()
        {
            // Check for null
            if (serializeCallbacks == null)
                return;

            // Do all callbacks
            foreach(var serialize in serializeCallbacks)
            {
                try
                {
                    serialize.OnSerialize();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        public readonly void PerformDeserializeCallbacks()
        {
            // Check for null
            if (serializeCallbacks == null)
                return;

            // Do all callbacks
            foreach (var serialize in serializeCallbacks)
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
    }
}
