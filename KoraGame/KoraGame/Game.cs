using SDL;
using KoraGame.Audio;
using KoraGame.Graphics;
using KoraGame.Input;
using KoraGame.Physics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

[assembly: InternalsVisibleTo("KoraGame-Desktop")]

namespace KoraGame
{
    [Serializable]
    public sealed class Game
    {
        // Private
        private static Game instance;
        private readonly Queue<GameElement> destroyElements = new();

        private bool quit = false;
        private Screen screen = null;
        private GraphicsDevice graphics = null;
        private AssetProvider assets = null;
        private AudioDevice audio = null;
        private InputProvider input = null;
        private PhysicsSimulation physics = null;
        private ScriptableProvider scriptable = null;
        private Scene scene = null;

        private ulong lastFrameTime = 0;
        private ulong performanceFrequency = 0;

        [DataMember(Name = "GameName")]
        private string gameName = "Default Game";
        [DataMember(Name = "GameVersion")]
        private Version gameVersion = new Version(1, 0, 0);
        [DataMember(Name = "CompanyName")]
        private string companyName = "Default Company";
        [DataMember(Name = "PreferredScreenWidth")]
        private uint preferredScreenWidth = 720;
        [DataMember(Name = "PreferredScreenHeight")]
        private uint preferredScreenHeight = 480;
        [DataMember(Name = "FullScreen")]
        private bool fullScreen = false;

        // Properties
        internal static Game Instance => instance;        

        public bool Quit => quit;
        public Screen Screen => screen;
        public GraphicsDevice Graphics => graphics;
        public AssetProvider Assets => assets;
        public AudioDevice Audio => audio;
        public InputProvider Input => input;
        public PhysicsSimulation Physics => physics;
        public ScriptableProvider Scriptable => scriptable;
        public Scene Scene => scene;

        public string GameName => gameName;
        public Version GameVersion => gameVersion;
        public string CompanyName => companyName;

        // Constructor
        internal Game()
        {
            instance = this;
        }

        ~Game()
        {
            if (instance == this)
                instance = null;
        }

        // Methods
        public void ChangeScene(Scene scene)
        {
            // Deactivate current scene
            if (this.scene != null)
                this.scene.Deactivate();
            
            // Switch scene
            this.scene = scene;

            // Activate
            if (scene != null)
                scene.Activate();

            Debug.Log($"Change current scene: '{(scene != null ? scene.Name : null)}'", LogFilter.Game);
        }
        AudioClip clip;
        internal void DoInitialize()
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

            // Read configuration
            if (File.Exists("Settings.json") == true)
                SerializedJson.PopulateAsync(File.ReadAllText("Settings.json"), this).Wait();

            // Create scripting
            Debug.Log("Initialize scripting", LogFilter.Script);
            this.scriptable = new ScriptableProvider();

            // Create the screen
            Debug.Log("Initialize graphics", LogFilter.Graphics);
            this.screen = new Screen(gameName, preferredScreenWidth, preferredScreenHeight, fullScreen);

            Debug.Log($"Use screen resolution: '{screen.Width} x {screen.Height}', FullScreen = '{screen.Fullscreen}'", LogFilter.Graphics);

            // Create graphics            
            this.graphics = new GraphicsDevice(this.screen);

            Debug.Log($"Use graphics API: '{graphics.GetDeviceDriverName()}'", LogFilter.Graphics);

            // Create assets
            Debug.Log($"Initialize assets", LogFilter.Assets);
            this.assets = new AssetProvider(scriptable, graphics, Environment.CurrentDirectory, false);

            Debug.Log($"Use assets directory: '{assets.AssetDirectory}'", LogFilter.Assets);

            // Create audio
            Debug.Log("Initialize audio", LogFilter.Audio);
            this.audio = new AudioDevice();

            // Create input
            Debug.Log("Initialize input", LogFilter.Input);
            this.input = new InputProvider();

            // Create physics
            Debug.Log("Initialize physics", LogFilter.Physics);
            this.physics = new PhysicsSimulation();

            //screen.VSync = VSyncMode.Sync;


            // Initialize timing
            performanceFrequency = SDL3.SDL_GetPerformanceFrequency();
            lastFrameTime = SDL3.SDL_GetPerformanceCounter();



            clip = AudioClip.LoadWav(audio, "ChangeRim.wav");


            var a = assets.LoadAsync<Shader>("DefaultAssets/PbrShader.json").Result;

            var res = assets.LoadAsync<Shader>("DefaultAssets/ErrorShader.json").Result;

            GameObject go = assets.LoadAsync<GameObject>("MonkeyObject.json").Result;
            Debug.LogWarning("Testing");
            // Create the scene
            Scene scene = new Scene("My test scene");

            GameObject cam = new GameObject("Cam");
            cam.Scene = scene;
                        
