using KoraGame.Graphics;
using KoraGame.Physics;
using SDL;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("KoraGame-Desktop")]
[assembly: InternalsVisibleTo("KoraEditor")]
[assembly: InternalsVisibleTo("KoraPipeline")]

namespace KoraGame
{
    public sealed class Game
    {
        // Private
        private readonly Queue<GameElement> destroyElements = new();

        private bool quit = false;
        private GameSettings settings = null;
        private Screen screen = null;
        private GraphicsDevice graphics = null;
        private AssetProvider assets = null;
        private ScriptableProvider scriptable = null;
        private PhysicsWorld physics = null;
        private Scene scene = null;
        private bool isEditor = false;
        private bool isPlaying = false;

        private ulong lastFrameTime = 0;
        private ulong performanceFrequency = 0;        

        // Properties    
        public bool Quit => quit;
        public GameSettings Settings => settings;
        public Screen Screen => screen;
        public GraphicsDevice GraphicsDevice => graphics;
        public AssetProvider Assets => assets;
        public ScriptableProvider Scriptable => scriptable;
        public PhysicsWorld PhysicsWorld => physics;
        public Scene Scene => scene;

        public bool IsEditor => isEditor;
        public bool IsPlaying => isPlaying;        

        // Constructor
        internal Game(GameSettings settings, Screen screen, GraphicsDevice graphics, AssetProvider assets, ScriptableProvider scriptable, PhysicsWorld physics, bool isEditor, bool isPlaying)
        {
            this.settings = settings;
            this.screen = screen;
            this.graphics = graphics;
            this.assets = assets;
            this.scriptable = scriptable;
            this.physics = physics;
            this.isEditor = isEditor;
            this.isPlaying = isPlaying;
        }

        // Methods
        public void ChangeScene(Scene scene)
        {
            if(this.scene == scene)
                Debug.LogWarning("Scene is already active");

            // Deactivate current scene
            if (this.scene != null)
                this.scene.SetActive(false);
            
            // Switch scene
            this.scene = scene;

            // Activate
            if (scene != null)
                scene.SetActive(true);

            Debug.Log($"Change current scene: '{(scene != null ? scene.Name : null)}'", LogFilter.Game);
        }

        internal void Initialize()
        {
            // Initialize timing
            performanceFrequency = SDL3.SDL_GetPerformanceFrequency();
            lastFrameTime = SDL3.SDL_GetPerformanceCounter();
                        

            Scene scene = new Scene("MyScene");

            GameObject cam = new GameObject("Camera");
            cam.AddComponent<Camera>();
            cam.Scene = scene;

            GameObject cube = assets.LoadAsync<GameObject>("DefaultAssets/Cube.fbx").Result;
            //cube.GetComponent<MeshRenderer>().SetMaterial(null);
            cube.Scene = scene;
            cube.WorldPosition = new Vector3F(0, 0, -5);
            cube.WorldRotation = QuaternionF.Euler(0, 45, 0);

            cube.AddComponent<RigidBody>();
            cube.AddComponent<BoxCollider>();

            ChangeScene(scene);
        }

        internal void Update()
        {
            // Calculate frame time
            ulong currentTime = SDL3.SDL_GetPerformanceCounter();
            float deltaTime = (float)(currentTime - lastFrameTime) / performanceFrequency;
            lastFrameTime = currentTime;

            // Update time system
            Time.UpdateTime(deltaTime);

            // Update physics
            physics?.Update();

            // Render current scene
            if (scene != null)
            {
                // Update all objects
                scene.Update();

                // Render all cameras
                foreach (Camera camera in scene.activeCameras)
                {
                    // Render the camera
                    camera.Render();
                }
            }

            // Update input
            //input.UpdateInputStates();

            // Update title
            screen.Title = "Fps = " + Time.FPS.ToString("F2");

            // Destroy elements at the end of the frame
            while (destroyElements.Count > 0)
                GameElement.DestroyImmediate(destroyElements.Dequeue());
        }

        internal void Shutdown()
        {
            // Stop physics
            physics?.Shutdown();

            // Unload assets
            assets?.UnloadAll();

            // Shutdown debug
            Debug.Terminate();
        }

        internal void HandleEvent(in SDL_Event evt)
        {
            switch(evt.Type)
            {
                case SDL_EventType.SDL_EVENT_QUIT:
                    {
                        quit = true;
                        break;
                    }

                // Input
                case SDL_EventType.SDL_EVENT_MOUSE_MOTION:
                    {
                        //input?.DoMouseMove(evt.motion.x, evt.motion.y);
                        break;
                    }
                case SDL_EventType.SDL_EVENT_MOUSE_WHEEL:
                    {
                        break;
                    }
                case SDL_EventType.SDL_EVENT_MOUSE_BUTTON_UP:
                case SDL_EventType.SDL_EVENT_MOUSE_BUTTON_DOWN:
                    {
                        //input?.DoMouseButtonEvent((MouseButton)evt.button.Button, evt.button.down);
                        break;
                    }
                case SDL_EventType.SDL_EVENT_KEY_UP:
                case SDL_EventType.SDL_EVENT_KEY_DOWN:
                    {
                        //input?.DoKeyboardButtonEvent((Key)evt.key.key, evt.key.down);
                        break;
                    }

                case SDL_EventType.SDL_EVENT_GAMEPAD_ADDED:
                case SDL_EventType.SDL_EVENT_GAMEPAD_REMOVED:
                    {
                        //input?.DoControllerAvailabilityEvent((int)evt.gdevice.which, evt.Type == SDL_EventType.SDL_EVENT_GAMEPAD_ADDED);
                        break;
                    }
                case SDL_EventType.SDL_EVENT_GAMEPAD_BUTTON_UP:
                case SDL_EventType.SDL_EVENT_GAMEPAD_BUTTON_DOWN:
                    {
                        //input?.DoControllerButtonEvent((int)evt.gbutton.which, (ControllerButton)evt.gbutton.button, evt.gbutton.down);
                        break;
                    }
                case SDL_EventType.SDL_EVENT_GAMEPAD_AXIS_MOTION:
                    {
                        // Get the value
                        int axisValue = evt.gaxis.value;

                        // Remap to float
                        //float remappedAxisValue = -1f + (axisValue - -InputProvider.ControllerAxisRange) * (1f - -1f) / (InputProvider.ControllerAxisRange - -InputProvider.ControllerAxisRange);

                        //input?.DoControllerAxisEvent((int)evt.gaxis.which, (ControllerAxis)evt.gaxis.axis, remappedAxisValue);
                        break;
                    }
                
            }
        }

        internal void DestroyDelayed(GameElement element)
        {
            // Will be destroyed at the end of the frame
            destroyElements.Enqueue(element);
        }

        internal static void DoEvent(Action action)
        {
            try
            {
                if (action != null)
                    action.Invoke();
            }
            catch (Exception e) { Debug.LogException(e); }
        }

        internal static void DoEvent<T>(Action<T> action, T arg0)
        {
            try
            {
                if (action != null)
                    action.Invoke(arg0);
            }
            catch (Exception e) { Debug.LogException(e); }
        }

        internal static void DoEvent<T0, T1>(Action<T0, T1> action, T0 arg0, T1 arg1)
        {
            try
            {
                if (action != null)
                    action.Invoke(arg0, arg1);
            }
            catch (Exception e) { Debug.LogException(e); }
        }
    }
}
