using KoraGame;

namespace KoraEditor
{
    public sealed class SerializedLayout
    {
        // Private
        private KoraGame.SerializedLayout layout;
        private List<SerializedElement> elements;
        private GameElement[] instances;
        
        // Properties
        public Type SerializeType => layout.SerializeType;
        public bool IsEditingMultiple => instances.Length > 1;
        public IReadOnlyList<GameElement> EditingInstances => instances;
        public IEnumerable<SerializedElement> Elements => elements;
        public IEnumerable<SerializedElement> VisibleElements => elements.Where(e => e.IsVisible);

        // Methods
        public SerializedElement FindElement(string name, bool includeHidden = false)
        {
            return elements.FirstOrDefault(e => e.ElementName == name && e.IsVisible == true || includeHidden == true);
        }

        /// <summary>
        /// Mark the associated objects as modified, so that changes will be saved and undoable.
        /// </summary>
        public void SetModified()
        {

        }
    }
}