            GameObject monkey = Assets.LoadAsync<GameObject>("Monkey.fbx").Result;
            monkey.Scene = scene;
            monkey.LocalPosition = new Vector3F(0f, 0f, -5f);
            monkey.AddComponent(new TestScript());


            Material mat = assets.LoadAsync<Material>("DefaultAssets/PbrMaterial.json").Result;
            mat.MainTexture = assets.LoadAsync<Texture>("wall.s3dasset").Result;


            object obj = assets.LoadAsync("Shader.s3dasset").Result;
            //mat.MainTexture = null;

            monkey.GetComponent<MeshRenderer>(true).SetMaterial(mat, 1);

            GameObject physicsCube = assets.LoadAsync<GameObject>("TestCube.fbx").Result; //GameObject.PrimitiveCube(graphics, new Vector3F(2f));
            physicsCube.GetComponent<MeshRenderer>(true).SetMaterial(mat);
            physicsCube.AddComponent<BoxCollider>();
            physicsCube.AddComponent<RigidBody>();
            physicsCube.LocalPosition = new Vector3F(0f, 5f, -10f);
            physicsCube.Scene = scene;

            Debug.LogException(new Exception("Test exception"));

            cam.AddComponent(new Camera());

            ChangeScene(scene);
        }

        internal void DoUpdate()
        {
            // Calculate frame time
            ulong currentTime = SDL3.SDL_GetPerformanceCounter();
            float deltaTime = (float)(currentTime - lastFrameTime) / performanceFrequency;
            lastFrameTime = currentTime;

            // Update time system
            Time.UpdateTime(deltaTime);

            // Update physics
            physics.Step();

            if (input.GetKeyDown(Key.Space) == true)
                audio.PlayOnce(clip);

            // Render current scene
            if(scene != null)
            {
                // Update all objects
                scene.Update();

                // Render all cameras
                foreach(Camera camera in scene.activeCameras)
                {
                    // Render the camera
                    camera.Render();
                }
            }

            // Update input
            input.UpdateInputStates();

            // Update title
            screen.Title = "Fps = " + Time.FPS.ToString("F2");

            // Destroy elements at the end of the frame
            while (destroyElements.Count > 0)
                GameElement.DestroyImmediate(destroyElements.Dequeue());
        }

        internal void DoShutdown()
        {
            // Unload assets
            assets.UnloadAll();

            // Shutdown debug
            Debug.Terminate();

            // Quit ttf
            SDL3_ttf.TTF_Quit();

            // Quit mixer
            SDL3_mixer.MIX_Quit();

            // Quit SDL
            SDL3.SDL_Quit();
        }

        internal void DoEvent(in SDL_Event evt)
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
                        input.DoMouseMove(evt.motion.x, evt.motion.y);
                        break;
                    }
                case SDL_EventType.SDL_EVENT_MOUSE_WHEEL:
                    {
                        break;
                    }
                case SDL_EventType.SDL_EVENT_MOUSE_BUTTON_UP:
                case SDL_EventType.SDL_EVENT_MOUSE_BUTTON_DOWN:
                    {
                        input.DoMouseButtonEvent((MouseButton)evt.button.Button, evt.button.down);
                        break;
                    }
                case SDL_EventType.SDL_EVENT_KEY_UP:
                case SDL_EventType.SDL_EVENT_KEY_DOWN:
                    {
                        input.DoKeyboardButtonEvent((Key)evt.key.key, evt.key.down);
                        break;
                    }

                case SDL_EventType.SDL_EVENT_GAMEPAD_ADDED:
                case SDL_EventType.SDL_EVENT_GAMEPAD_REMOVED:
                    {
                        input.DoControllerAvailabilityEvent((int)evt.gdevice.which, evt.Type == SDL_EventType.SDL_EVENT_GAMEPAD_ADDED);
                        break;
                    }
                case SDL_EventType.SDL_EVENT_GAMEPAD_BUTTON_UP:
                case SDL_EventType.SDL_EVENT_GAMEPAD_BUTTON_DOWN:
                    {
                        input.DoControllerButtonEvent((int)evt.gbutton.which, (ControllerButton)evt.gbutton.button, evt.gbutton.down);
                        break;
                    }
                case SDL_EventType.SDL_EVENT_GAMEPAD_AXIS_MOTION:
                    {
                        // Get the value
                        int axisValue = evt.gaxis.value;

                        // Remap to float
                        float remappedAxisValue = -1f + (axisValue - -InputProvider.ControllerAxisRange) * (1f - -1f) / (InputProvider.ControllerAxisRange - -InputProvider.ControllerAxisRange);

                        input.DoControllerAxisEvent((int)evt.gaxis.which, (ControllerAxis)evt.gaxis.axis, remappedAxisValue);
                        break;
                    }
                
            }
        }

        internal void DestroyDelayed(GameElement element)
        {
            // Will be destroyed at the end of the frame
            destroyElements.Enqueue(element);
        }
    }
}
