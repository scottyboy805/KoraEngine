using KoraGame;
using KoraGame.Graphics;
using KoraGame.Physics;

namespace KoraEditor
{
    public sealed class EditorScene : Scene
    {
        // Events
        public event Action<GameObject> OnGameObjectModified;
        public event Action<GameObject> OnGameObjectCreated;
        public event Action<GameObject> OnGameObjectDestroyed;

        // Properties
        public static Editor EditorInstance => Editor.EditorInstance;
        public static EditorScene EditorSceneInstance => Editor.EditorInstance?.EditorScene;

        // Constructor
        internal EditorScene(string name)
            : base(name)
        {
        }

        // Methods
        public async void CreateDefault()
        {
            // Load assets
            Material mat = await EditorInstance.EditorAssets.LoadAsync<Material>("DefaultAssets/PbrMaterial.json");
            GameObject cube = await EditorInstance.EditorAssets.LoadAsync<GameObject>("DefaultAssets/Cube.fbx");

            // Setup cube
            cube.GetComponent<MeshRenderer>(true).SetMaterial(mat, 0);
            cube.AddComponent<BoxCollider>();
            cube.LocalPosition = new Vector3F(0f, 5f, -10f);


            GameObject cube2 = await EditorInstance.EditorAssets.LoadAsync<GameObject>("DefaultAssets/Cube.fbx");
            cube2 = GameObject.Instantiate(cube2);
            cube2.GetComponent<MeshRenderer>(true).SetMaterial(mat, 0);
            cube2.Parent = cube;
            cube2.LocalPosition = new Vector3F(5f, -5f, 0f);

            // Create the camera
            GameObject cam = CreateCameraObject();            
            cam.Scene = this;
            cam.SetActive(true);

            cube.Scene = this;
            cube.SetActive(true);
            cube2.Scene = this;
            cube2.SetActive(true);
        }

        public GameObject CreateEmptyObject(string name = null)
        {
            // Generate name
            if (name == null)
                name = GetNewObjectName("Game Object");

            // Create object
            GameObject go = new GameObject(name, false);

            // Add to scene
            go.Scene = this;

            // Select the new object
            Editor.EditorInstance?.Selection.Select(go);

            // Mark as dirty
            SetDirty();

            // Do event
            Editor.DoEvent(OnGameObjectModified, go);
            Editor.DoEvent(OnGameObjectCreated, go);

            return go;
        }

        public GameObject CreateCameraObject(string name = "Camera")
        {
            // Create object
            GameObject cam = CreateEmptyObject(name);

            // Add component
            cam.AddComponent<Camera>();

            // Do event
            Editor.DoEvent(OnGameObjectModified, cam);
            return cam;
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
