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
                name = "New Game Object";

            // Create object
            GameObject go = new GameObject(name, false);

            // Add to scene
            gameObjects.Add(go);

            // Mark as dirty


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

        #region MenuActions_GameObject
        public static void CreateEmptyObjectAction()
        {
            EditorSceneInstance?.CreateEmptyObject();
        }
        #endregion
    }
}
