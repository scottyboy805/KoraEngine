using KoraGame;
using KoraEditor.UI;
using KoraGame.Graphics;

namespace KoraEditor
{
    public class ConsoleWindow : EditorWindow
    {
        enum MyEnum
        {
            Val1
                , Val2
                ,Val3
        }

        float val = 0.5f;
        MyEnum e = MyEnum.Val2;

        Texture icon;

        // Private
        private Texture infoIcon;
        private Texture warningIcon;
        private Texture errorIcon;

        private string consoleSearch = "";
        private Vector2F consoleFilterIconSize = new Vector2F(32, 32);

        // Methods
        public ConsoleWindow()
        {
            Title = "Console";
            icon = ((Editor)Editor.Instance).EditorAssets.LoadAsync<Texture>("Icon/WGPU-Logo.png").Result;
        }

        protected async override void OnOpen()
        {
            // Load icons
            infoIcon = await EditorAssets.LoadAsync<Texture>("Icon/Info.png");
            warningIcon = await EditorAssets.LoadAsync<Texture>("Icon/Warning.png");
            errorIcon = await EditorAssets.LoadAsync<Texture>("Icon/Error.png");
        }

        protected override void OnGui()
        {

            OnTopBarGui();

            base.OnGui();
            Gui.Label(new GuiContent("Testing label", "This should be a tooltip"));
            if(Gui.Slider(ref val, 0, 1))
                Debug.Log("Changed");


            if (Gui.EnumPopup(ref e))
                Debug.Log("Changed");

            Gui.Image(icon, new Vector2F(500, 500));
        }

        private void OnTopBarGui()
        {
            Gui.BeginLayout(GuiLayout.Horizontal);
            {
                // Clear button
                Gui.Button(new GuiContent("Clear", "Clear all console messages"), Clear);
                Gui.Space();

                // Search input
                Gui.Label("Search:");
                Gui.Input(ref consoleSearch);

                // Filter icons
                Gui.ImageButton(infoIcon, consoleFilterIconSize, ToggleMessageFilter);
                Gui.ImageButton(warningIcon, consoleFilterIconSize, ToggleWarningFilter);
                Gui.ImageButton(errorIcon, consoleFilterIconSize, ToggleErrorFilter);
            }
            Gui.EndLayout();
        }

        [Menu("Window/Console", "Ctrl+Shift+C")]
        public static void Open()
        {
            Open<ConsoleWindow>();
        }

        public void Clear()
        {

        }

        public void ToggleMessageFilter()
        {

        }

        public void ToggleWarningFilter()
        {

        }

        public void ToggleErrorFilter()
        {

        }
    }
}
