using KoraGame;
using System.Reflection;

namespace KoraEditor
{
    public abstract class ElementEditor : EditorContext
    {
        // Private
        private static readonly Dictionary<Type, ElementEditor> specificElementEditors = new();
        private static readonly List<(Type, ElementEditor)> derivedElementEditors = new();

        private EditorSerializedLayout layout;
        private bool isModified = false;

        // Properties
        public EditorSerializedLayout Layout => layout;

        // Methods
        protected virtual void OnCreate() { }
        protected abstract void OnGui();

        public bool DrawEditorGui(EditorSerializedLayout layout)
        {
            // Check for null
            if (layout == null)
                return false;

            // Check type
            if (IsEditorFor(layout.SerializeType) == false)
                return false;

            // Set layout and state
            this.layout = layout;
            this.isModified = false;

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

        public bool IsEditorFor(Type type)
        {
            return ForType(type) == this;
        }

        public static ElementEditor ForType<T>()
        {
            return ForType(typeof(T));
        }

        public static ElementEditor ForType(Type type)
        {
            // Check for null
            if (type == null)
                return null;

            ElementEditor propertyEditor = null;

            // Check for specified
            if (specificElementEditors.TryGetValue(type, out propertyEditor) == false)
            {
                // Try to get derived
                foreach ((Type, ElementEditor) derivedPropertyEditor in derivedElementEditors)
                {
                    // Check for found
                    if (derivedPropertyEditor.Item1.IsAssignableFrom(type) == true)
                    {
                        propertyEditor = derivedPropertyEditor.Item2;
                        break;
                    }
                }
            }

            // Get property editor
            return propertyEditor;
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

                        // Create instance of editor
                        ElementEditor propertyEditor = (ElementEditor)Activator.CreateInstance(type);

                        // Check for specific
                        if (attrib.ForDerivedTypes == false)
                        {
                            // Check for already added
                            if (specificElementEditors.ContainsKey(attrib.ForType) == true)
                            {
                                Debug.LogError("An element editor already exists for type: " + attrib.ForType);
                                continue;
                            }

                            specificElementEditors[attrib.ForType] = propertyEditor;
                        }
                        // Add derived
                        else
                        {
                            // Add to derived
                            derivedElementEditors.Add((attrib.ForType, propertyEditor));
                        }
                    }
                }
            }
        }
    }
}
