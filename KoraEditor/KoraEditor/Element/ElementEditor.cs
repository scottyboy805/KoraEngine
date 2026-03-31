using KoraGame;
using System.Reflection;

namespace KoraEditor
{
    public abstract class ElementEditor : EditorContext
    {
        // Private
        private static readonly Dictionary<Type, Type> specificElementEditors = new(); // Edit Type, ElementEditor Type
        private static readonly List<(Type, Type)> derivedElementEditors = new();      // Edit Type, ElementEditor Type

        private EditorSerializedLayout layout;
        private bool isModified = false;

        // Properties
        public EditorSerializedLayout Layout => layout;

        // Methods
        protected virtual void OnCreate() { }
        protected abstract void OnGui();

        public bool DrawEditorGui()
        {
            // Draw gui
            OnGui();

            // Check modified
            return isModified;
        }

        public void SetModified()
        {
            this.isModified = true;
            this.layout?.SetModified();
        }

        public IEnumerable<PropertyEditor> GetPropertyEditors(bool visible)
        {
            // Select elements
            IEnumerable<EditorSerializedElement> elements = visible == true
                ? layout.VisibleElements
                : layout.Elements;

            // Create editor for element
            return elements
                .Select(PropertyEditor.ForElement)
                .Where(e => e != null);
        }

        public static ElementEditor ForElement(object element)
        {
            // Check for null
            if (element == null)
                return null;

            // Create layout
            EditorSerializedLayout layout = new EditorSerializedLayout(element.GetType(), new[] { element });

            // Creat editor
            return ForLayout(layout);
        }

        public static ElementEditor ForElements(Type editType, object[] elements)
        {
            // Check for null
            if (editType == null || elements == null)
                return null;

            // Create layout
            EditorSerializedLayout layout = new EditorSerializedLayout(editType, new[] { elements });

            // Creat editor
            return ForLayout(layout);
        }

        public static ElementEditor ForLayout(EditorSerializedLayout layout)
        {
            // Check for null
            if (layout == null)
                return null;

            // Lookup type
            Type elementEditorType = GetElementEditorType(layout.SerializeType);

            // Create instance
            ElementEditor editor = (ElementEditor)Activator.CreateInstance(elementEditorType);
            editor.layout = layout;

            // Create editor
            try
            {
                editor.OnCreate();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return editor;
        }

        private static Type GetElementEditorType(Type type)
        {
            // Check for null
            if (type == null)
                return null;

            Type propertyEditorType = null;

            // Check for specified
            if (specificElementEditors.TryGetValue(type, out propertyEditorType) == false)
            {
                // Try to get derived
                foreach ((Type, Type) derivedPropertyEditor in derivedElementEditors)
                {
                    // Check for found
                    if (derivedPropertyEditor.Item1.IsAssignableFrom(type) == true)
                    {
                        propertyEditorType = derivedPropertyEditor.Item2;
                        break;
                    }
                }
            }

            // Get property editor
            return propertyEditorType;
        }

        internal static void InitializePropertyEditors()
        {
            // Get this assembly name
            Assembly thisAsm = typeof(ElementEditor).Assembly;
            AssemblyName thisName = thisAsm.GetName();

            // Process all assemblies
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                // Check if we should scan
                bool shouldCheckAssembly = thisAsm == asm;

                // Check for referenced
                if (shouldCheckAssembly == false)
                {
                    // Get references
                    AssemblyName[] referenceNames = asm.GetReferencedAssemblies();

                    // Check for assembly referenced
                    foreach (AssemblyName referenceName in referenceNames)
                    {
                        if (referenceName.FullName == thisName.FullName)
                        {
                            shouldCheckAssembly = true;
                            break;
                        }
                    }
                }

                // Check for skip
                if (shouldCheckAssembly == false)
                    continue;

                Type[] checkTypes = null;

                try
                {
                    // Try to load all types
                    checkTypes = asm.GetTypes();
                }
                catch (ReflectionTypeLoadException e)
                {
                    Debug.LogException(e);

                    // Get all types that could be loaded
                    checkTypes = e.Types.Where(t => t != null)
                        .ToArray();
                }

                // Check all types
                foreach (Type type in checkTypes)
                {
                    // Check for attribute
                    if (type.IsDefined(typeof(ElementEditorForAttribute)) == true)
                    {
                        // Get the attribute
                        ElementEditorForAttribute attrib = type.GetCustomAttribute<ElementEditorForAttribute>();

                        // Check for type
                        if (typeof(ElementEditor).IsAssignableFrom(type) == false)
                        {
                            Debug.LogError($"Element editor '{type}' must derive from '{typeof(ElementEditor)}'");
                            break;
                        }

                        // Check for specific
                        if (attrib.ForDerivedTypes == false)
                        {
                            // Check for already added
                            if (specificElementEditors.ContainsKey(attrib.ForType) == true)
                            {
                                Debug.LogError("An element editor already exists for type: " + attrib.ForType);
                                continue;
                            }

                            specificElementEditors[attrib.ForType] = type;
                        }
                        // Add derived
                        else
                        {
                            // Add to derived
                            derivedElementEditors.Add((attrib.ForType, type));
                        }
                    }
                }
            }
        }
    }
}
