using KoraEditor.UI;
using KoraGame;
using KoraGame.Graphics;
using KoraPipeline;
using SDL;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("KoraEditor-Windows")]

namespace KoraEditor
{
    public sealed class Editor : Game
    {
        // Events
        public event Action OnProjectChanged;
        public event Action OnProjectOpened;
        public event Action OnProjectClosed;

        public event Action OnScenesChanged;
        public event Action OnSceneOpened;
        public event Action OnSceneClosed;

        // Private
        private Project project = null;
        private Selection selection = new();        
        private AssetDatabase assetDatabase = null;
        private Screen editorScreen = null;
        private GraphicsDevice editorGraphics = null;
        private EditorScene editorScene = null;

        private AssetProvider editorAssets = null;
        private ImGuiContext gui = null;
        private Menu menuBar = new();
        private bool isPlaying = false;

        // Properties
        public Project Project => project;
        public Selection Selection => selection;
        public AssetDatabase AssetDatabase => assetDatabase;
        public Screen EditorScreen => editorScreen;
        public GraphicsDevice GraphicsDevice => editorGraphics;
        public EditorScene EditorScene => editorScene;
        public AssetProvider EditorAssets => editorAssets;
        internal ImGuiContext Gui => gui;

        // Properties
        internal static Editor EditorInstance => Instance as Editor;

        public override bool IsEditor => true;
        public override bool IsPlaying => isPlaying;

