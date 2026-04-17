using KoraGame.Graphics;
using System.Runtime.Serialization;

namespace KoraGame
{
    public class Scene : GameElement
    {
        // Private
        private bool active = false;

        // Internal
        [DataMember(Name = "GameObjects")]
        internal List<GameObject> gameObjects = new();

        internal readonly List<Camera> activeCameras = new();
        internal readonly List<Renderer> activeRenderers = new();
        internal readonly List<ScriptableBehaviour> activeBehaviours = new();

        // Properties
        public bool Active => active;
        public IReadOnlyList<GameObject> GameObjects => gameObjects;

        // Constructor
        private Scene() { }

        public Scene(string name)
            : base(name)
        {
        }

        // Methods
        internal void Activate()
        {
            active = true;

            // Update all objects
            foreach (GameObject go in gameObjects)
                go.SetActive(true);
        }

        internal void Deactivate()
        {
            active = false;

            // Update all objects
            foreach (GameObject go in gameObjects)
                go.SetActive(false);
        }

        internal void Update()
        {
            // Process all behaviours start
            foreach(ScriptableBehaviour behaviour in activeBehaviours)
            {
                // Start the component
                behaviour.DoStart();
            }

            // Process all behaviours update in a separate batch
            foreach (ScriptableBehaviour behaviour in activeBehaviours)
            {
                // Update the component
                behaviour.DoUpdate();
            }
        }

        internal void Draw(GraphicsBatch renderBatch)
        {
            // Process all renderers
            foreach(Renderer renderer in activeRenderers)
            {
                // Render the object
                if(renderer.Active == true)
                    renderer.Draw(renderBatch);
            }
        }

        public static Scene Empty(string name = "New Scene")
        {
            // Create the scene
            Scene scene = new Scene(name);

            // Create just a camera
            GameObject go = new GameObject("Cam");
            go.AddComponent<Camera>();

            // Set scene
            go.Scene = scene;

            return scene;
        }
    }
}
