using System.Collections;
using System.Reflection;
using System.Runtime.Serialization;

namespace KoraGame
{
    internal sealed class BindElement
    {
        // Public
        public readonly object Instance;
        public readonly SerializedProperty Property;
        public readonly BindElement Parent;

        // Constructor
        public BindElement(object instance, SerializedProperty property, BindElement parent = null)
        {
            this.Instance = instance;
            this.Property = property;
            this.Parent = parent;
        }

        // Methods
        public void Bind(object value)
        {
            // Set the value for this property
            Property.SetValue(Instance, value);

            // If we have a parent, trigger parent rebuild to propagate struct changes
            if (Parent != null && Instance is ValueType)
            {
                // Bind this instance to the parent value
                Parent.Bind(Instance);
            }
        }
    }

    internal sealed class SerializedLayout
    {
        // Private
        private static readonly Dictionary<Type, SerializedLayout> serializedTypeLayouts = new();

        public readonly Type SerializeType;
        public readonly List<SerializedProperty> SerializeProperties = new();

        // Properties
        public SerializedProperty this[string name] => SerializeProperties.FirstOrDefault(e => e.PropertyName == name);

        // Constructor
        private SerializedLayout(Type fromType, IEnumerable<SerializedProperty> serializedElements)
        {
            this.SerializeType = fromType;
            this.SerializeProperties.AddRange(serializedElements);
        }

        // Methods
        public static SerializedLayout GetSerializeLayout(Type forType)
        {
            // Check for cached
            if (serializedTypeLayouts.TryGetValue(forType, out SerializedLayout layout) == true)
                return layout;

            // Check for serializable
            if (IsTypeSerializable(forType) == false)
                return null;

            // Create new
            layout = new(forType, GetSerializableElements(forType));

            // Register layout
            serializedTypeLayouts[forType] = layout;
            return layout;
        }

        public static bool IsTypeSerializable(Type type)
        {
            // Ignore abstract and interfaces
            if (type.IsAbstract == true || type.IsInterface == true)
                return false;

            // Check for serializable
            if (typeof(GameElement).IsAssignableFrom(type) == true ||
                type.GetCustomAttribute<SerializableAttribute>() != null ||
                type.GetCustomAttribute<DataContractAttribute>() != null)
            {
                return true;
            }
            return false;
        }

        public static bool IsFieldSerializable(FieldInfo field)
        {
            // Check ignored
            if (field.IsDefined(typeof(IgnoreDataMemberAttribute), false) == true)
                return false;

            // Check for attribute
            return field.IsPublic == true || field.IsDefined(typeof(DataMemberAttribute), false) == true;
        }

        public static bool IsPropertySerializable(PropertyInfo property)
        {
            // Check ignored
            if (property.IsDefined(typeof(IgnoreDataMemberAttribute), false) == true)
                return false;

            // Require getter, setter and attribute
            return property.GetMethod != null && property.SetMethod != null
                && property.IsDefined(typeof(DataMemberAttribute), false) == true;
        }

        private static IEnumerable<SerializedProperty> GetSerializableElements(Type type)
        {
            // Search the base type first
            if(type.BaseType != null && type.BaseType != typeof(object) && type.BaseType != typeof(ValueType))
            {
                // Get base elements first
                foreach(SerializedProperty element in GetSerializableElements(type.BaseType))
                    yield return element;
            }

            // Check fields
            foreach(FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                // Check for serializable
                if (IsFieldSerializable(field) == false)
                    continue;

                // Create field element
                yield return new SerializedProperty.SerializedFieldMember(field);
            }

            // Check properties
            foreach(PropertyInfo property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                // Check for serializable
                if(IsPropertySerializable(property) == false)
                    continue;

                // Create property element
                yield return new SerializedProperty.SerializedPropertyMember(property);
            }
        }
    }

    internal abstract class SerializedProperty
    {
        // Type
        internal sealed class SerializedFieldMember : SerializedProperty
        {
            // Private
            private readonly FieldInfo serializeField;

            // Constructor
            public SerializedFieldMember(FieldInfo serializeField)
                : base(GetSerializeMemberName(serializeField), serializeField.FieldType)
            {
                this.serializeField = serializeField;
            }

            // Methods
            public override object GetValue(object instance)
            {
                return serializeField.GetValue(instance);
            }

            public override void SetValue(object instance, object value)
            {
                serializeField.SetValue(instance, value);
            }

            public override T GetAttribute<T>()
            {
                return serializeField.GetCustomAttribute<T>();
            }
        }
        internal sealed class SerializedPropertyMember : SerializedProperty
        {
            // Private
            private readonly PropertyInfo serializeProperty;

            // Constructor
            public SerializedPropertyMember(PropertyInfo serializeProperty)
                : base(GetSerializeMemberName(serializeProperty), serializeProperty.PropertyType)
            {
                this.serializeProperty = serializeProperty;
            }

            // Methods
            public override object GetValue(object instance)
            {
                return serializeProperty.GetValue(instance);
            }

            public override void SetValue(object instance, object value)
            {
                serializeProperty.SetValue(instance, value);
            }

            public override T GetAttribute<T>()
            {
                return serializeProperty.GetCustomAttribute<T>();
            }
        }
        internal sealed class SerializedArrayElement : SerializedProperty
        {
            // Private
            private readonly int index;

            // Constructor
            public SerializedArrayElement(Type elementType, int index)
                : base(index.ToString(), elementType)
            {
                this.index = index;
            }

            // Methods
            public override object GetValue(object instance)
            {
                if (instance is IList list)
                    return list[index];

                return null;
            }

            public override void SetValue(object instance, object value)
            {
                if (instance is IList list)
                    list[index] = value;
            }

            public override T GetAttribute<T>()
            {
                // No attributes for arrays
                return null;
            }
        }

        // Public
        public readonly string PropertyName;
        public readonly Type PropertyType;

        // Properties
        public bool IsArray => PropertyType.IsArray == true || typeof(IList).IsAssignableFrom(PropertyType) == true;
        public bool IsObject => PropertyType.IsPrimitive == false && PropertyType.IsEnum == false && PropertyType != typeof(string);

        // Constructor
        protected SerializedProperty(string name, Type type)
        {
            this.PropertyName = name;
            this.PropertyType = type;
        }

        // Methods
        public abstract object GetValue(object instance);
        public abstract void SetValue(object instance, object value);
        public abstract T GetAttribute<T>() where T : Attribute;
        public bool HasAttribute<T>() where T : Attribute
        {
            return GetAttribute<T>() != null;
        }

        protected static string GetSerializeMemberName(MemberInfo member)
        {
            // Get attribute
            DataMemberAttribute attrib = member.GetCustomAttribute<DataMemberAttribute>();

            // Get name from attribute
            if (attrib != null && string.IsNullOrEmpty(attrib.Name) == false && attrib.Name != member.Name)
                return attrib.Name;

            return member.Name;
        }

        public static Type GetArrayElementType(IList list)
        {
            // Get type
            Type listType = list != null ? list.GetType() : null;

            // Check for any
            if (listType != null)
            {
                // Check for array
                if (list is Array)
                    return listType.GetElementType();

                // Check for list
                if (listType.IsGenericType == true && typeof(List<>).IsAssignableFrom(listType.GetGenericTypeDefinition()) == true)
                    return listType.GetGenericArguments()[0];
            }

            // Unknown
            throw new ArgumentException("Unable to find array element type");
        }
    }
}