        public string EditorBasePath
        {
            get
            {
#if DEBUG
                return Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "../../../../"));
#else
                // Get path next to executable
                return Environment.CurrentDirectory;
#endif
            }
        }

        public string EditorAssetsPath
        {
            get
            {
                return Path.Combine(EditorBasePath, "Assets");
            }
        }

        public bool IsProjectOpen => project != null;
        public bool IsSceneOpen => editorScene != null;

        // Methods
        internal override void DoInitialize()
        {
            base.DoInitialize();

            // Create scripting
            Debug.Log("Initialize scripting", LogFilter.Script);
            this.scriptable = new ScriptableProvider();

            // Create the screen
            Debug.Log("Initialize graphics", LogFilter.Graphics);
            this.editorScreen = new Screen("KoraEditor", 1920, 1080, false);

            Debug.Log($"Use screen resolution: '{editorScreen.Width} x {editorScreen.Height}', FullScreen = '{editorScreen.Fullscreen}'", LogFilter.Graphics);

            // Create graphics            
            this.editorGraphics = new GraphicsDevice(this.editorScreen);

            Debug.Log($"Use graphics API: '{editorGraphics.GetDeviceDriverName()}'", LogFilter.Graphics);

            // Create assets
            Debug.Log($"Initialize assets", LogFilter.Assets);
            this.editorAssets = new AssetProvider(scriptable, editorGraphics, EditorAssetsPath, false);

            Debug.Log($"Use assets directory: '{editorAssets.AssetDirectory}'", LogFilter.Assets);

            // Ensure SDL text input is enabled so we receive SDL_TEXTINPUT events
            unsafe
            {
                SDL3.SDL_StartTextInput(editorScreen.sdlWindow);
            }

            // Initialize the game layer
            this.assets = assetDatabase;
            this.graphics = editorGraphics;

            // Init gui
            gui = new ImGuiContext();
            gui.Initialize(editorGraphics, editorAssets);


            // Init menu
            menuBar.RebuildMenu();


            // Init editors
            PropertyEditor.InitializePropertyEditors();
            ElementEditor.InitializePropertyEditors();
            //EditorWindow.Open<ConsoleWindow>();

            // For testing
            OpenProject("../../../../../ExampleProject/ExampleProject.koragame");


            AssetsWindow.Open();
            HierarchyWindow.Open();
            PropertiesWindow.Open();
            GameWindow.Open();

            

            
        }

        internal override void DoUpdate()
        {
            base.DoUpdate();

            // Render the editor
            DoRender();
        }

        internal void DoRender()
        {
            gui.BeginFrame();

            // Render menu
            menuBar.DisplayMenu();

            // Render windows
            foreach (EditorWindow window in EditorWindow.openWindows)
                window.OnWindowGui();

            gui.EndFrame();
        }

        internal override void DoShutdown()
        {
            base.DoShutdown();
        }

        internal override void DoEvent(in SDL_Event evt)
        {
            base.DoEvent(evt);

            // Update gui
            gui.HandleSDLEvent(evt);
        }

        // Project
        public Task<Texture> LoadEditorIconAsync(string iconPath)
        {
            return editorAssets.LoadAsync<Texture>(iconPath);
        }

        public async Task<Texture> LoadEditorIconAsync(GameElement element)
        {
            // Check for null
            if (element == null)
                return null;

            // Check for attribute
            EditorIconAttribute attribute = element.elementType.GetCustomAttribute<EditorIconAttribute>();

            // Check for any
            if (attribute == null)
                return null;

            // Get load path
            string loadPath = attribute.Path;

            // Check for any
            if (string.IsNullOrWhiteSpace(loadPath) == true)
                return null;

            // Select location - built in components load from editor and user components load from project
            AssetProvider provider = element.elementType.Assembly == typeof(Game).Assembly
                ? editorAssets : assetDatabase;

            // Load the asset
            return await provider.LoadAsync<Texture>(loadPath);
        }

        #region IODialog
        public static bool ShowSaveFileDialog(out string fileName, string filter, string directory = null)
        {
            bool done = false;
            string file = null;

            // Run dialog on a background thread
            ThreadPool.QueueUserWorkItem((obj) =>
            {
                FileDialog.ShowSaveFileDialog((filename) =>
                {
                    done = true;
                    file = filename;
                }, filter, directory);
            });

            // Wait for done
            while (done == false)
                Thread.Sleep(10);

            // Get result
            fileName = file;
            return string.IsNullOrEmpty(file) == false;
        }

        public static bool ShowOpenFileDialog(out string fileName, string filter, string directory = null)
        {
            bool done = false;
            string[] files = null;

            // Run dialog on a background thread
            ThreadPool.QueueUserWorkItem((obj) =>
            {
                FileDialog.ShowOpenFileDialog((filenames) =>
                {
                    done = true;
                    files = filenames;
                }, filter, directory, false);
            });

            // Wait for done
            while (done == false)
                Thread.Sleep(10);

            // Get result
            fileName = files != null && files.Length > 0 ? files[0] : null;
            return string.IsNullOrEmpty(fileName) == false;
        }

        public static bool ShowOpenFilesDialog(out string[] fileNames, string filter, string directory = null)
        {
            bool done = false;
            string[] files = null;

            // Run dialog on a background thread
            ThreadPool.QueueUserWorkItem((obj) =>
            {
                FileDialog.ShowOpenFileDialog((filenames) =>
                {
                    done = true;
                    files = filenames;
                }, filter, directory, true);
            });

            // Wait for done
            while (done == false)
                Thread.Sleep(10);

            // Get result
            fileNames = files;
            return files != null && files.Length > 0;
        }

        public static bool ShowOpenFolderDialog(out string folderName, string title, string directory = null)
        {
            bool done = false;
            string[] folders = null;

            // Run dialog on a background thread
            ThreadPool.QueueUserWorkItem((obj) =>
            {
                FileDialog.ShowOpenFolderDialog((filenames) =>
                {
                    done = true;
                    folders = filenames;
                }, directory, false);
            });

            // Wait for done
            while (done == false)
                Thread.Sleep(10);

            // Get result
            folderName = folders != null && folders.Length > 0 ? folders[0] : null;
            return string.IsNullOrEmpty(folderName) == false;
        }

        public static bool ShowOpenFoldersDialog(out string[] folderNames, string filter, string directory = null)
        {
            bool done = false;
            string[] folders = null;

            // Run dialog on a background thread
            ThreadPool.QueueUserWorkItem((obj) =>
            {
                FileDialog.ShowOpenFolderDialog((filenames) =>
                {
                    done = true;
                    folders = filenames;
                }, directory, true);
            });

            // Wait for done
            while (done == false)
                Thread.Sleep(10);

            // Get result
            folderNames = folders;
            return folders != null && folders.Length > 0;
        }
        #endregion

        #region PopupDialog
        public static bool ShowPopupDialog(string title, string text, string confirm)
        {
            // Create single button
            PopupDialog.DialogButton[] buttons =
            {
                new PopupDialog.DialogButton
                {
                    Text = confirm,
                    Flags = SDL_MessageBoxButtonFlags.SDL_MESSAGEBOX_BUTTON_RETURNKEY_DEFAULT,
                }
            };

            // Show dialog
            return PopupDialog.ShowPopupDialog(title, text, 0, buttons) == 0;
        }

        public static bool ShowPopupDialog(string title, string text, string confirm, string cancel)
        {
            // Create 2 buttons
            PopupDialog.DialogButton[] buttons =
            {
                new PopupDialog.DialogButton
                {
                    Text = confirm,
                    Flags = SDL_MessageBoxButtonFlags.SDL_MESSAGEBOX_BUTTON_RETURNKEY_DEFAULT,
                },
                new PopupDialog.DialogButton
                {
                    Text = cancel,
                    Flags = SDL_MessageBoxButtonFlags.SDL_MESSAGEBOX_BUTTON_ESCAPEKEY_DEFAULT,
                }
            };

            // Show dialog
            return PopupDialog.ShowPopupDialog(title, text, 0, buttons) == 0;
        }

        public static bool ShowPopupDialog(string title, string text, string yes, string no, string cancel)
        {
            // Create 2 buttons
            PopupDialog.DialogButton[] buttons =
            {
                new PopupDialog.DialogButton
                {
                    Text = yes,
                    Flags = SDL_MessageBoxButtonFlags.SDL_MESSAGEBOX_BUTTON_RETURNKEY_DEFAULT,
                },
                new PopupDialog.DialogButton
                {
                    Text = no,
                    Flags = 0,
                },
                new PopupDialog.DialogButton
                {
                    Text = cancel,
                    Flags = SDL_MessageBoxButtonFlags.SDL_MESSAGEBOX_BUTTON_ESCAPEKEY_DEFAULT,
                }
            };

            // Show dialog
            return PopupDialog.ShowPopupDialog(title, text, 0, buttons) == 0;
        }
        #endregion


        public void NewProject(string projectPath)
        {            
            // Close current project
            CloseProject();

            // Create project
            project = new Project(projectPath, false);
            project.Save();

            // Open the project
            OpenProject(projectPath);
        }        

        public void OpenProject(string projectPath)
        {
            // Try to open
            Project openProject = new Project(projectPath, true);

            // Close current project
            CloseProject();

            Debug.Log($"Open project: {openProject.ProjectPath}", LogFilter.Editor);

            // Load new project
            project = openProject;
            project.Load();

            // Init assets
            assets = assetDatabase = new AssetDatabase(scriptable, graphics, project.AssetsFolder, false);

            // Refresh assets
            assetDatabase.Refresh();

            // Open scene
            if (IsSceneOpen == false)
                NewScene();

            // Update title
            EditorScreen.Title = $"KoraEditor ({project.Name})";
            
            // Do events
            DoEvent(OnProjectChanged);
            DoEvent(OnProjectOpened);
        }

        public void CloseProject()
        {
            if (project != null)
            {
                // Save and close
                project.Save();
                project = null;

                // Clear assets
                assets = null;
                assetDatabase = null;

                // Update title
                EditorScreen.Title = "Kora Editor";

                // Do events
                DoEvent(OnProjectChanged);
                DoEvent(OnProjectClosed);
            }
        }

        public void NewScene()
        {
            CloseScene();
            editorScene = new EditorScene("New Scene");

            editorScene.CreateDefault();
            editorScene.Activate();

            // Do events
            DoEvent(OnScenesChanged);
            DoEvent(OnSceneOpened);
        }

        public void OpenScene(string scenePath)
        {
            CloseScene();

            // Try to load the scene
            editorScene = assetDatabase.LoadAsync<EditorScene>(scenePath).Result;

            // Do events
            DoEvent(OnScenesChanged);
            DoEvent(OnSceneOpened);
        }

        public void SaveScene()
        {
            if (IsSceneOpen == true && assetDatabase.IsAssetDirty(scene) == true)
                ;// TODO
        }

        private void CloseScene()
        {
            // Save changes
            SaveScene();

            // Do events
            DoEvent(OnScenesChanged);
            DoEvent(OnSceneOpened);
        }

        #region MenuActions_File
        [Menu("File/New Project")]
        internal static void NewProjectAction()
        {
            if (ShowSaveFileDialog(out string projectPath, Project.FileExtension.TrimStart('.')) == true)
            {
                // Create the project
                EditorInstance?.NewProject(projectPath);
            }
        }

        [Menu("File/Open Project", "Ctrl+O")]
        internal static void OpenProjectAction()
        {
            if (ShowOpenFileDialog(out string projectPath, Project.FileExtension.TrimStart('.')) == true)
            {
                // Open the project
                EditorInstance?.OpenProject(projectPath);
            }
        }

        [Menu("File/Close Project")]
        internal static void CloseProjectAction()
        {
            EditorInstance?.CloseProject();
        }

        [Menu("File/New Scene", "", true)]
        internal static void NewSceneAction()
        {
            EditorInstance?.NewScene();
        }

        [Menu("File/Open Scene")]
        internal static void OpenSceneAction()
        {
            if (EditorInstance?.IsProjectOpen == true && ShowOpenFileDialog(out string scenePath, "kscene", EditorInstance.project.AssetsFolder) == true)
            {
                // Open the scene
                EditorInstance?.OpenScene(scenePath);
            }
        }

        [Menu("File/Save Scene")]
        internal static void SaveSceneAction()
        {
            EditorInstance?.SaveScene();
        }

        [Menu("File/Quit", "", true)]
        internal static void ExitAction()
        {
            EditorInstance?.quit = true;
        }
        #endregion
    }
}
