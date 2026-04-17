using KoraGame.Audio;
using KoraGame.Graphics;
using KoraGame.Input;
using KoraGame.Physics;
using SDL;

namespace KoraGame
{
    [Serializable]
    internal sealed class GameApp : Game
    {
        // Methods
        internal override void DoInitialize()
        {
            base.DoInitialize();

            // Read configuration
            if (File.Exists("Settings.json") == true)
                SerializedJson.PopulateAsync(File.ReadAllText("Settings.json"), settings).Wait();

            // Create scripting
            Debug.Log("Initialize scripting", LogFilter.Script);
            this.scriptable = new ScriptableProvider();

            // Create the screen
            Debug.Log("Initialize graphics", LogFilter.Graphics);
            this.screen = new Screen(settings.GameName, settings.PreferredScreenWidth, settings.PreferredScreenHeight, settings.Fullscreen);

            Debug.Log($"Use screen resolution: '{screen.Width} x {screen.Height}', FullScreen = '{screen.Fullscreen}'", LogFilter.Graphics);

            // Create graphics            
            this.graphicsDevice = new GraphicsDevice(this.screen);

            Debug.Log($"Use graphics API: '{graphicsDevice.GetDeviceDriverName()}'", LogFilter.Graphics);

            // Create assets
            Debug.Log($"Initialize assets", LogFilter.Assets);
            this.assets = new AssetProvider(scriptable, graphicsDevice, Environment.CurrentDirectory, false);

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



            //clip = AudioClip.LoadWav(audio, "ChangeRim.wav");


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

        internal override void DoUpdate()
        {
            base.DoUpdate();

            // Update physics
            physics.Step();

            //if (input.GetKeyDown(Key.Space) == true)
            //    audio.PlayOnce(clip);

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
            input.UpdateInputStates();

            // Update title
            screen.Title = "Fps = " + Time.FPS.ToString("F2");

            // Destroy elements at the end of the frame
            while (destroyElements.Count > 0)
                GameElement.DestroyImmediate(destroyElements.Dequeue());
        }
    }
}
