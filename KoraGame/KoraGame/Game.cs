using SDL;
using KoraGame.Graphics;
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
        private GraphicsProvider graphics = null;
        private AssetProvider assets = null;
        private ScriptableProvider scriptable = null;
        private Scene scene = null;
        private bool isEditor = false;
        private bool isPlaying = false;

        private ulong lastFrameTime = 0;
        private ulong performanceFrequency = 0;        

        // Properties    
        public bool Quit => quit;
        public GameSettings Settings => settings;
        public Screen Screen => screen;
        public GraphicsProvider Graphics => graphics;
        public AssetProvider Assets => assets;
        public ScriptableProvider Scriptable => scriptable;
        public Scene Scene => scene;

        public bool IsEditor => isEditor;
        public bool IsPlaying => isPlaying;        

        // Constructor
        internal Game(GameSettings settings, Screen screen, GraphicsProvider graphics, AssetProvider assets, ScriptableProvider scriptable, bool isEditor, bool isPlaying)
        {
            this.settings = settings;
            this.screen = screen;
            this.graphics = graphics;
            this.assets = assets;
            this.scriptable = scriptable;
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
            // Init SDL
            Debug.Log("Initialize SDL", LogFilter.Game);
            if(SDL3.SDL_Init(SDL_InitFlags.SDL_INIT_VIDEO | SDL_InitFlags.SDL_INIT_GAMEPAD) == false)
            {
                Debug.LogError("Failed to initialize SDL", LogFilter.Game);
                return;
            }

            // Init SDL audio
            if(SDL3_mixer.MIX_Init() == false)
            {
                Debug.LogError("Failed to initialize SDL mixer", LogFilter.Audio);
            }

            // Init SDL font
            if(SDL3_ttf.TTF_Init() == false)
            {
                Debug.LogError("Failed to initialize SDL ttf", LogFilter.Graphics);
            }

            // Initialize timing
            performanceFrequency = SDL3.SDL_GetPerformanceFrequency();
            lastFrameTime = SDL3.SDL_GetPerformanceCounter();
                        

            Scene scene = new Scene("MyScene");

            GameObject cam = new GameObject("Camera");
            cam.AddComponent<Camera>();
            cam.Scene = scene;

            GameObject cube = assets.LoadAsync<GameObject>("DefaultAssets/Cube.fbx").Result;
            cube.Scene = scene;
            cube.WorldPosition = new Vector3F(0, 0, -5);
            cube.WorldRotation = QuaternionF.Euler(0, 45, 0);

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
            // Unload assets
            assets?.UnloadAll();

            // Shutdown debug
            Debug.Terminate();

            // Quit ttf
            SDL3_ttf.TTF_Quit();

            // Quit mixer
            SDL3_mixer.MIX_Quit();

            // Quit SDL
            SDL3.SDL_Quit();
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
