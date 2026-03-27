using KoraGame;

namespace KoraEditor
{
    public sealed class Selection
    {
        // Events
        public event Action OnSelectionChanged;

        // Private
        private List<object> selectedObjects = new List<object>();
        private Type selectedType = null;

        // Properties
        public bool HasAnySelection
        {
            get { return selectedObjects.Count > 0; }
        }

        public bool IsSingleSelection
        {
            get { return selectedObjects.Count == 1; }
        }

        public bool IsCommonSelection
        {
            get
            {
                Type matchType = null;
                foreach (object selectedObject in selectedObjects)
                {
                    Type currentType = selectedObject.GetType();

                    if (matchType != null && currentType != matchType)
                        return false;

                    matchType = currentType;
                }
                // Must all be of same type
                return true;
            }
        }

        public Type SelectedType
        {
            get { return selectedType; }
        }

        // Methods
        public GameElement GetSelectedElement()
        {
            foreach (object selection in selectedObjects)
            {
                if (selection is GameElement)
                    return (GameElement)selection;
            }
            return null;
        }

        public IEnumerable<object> GetSelected()
        {
            return selectedObjects;
        }

        public IEnumerable<T> GetSelected<T>()
        {
            // Process all
            foreach (object selection in selectedObjects)
            {
                // Check for element
                if (selection is T)
                    yield return (T)selection;
            }
        }

        public object GetMainSelected()
        {
            return selectedObjects.Count >= 1
                ? selectedObjects[0]
                : null;
        }

        public T GetMainSelected<T>()
        {
            if (selectedObjects.Count >= 1 && selectedObjects[0] is T value)
                return value;

            return default;
        }

        public bool IsSelected<T>(T obj)
        {
            return selectedObjects.Contains(obj);
        }

        public void Clear()
        {
            Select<object>((object)null);
        }

        public void Select<T>(T selection)
        {
            // Clear current selection
            selectedObjects.Clear();
            selectedType = null;

            // Add selection
            if (selection != null)
            {
                selectedObjects.Add(selection);
                selectedType = selection.GetType();
            }

            // Trigger event
            Editor.DoEvent(OnSelectionChanged);
        }

        public void Select<T>(IEnumerable<T> selected)
        {
            // Check for null
            if (selected == null)
                throw new ArgumentNullException(nameof(selected));

            // Clear current selection
            selectedObjects.Clear();
            selectedType = null;

            // Add selection
            foreach (T selection in selected)
            {
                if (selected != null)
                    selectedObjects.Add(selection);
            }

            // Check for any
            if (selectedObjects.Count > 0)
                selectedType = typeof(T);

            // Trigger event
            Editor.DoEvent(OnSelectionChanged);
        }
    }
}
