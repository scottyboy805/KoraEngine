using KoraGame;

namespace KoraEditor
{
    public sealed class EditorScene : Scene
    {
        // Events
        public event Action<GameObject> OnGameObjectModified;
        public event Action<GameObject> OnGameObjectCreated;
        public event Action<GameObject> OnGameObjectDestroyed;

        // Properties
        public static EditorScene EditorSceneInstance => Editor.EditorInstance?.EditorScene;

        // Constructor
        internal EditorScene(string name)
            : base(name)
        {
        }

        // Methods
        public GameObject CreateEmptyObject(string name = null)
        {
            // Generate name
            if (name == null)
                name = GetNewObjectName("Game Object");

            // Create object
            GameObject go = new GameObject(name, false);

            // Add to scene
            gameObjects.Add(go);

            // Select the new object
            Editor.EditorInstance?.Selection.Select(go);

            // Mark as dirty
            SetDirty();

            // Do event
            Editor.DoEvent(OnGameObjectModified, go);
            Editor.DoEvent(OnGameObjectCreated, go);

            return go;
        }

        public void SetDirty()
        {
            if (Editor.EditorInstance != null && Editor.EditorInstance.AssetDatabase != null)
                Editor.EditorInstance.AssetDatabase.SetAssetDirty(this);
        }

        private string GetNewObjectName(string baseName)
        {
            int counter = 1;
            string currentName = baseName;

            // Check for exists
            while(gameObjects.Any(g => g.Name == currentName) == true)
            {
                currentName = baseName + " " + counter.ToString();
                counter++;
            }
            return currentName;
        }

        #region MenuActions_GameObject
        [Menu("GameObject/Empty")]
        public static void CreateEmptyObjectAction()
        {
            EditorSceneInstance?.CreateEmptyObject();
        }
        #endregion
    }
}
