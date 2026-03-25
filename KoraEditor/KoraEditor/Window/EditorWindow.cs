using ImGuiNET;
using KoraGame;
using System.Numerics;

namespace KoraEditor
{
    public abstract class EditorWindow : EditorContext
    {
        // Internal
        internal static readonly List<EditorWindow> openWindows = new();
        internal bool repaint = false;

        // Private
        private string title = "";
        private Vector2F position;
        private Vector2F size;        

        // Properties
        public string Title
        {
            get => title;
            set => title = value;
        }

        public Vector2F Position
        {
            get => position;
            set => position = value;
        }

        public Vector2F Size
        {   
            get => size;
            set => size = value;
        }

        public virtual bool AllowMultipleWindows => false;

        // Methods
        internal void OnWindowGui()
        {
            // Clear repaint flag
            repaint = false;

            ImGui.SetNextWindowPos((Vector2)position, ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSize((Vector2)size, ImGuiCond.FirstUseEver);

            // Get display title
            string displayTitle = string.IsNullOrEmpty(title) == true
                ? GetType().Name
                : title;

            // Begin the window
            ImGui.Begin(displayTitle, ImGuiWindowFlags.None);
            {
                // Update state
                position = (Vector2F)ImGui.GetWindowPos();
                size = (Vector2F)ImGui.GetWindowSize();

                OnGui();
            }
            ImGui.End();
        }

        public void Close()
        {
            if (openWindows.Contains(this) == true)
            {
                openWindows.Remove(this);

                // Do close
                try
                {
                    OnClose();
                }
                catch(Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        public void Repaint()
        {
            repaint = true;
        }

        protected virtual void OnOpen() { }
        protected virtual void OnClose() { }

        protected virtual void OnGui()
        {
            ImGui.Text("Implement EditorWindow.OnGui to display content here");
        }

        public static bool IsOpen<T>() where T : EditorWindow
        {
            return openWindows.OfType<T>().Any();
        }

        public static T Open<T>() where T : EditorWindow, new()
        {
            // Check for already open
            T window = openWindows.OfType<T>().FirstOrDefault();

            // Get the open instance
            if (window != null && window.AllowMultipleWindows == false)
                return window;

            // Create a new window
            window = new T();
            openWindows.Add(window);

            // Do window open
            try
            {
                window.OnOpen();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return window;
        }
    }
}
