using KoraGame;

namespace KoraEditor
{
    public sealed class EditorSerializedElement
    {
        // Private
        private SerializedElement element;
        private EditorSerializedLayout root;
        private List<EditorSerializedElement> childElements;
        private object[] instances;

        // Properties
        public EditorSerializedLayout Layout => root;
        public string ElementName => element.ElementName;
        public Type ElementType => element.ElementType;
        public bool IsVisible => element.HasAttribute<EditorHiddenAttribute>() == false;
        public bool IsReadOnly => element.HasAttribute<EditorReadOnlyAttribute>() == true;
        public bool IsEditingMultiple => instances.Length > 1;
        public bool IsArray => element.IsArray;
        public bool IsObject => element.IsObject;
        public IEnumerable<EditorSerializedElement> ChildElements => childElements;
        public IEnumerable<EditorSerializedElement> VisibleChildElements => childElements.Where(e => e.IsVisible);

        // Constructor
        internal EditorSerializedElement(SerializedElement element, EditorSerializedLayout root, object[] instances)
        {
            this.element = element;
            this.root = root;
            this.instances = instances;
            // Create child elements
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
            return string.Format("Serialize Property ({0}): {1}", ElementName, ElementType);
        }

        public void SetValue<T>(in T value)
        {
            // Check for write access
            if (IsReadOnly == true)
                throw new InvalidOperationException("Property is readonly and cannot be modified");

            // Check for type
            if (element.ElementType != typeof(T) && (value != null && element.ElementType != value.GetType()))
                throw new InvalidOperationException("Cannot set property value as type: " + typeof(T) + ", property is of type: " + element.ElementType);

            // Update all instances
            for (int i = 0; i < instances.Length; i++)
            {
                element.SetValue(instances[i], value);
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
            if (element.ElementType != typeof(T) && typeof(T).IsAssignableFrom(element.ElementType) == false)
                throw new InvalidOperationException("Cannot get property value as type: " + typeof(T) + ", property is of type: " + element.ElementType);

            // Store values
            T firstValue = instances[0] != null
                    ? (T)element.GetValue(instances[0])
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
                    ? (T)element.GetValue(instances[i])
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
                    ? element.GetValue(instances[0])
                    : default;

            // Check all
            for (int i = 1; i < instances.Length; i++)
            {
                // Get value
                object otherValue = instances[i] != null
                    ? element.GetValue(instances[i])
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

        public EditorSerializedElement FindChildElement(string name, bool includeHidden = false)
        {
            return childElements.FirstOrDefault(e => e.ElementName == name && e.IsVisible == true || includeHidden == true);
        }
    }
}
