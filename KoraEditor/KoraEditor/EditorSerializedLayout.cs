using KoraGame;

namespace KoraEditor
{
    public sealed class EditorSerializedLayout
    {
        // Private
        private SerializedLayout layout;
        private List<EditorSerializedElement> elements;
        private object[] instances;
        
        // Properties
        public Type SerializeType => layout.SerializeType;
        public bool IsEditingMultiple => instances.Length > 1;
        public IReadOnlyList<object> EditingInstances => instances;
        public IEnumerable<EditorSerializedElement> Elements => elements;
        public IEnumerable<EditorSerializedElement> VisibleElements => elements.Where(e => e.IsVisible);

        // Constructor
        public EditorSerializedLayout(Type editInstanceType, object[] instances)
        {
            this.instances = instances;
            this.layout = SerializedLayout.GetSerializeLayout(editInstanceType);
            this.elements = layout.SerializeElements.Select(e => new EditorSerializedElement(e, this, instances)).ToList();
        }

        // Methods
        public EditorSerializedElement FindElement(string name, bool includeHidden = false)
        {
            return elements.FirstOrDefault(e => e.ElementName == name && (e.IsVisible == true || includeHidden == true));
        }

        /// <summary>
        /// Mark the associated objects as modified, so that changes will be saved and undoable.
        /// </summary>
        public void SetModified()
        {

        }
    }
}
