using KoraGame;
using System.Text.RegularExpressions;

namespace KoraEditor
{
    public sealed class EditorSerializedLayout
    {
        // Private
        private SerializedLayout layout;
        private List<EditorSerializedProperty> properties;
        private object[] instances;

        // Properties
        public string DisplayName
        {
            get
            {
                // Add space before capital letters (except the first one)
                var result = Regex.Replace(layout.SerializeType.Name, "(\\B[A-Z])", " $1");

                // Capitalize first letter
                return char.ToUpper(result[0]) + result.Substring(1);
            }
        }
        public Type SerializeType => layout.SerializeType;
        public bool IsEditingMultiple => instances.Length > 1;
        public IReadOnlyList<object> EditingInstances => instances;
        public IEnumerable<EditorSerializedProperty> Properties => properties;
        public IEnumerable<EditorSerializedProperty> VisibleProperties => properties.Where(e => e.IsVisible);

        // Constructor
        public EditorSerializedLayout(Type editInstanceType, object[] instances)
        {
            this.instances = instances;
            this.layout = SerializedLayout.GetSerializeLayout(editInstanceType);
            this.properties = layout.SerializeProperties.Select(e => new EditorSerializedProperty(e, this, instances)).ToList();
        }

        // Methods
        public EditorSerializedProperty FindProperty(string name, bool includeHidden = false)
        {
            return properties.FirstOrDefault(e => e.PropertyName == name && (e.IsVisible == true || includeHidden == true));
        }

        /// <summary>
        /// Mark the associated objects as modified, so that changes will be saved and undoable.
        /// </summary>
        public void SetModified()
        {

        }
    }
}
