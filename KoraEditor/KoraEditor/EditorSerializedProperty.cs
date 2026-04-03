using KoraGame;
using System.Collections;
using System.Text.RegularExpressions;

namespace KoraEditor
{
    public sealed class EditorSerializedProperty
    {
        // Private
        private SerializedProperty property;
        private EditorSerializedLayout root;
        private List<EditorSerializedProperty> childProperties;
        private object[] instances;

        // Properties
        public EditorSerializedLayout Layout => root;
        public string DisplayName
        {
            get
            {
                // Get the name attribute
                if (property.HasAttribute<EditorNameAttribute>() == true)
                    return property.GetAttribute<EditorNameAttribute>().DisplayName;

                // Add space before capital letters (except the first one)
                var result = Regex.Replace(property.PropertyName, "(\\B[A-Z])", " $1");

                // Capitalize first letter
                return char.ToUpper(result[0]) + result.Substring(1);
            }
        }
        public string Tooltip
        {
            get
            {
                // Get the tooltip value
                if (property.HasAttribute<EditorTooltipAttribute>() == true)
                    return property.GetAttribute<EditorTooltipAttribute>().Tooltip;

                return null;
            }
        }
        public string PropertyName => property.PropertyName;
        public Type PropertyType => property.PropertyType;
        public bool IsVisible => property.HasAttribute<EditorHiddenAttribute>() == false;
        public bool IsReadOnly => property.HasAttribute<EditorReadOnlyAttribute>() == true;
        public bool IsEditingMultiple => instances.Length > 1;
        public bool IsArray => property.IsArray;
        public bool IsObject => property.IsObject;
        public IEnumerable<EditorSerializedProperty> ChildProperties => childProperties != null ? childProperties : Array.Empty<EditorSerializedProperty>();
        public IEnumerable<EditorSerializedProperty> VisibleChildProperties => ChildProperties.Where(e => e.IsVisible);

        // Constructor
        internal EditorSerializedProperty(SerializedProperty property, EditorSerializedLayout root, object[] instances)
        {
            this.property = property;
            this.root = root;
            this.instances = instances;

            // Create child elements
            if(property.IsArray == true)
            {
                // Get the element
                IList[] arrayInstances = instances
                    .Select(e => property.GetValue(e) as IList)
                    .ToArray();

                // Get max length
                int length = arrayInstances.Max(e => e.Count);

                // Check for any
                if (length > 0)
                {
                    // Create child elements
                    this.childProperties = new List<EditorSerializedProperty>(length);

                    // Get element type
                    Type arrayElementType = SerializedProperty.GetArrayElementType(arrayInstances.First());

                    // Create all elements
                    for (int i = 0; i < length; i++)
                    {
                        // Get instances
                        object[] arrayElementInstances = arrayInstances.Select(a => a[i]).ToArray();

                        // Find the first explicit type if possible
                        Type firstExplicitArrayElementType = arrayElementInstances
                            .Where(e => e != null)
                            .Select(e => e.GetType())
                            .FirstOrDefault();

                        // Select element type
                        if (firstExplicitArrayElementType != null && arrayElementInstances.All(i => i.GetType() == firstExplicitArrayElementType) == true)
                            arrayElementType = firstExplicitArrayElementType;

                        // Create the array element item
                        SerializedProperty.SerializedArrayElement arrayElement = new SerializedProperty.SerializedArrayElement(arrayElementType, i);

                        // Create all elements
                        childProperties.Add(new EditorSerializedProperty(arrayElement, root, arrayElementInstances));
                    }
                }
            }
            //childElements = new List<SerializedElement>();
            //foreach (KoraGame.SerializedElement child in element..ChildElements)
            //{
            //    object[] childInstances = new object[instances.Length];
            //    for (int i = 0; i < instances.Length; i++)
            //    {
            //        childInstances[i] = child.GetValue(instances[i]);
            //    }
            //    childElements.Add(new SerializedElement(child, childInstances));
            //}
        }

        // Methods
        public override string ToString()
        {
            return string.Format("Serialize Property ({0}): {1}", PropertyName, PropertyType);
        }

        public ElementEditor CreateEditor()
        {
            return ElementEditor.ForElements(PropertyType, instances);
        }

        public void SetValue<T>(in T value)
        {
            // Check for write access
            if (IsReadOnly == true)
                throw new InvalidOperationException("Property is readonly and cannot be modified");

            // Check for type
            if (property.PropertyType != typeof(T) && (value != null && property.PropertyType != value.GetType()))
                throw new InvalidOperationException("Cannot set property value as type: " + typeof(T) + ", property is of type: " + property.PropertyType);

            // Update all instances
            for (int i = 0; i < instances.Length; i++)
            {
                property.SetValue(instances[i], value);
            }

            // Mark as modified
            root.SetModified();
        }

        public T GetValue<T>()
        {
            return GetValue<T>(out _);
        }

        public T GetValue<T>(out bool isMixed)
        {
            // Check for type
            if (property.PropertyType != typeof(T) && typeof(T).IsAssignableFrom(property.PropertyType) == false)
                throw new InvalidOperationException("Cannot get property value as type: " + typeof(T) + ", property is of type: " + property.PropertyType);

            // Store values
            T firstValue = instances[0] != null
                    ? (T)property.GetValue(instances[0])
                    : default;
            isMixed = false;

            // Check for single instance
            if(instances.Length == 1)
                return firstValue;

            // Check all
            for (int i = 1; i < instances.Length; i++)
            {
                // Get value
                T otherValue = instances[i] != null
                    ? (T)property.GetValue(instances[i])
                    : default;

                // Check for mixed
                if ((firstValue != null && firstValue.Equals(otherValue) == false)
                    || (otherValue != null && otherValue.Equals(firstValue) == false))
                {
                    isMixed = true;
                    break;
                }
            }
            return firstValue;
        }

        public bool IsMixed()
        {
            // Check for simple case
            if (instances == null || instances.Length <= 1)
                return false;

            // Store values
            object firstValue = instances[0] != null
                    ? property.GetValue(instances[0])
                    : default;

            // Check all
            for (int i = 1; i < instances.Length; i++)
            {
                // Get value
                object otherValue = instances[i] != null
                    ? property.GetValue(instances[i])
                    : default;

                // Check for mixed
                if ((firstValue != null && firstValue.Equals(otherValue) == false)
                    || (otherValue != null && otherValue.Equals(firstValue) == false))
                {
                    // Value is mixed
                    return true;
                }
            }
            return false;
        }

        public EditorSerializedProperty FindChildProperty(string name, bool includeHidden = false)
        {
            return childProperties.FirstOrDefault(e => e.PropertyName == name && e.IsVisible == true || includeHidden == true);
        }
    }
}
