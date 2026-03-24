using ImGuiNET;
using Microsoft.Win32;
using KoraGame;
using KoraGame.Graphics;
using SDL;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("KoraEditor-Windows")]

namespace KoraEditor
{
    public sealed class Editor : Game
    {
        // Private
        private Project project = null;
        private AssetProvider editorAssets = null;
        private ImGuiContext gui = null;

        private Menu menuBar = new();

        // Properties
        public AssetProvider EditorAssets => editorAssets;
        internal ImGuiContext Gui => gui;

        // Properties
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

        public string EditorContentPath
        {
            get
            {
                return Path.Combine(EditorBasePath, "Content");
            }
        }

        public bool IsProjectOpen => project != null;

        // Methods
        internal override void DoInitialize()
        {
            base.DoInitialize();



            // Create scripting
            Debug.Log("Initialize scripting", LogFilter.Script);
            this.scriptable = new ScriptableProvider();

            // Create the screen
            Debug.Log("Initialize graphics", LogFilter.Graphics);
            this.screen = new Screen("KoraEditor", 1920, 1080, false);

            Debug.Log($"Use screen resolution: '{screen.Width} x {screen.Height}', FullScreen = '{screen.Fullscreen}'", LogFilter.Graphics);

            // Create graphics            
            this.graphics = new GraphicsDevice(this.screen);

            Debug.Log($"Use graphics API: '{graphics.GetDeviceDriverName()}'", LogFilter.Graphics);

            // Create assets
            Debug.Log($"Initialize assets", LogFilter.Assets);
            this.editorAssets = new AssetProvider(scriptable, graphics, EditorContentPath, false);

            Debug.Log($"Use assets directory: '{editorAssets.AssetDirectory}'", LogFilter.Assets);

            // Ensure SDL text input is enabled so we receive SDL_TEXTINPUT events
            unsafe
            {
                SDL3.SDL_StartTextInput(screen.sdlWindow);
            }

            // Init gui
            gui = new ImGuiContext();
            gui.Initialize(graphics, editorAssets);


            // Init menu
            menuBar.RebuildMenu();

            //EditorWindow.Open<ConsoleWindow>();
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
        //#region IODialog
        //public bool ShowSaveFileDialog(ref string fileName, string title, string filter, string directory = null)
        //{
        //    SaveFileDialog saveFileDialog = new SaveFileDialog();
        //    saveFileDialog.FileName = fileName;
        //    saveFileDialog.Title = title;
        //    saveFileDialog.Filter = filter;

        //    if (directory != null)
        //        saveFileDialog.DefaultDirectory = directory;

        //    // Check for success
        //    if (saveFileDialog.ShowDialog() == true)
        //    {
        //        fileName = saveFileDialog.FileName;
        //        return true;
        //    }
        //    fileName = null;
        //    return false;
        //}

        //public bool ShowOpenFileDialog(ref string fileName, string title, string filter, string directory = null)
        //{
        //    OpenFileDialog openFileDialog = new OpenFileDialog();
        //    openFileDialog.FileName = fileName;
        //    openFileDialog.Title = title;
        //    openFileDialog.Filter = filter;

        //    if (directory != null)
        //        openFileDialog.DefaultDirectory = directory;

        //    // Check for success
        //    if (openFileDialog.ShowDialog() == true)
        //    {
        //        fileName = openFileDialog.FileName;
        //        return true;
        //    }
        //    fileName = null;
        //    return false;
        //}

        //public bool ShowOpenFolderDialog(ref string folderName, string title, string directory = null)
        //{
        //    OpenFolderDialog openFolderDialog = new OpenFolderDialog();
        //    openFolderDialog.FolderName = folderName;
        //    openFolderDialog.Title = title;

        //    if (directory != null)
        //        openFolderDialog.DefaultDirectory = directory;

        //    // Check for success
        //    if (openFolderDialog.ShowDialog() == true)
        //    {
        //        folderName = openFolderDialog.FolderName;
        //        return true;
        //    }
        //    folderName = null;
        //    return false;
        //}
        //#endregion

        public void NewProject()
        {

        }

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

        [Menu("File/Open Project", "Ctrl+O")]
        public void OpenProject()
        {
            Debug.Log("Open project");
        }

        public void OpenProject(string projectPath)
        {
            // Try to open
            Project openProject = new Project(projectPath, true);

            // Close current project
            CloseProject();

            // Load new project
            project = openProject;
            project.Load();

            // Init assets
            assets = new AssetProvider(scriptable, graphics, project.ContentFolder, false);
        }

        [Menu("File/Close Project")]
        public void CloseProject()
        {
            if(project != null)
            {
                // Save and close
                project.Save();
                project = null;

                // Clear assets
                assets = null;
            }
        }
    }
}
