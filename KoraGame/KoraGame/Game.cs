using SDL;
using KoraGame.Audio;
using KoraGame.Graphics;
using KoraGame.Input;
using KoraGame.Physics;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("KoraGame-Desktop")]
[assembly: InternalsVisibleTo("KoraEditor")]
[assembly: InternalsVisibleTo("KoraPipeline")]

namespace KoraGame
{
    public abstract class Game
    {
        // Private
        private static Game instance;
        protected readonly Queue<GameElement> destroyElements = new();

        protected bool quit = false;
        protected GameSettings settings = new();
        protected Screen screen = null;
        protected GraphicsDevice graphics = null;
        protected AssetProvider assets = null;
        protected AudioDevice audio = null;
        protected InputProvider input = null;
        protected PhysicsSimulation physics = null;
        protected ScriptableProvider scriptable = null;
        protected Scene scene = null;

        private ulong lastFrameTime = 0;
        private ulong performanceFrequency = 0;        

        // Properties
        internal static Game Instance => instance;        

        public bool Quit => quit;
        public GameSettings Settings => settings;
        public Screen Screen => screen;
        public GraphicsDevice Graphics => graphics;
        public AssetProvider Assets => assets;
        public AudioDevice Audio => audio;
        public InputProvider Input => input;
        public PhysicsSimulation Physics => physics;
        public ScriptableProvider Scriptable => scriptable;
        public Scene Scene => scene;
        

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
        public virtual void ChangeScene(Scene scene)
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

        internal virtual void DoInitialize()
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
        }

        internal virtual void DoUpdate()
        {
            // Calculate frame time
            ulong currentTime = SDL3.SDL_GetPerformanceCounter();
            float deltaTime = (float)(currentTime - lastFrameTime) / performanceFrequency;
            lastFrameTime = currentTime;

            // Update time system
            Time.UpdateTime(deltaTime);
        }

        internal virtual void DoShutdown()
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

        internal virtual void DoEvent(in SDL_Event evt)
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
                        input?.DoMouseMove(evt.motion.x, evt.motion.y);
                        break;
                    }
                case SDL_EventType.SDL_EVENT_MOUSE_WHEEL:
                    {
                        break;
                    }
                case SDL_EventType.SDL_EVENT_MOUSE_BUTTON_UP:
                case SDL_EventType.SDL_EVENT_MOUSE_BUTTON_DOWN:
                    {
                        input?.DoMouseButtonEvent((MouseButton)evt.button.Button, evt.button.down);
                        break;
                    }
                case SDL_EventType.SDL_EVENT_KEY_UP:
                case SDL_EventType.SDL_EVENT_KEY_DOWN:
                    {
                        input?.DoKeyboardButtonEvent((Key)evt.key.key, evt.key.down);
                        break;
                    }

                case SDL_EventType.SDL_EVENT_GAMEPAD_ADDED:
                case SDL_EventType.SDL_EVENT_GAMEPAD_REMOVED:
                    {
                        input?.DoControllerAvailabilityEvent((int)evt.gdevice.which, evt.Type == SDL_EventType.SDL_EVENT_GAMEPAD_ADDED);
                        break;
                    }
                case SDL_EventType.SDL_EVENT_GAMEPAD_BUTTON_UP:
                case SDL_EventType.SDL_EVENT_GAMEPAD_BUTTON_DOWN:
                    {
                        input?.DoControllerButtonEvent((int)evt.gbutton.which, (ControllerButton)evt.gbutton.button, evt.gbutton.down);
                        break;
                    }
                case SDL_EventType.SDL_EVENT_GAMEPAD_AXIS_MOTION:
                    {
                        // Get the value
                        int axisValue = evt.gaxis.value;

                        // Remap to float
                        float remappedAxisValue = -1f + (axisValue - -InputProvider.ControllerAxisRange) * (1f - -1f) / (InputProvider.ControllerAxisRange - -InputProvider.ControllerAxisRange);

                        input?.DoControllerAxisEvent((int)evt.gaxis.which, (ControllerAxis)evt.gaxis.axis, remappedAxisValue);
                        break;
                    }
                
            }
        }

        internal virtual void DestroyDelayed(GameElement element)
        {
            // Will be destroyed at the end of the frame
            destroyElements.Enqueue(element);
        }
    }
}
