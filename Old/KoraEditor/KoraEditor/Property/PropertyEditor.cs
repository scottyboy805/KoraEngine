using KoraEditor.UI;
using KoraGame;
using System.Reflection;

namespace KoraEditor
{
    public abstract class PropertyEditor : EditorContext
    {
        // Private
        private static readonly Dictionary<Type, Type> specificPropertyEditors = new ();  // Edit Type, PropertyEditor Type
        private static readonly List<(Type, Type)> derivedPropertyEditors = new ();       // Edit Type, PropertyEditor Type

        private EditorSerializedProperty property;
        private bool isModified = false;
        private float[] columnSizes = new float[1];

        // Properties
        public EditorSerializedProperty Property => property;
        public float PropertyLabelWidth => Gui.PropertyLableWidth;
        public float PropertyValueWidth => Gui.PropertyValueWidth;

        // Methods
        protected virtual void OnCreate() { }
        protected virtual void OnGui() 
        {
            // Update column size
            columnSizes[0] = Gui.PropertyLableWidth;

            // Start table
            Gui.BeginTableLayout(2, columnSizes);
            {
                // Display label
                OnLabelGui();

                // Column separator
                Gui.ColumnSeparator();

                // Display value
                OnValueGui();
            }
            Gui.EndTableLayout();
        }

        protected virtual void OnLabelGui()
        {
            Gui.PropertyLabel(property);
        }

        protected virtual void OnValueGui()
        {
            Gui.Label("Property editor not implemented");
        }

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
            this.property?.Layout.SetModified();
        }

        public static PropertyEditor ForElement(EditorSerializedProperty element)
        {
            // Check for null
            if (element == null)
                return null;

            // Lookup type
            Type propertyEditorType = GetPropertyEditorType(element.PropertyType);

            // Check for no editor
            if (propertyEditorType == null)
                return null;

            // Create instance
            PropertyEditor editor = (PropertyEditor)Activator.CreateInstance(propertyEditorType);
            editor.property = element;

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

        private static Type GetPropertyEditorType(Type type)
        {
            // Check for null
            if (type == null)
                return null;

            Type propertyEditor = null;

            // Check for specified
            if (specificPropertyEditors.TryGetValue(type, out propertyEditor) == false)
            {
                // Try to get derived
                foreach ((Type, Type) derivedPropertyEditor in derivedPropertyEditors)
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
            Assembly thisAsm = typeof(PropertyEditor).Assembly;
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
                    if (type.IsDefined(typeof(PropertyEditorForAttribute)) == true)
                    {
                        // Get the attribute
                        PropertyEditorForAttribute attrib = type.GetCustomAttribute<PropertyEditorForAttribute>();

                        // Check for type
                        if (typeof(PropertyEditor).IsAssignableFrom(type) == false)
                        {
                            Debug.LogError($"Property editor '{type}' must derive from '{typeof(PropertyEditor)}'");
                            break;
                        }
                        // Check for specific
                        if (attrib.ForDerivedTypes == false)
                        {
                            // Check for already added
                            if (specificPropertyEditors.ContainsKey(attrib.ForType) == true)
                            {
                                Debug.LogError("A property editor already exists for type: " + attrib.ForType);
                                continue;
                            }

                            specificPropertyEditors[attrib.ForType] = type;
                        }
                        // Add derived
                        else
                        {
                            // Add to derived
                            derivedPropertyEditors.Add((attrib.ForType, type));
                        }
                    }
                }
            }
        }
    }
}
