using KoraGame;
using System.Collections;
using System.Runtime.InteropServices.Marshalling;
using System.Text.RegularExpressions;

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
        public string DisplayName
        {
            get
            {
                // Get the name attribute
                if (element.HasAttribute<EditorNameAttribute>() == true)
                    return element.GetAttribute<EditorNameAttribute>().DisplayName;

                // Add space before capital letters (except the first one)
                var result = Regex.Replace(element.ElementName, "(\\B[A-Z])", " $1");

                // Capitalize first letter
                return char.ToUpper(result[0]) + result.Substring(1);
            }
        }
        public string Tooltip
        {
            get
            {
                // Get the tooltip value
                if (element.HasAttribute<EditorTooltipAttribute>() == true)
                    return element.GetAttribute<EditorTooltipAttribute>().Tooltip;

                return null;
            }
        }
        public string ElementName => element.ElementName;
        public Type ElementType => element.ElementType;
        public bool IsVisible => element.HasAttribute<EditorHiddenAttribute>() == false;
        public bool IsReadOnly => element.HasAttribute<EditorReadOnlyAttribute>() == true;
        public bool IsEditingMultiple => instances.Length > 1;
        public bool IsArray => element.IsArray;
        public bool IsObject => element.IsObject;
        public IEnumerable<EditorSerializedElement> ChildElements => childElements != null ? childElements : Array.Empty<EditorSerializedElement>();
        public IEnumerable<EditorSerializedElement> VisibleChildElements => ChildElements.Where(e => e.IsVisible);

        // Constructor
        internal EditorSerializedElement(SerializedElement element, EditorSerializedLayout root, object[] instances)
        {
            this.element = element;
            this.root = root;
            this.instances = instances;

            // Create child elements
            if(element.IsArray == true)
            {
                // Get the element
                IList[] arrayInstances = instances
                    .Select(e => element.GetValue(e) as IList)
                    .ToArray();

                // Get max length
                int length = arrayInstances.Max(e => e.Count);

                // Check for any
                if (length > 0)
                {
                    // Create child elements
                    this.childElements = new List<EditorSerializedElement>(length);

                    // Get element type
                    Type arrayElementType = SerializedElement.GetArrayElementType(arrayInstances.First());

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
                        SerializedElement.SerializedArrayElement arrayElement = new SerializedElement.SerializedArrayElement(arrayElementType, i);

                        // Create all elements
                        childElements.Add(new EditorSerializedElement(arrayElement, root, arrayElementInstances));
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
            return string.Format("Serialize Property ({0}): {1}", ElementName, ElementType);
        }

        public ElementEditor CreateEditor()
        {
            return ElementEditor.ForElements(ElementType, instances);
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
