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
        // Methods
        public ConsoleWindow()
        {
            icon = ((Editor)Editor.Instance).EditorAssets.LoadAsync<Texture>("Icon/WGPU-Logo.png").Result;
        }

        protected override void OnGui()
        {
            
            base.OnGui();
            Gui.Label(new GuiContent("Testing label", "This should be a tooltip"));
            if(Gui.Slider(ref val, 0, 1))
                Debug.Log("Changed");


            if (Gui.EnumPopup(ref e))
                Debug.Log("Changed");

            Gui.Image(icon, new Vector2F(500, 500));
        }

        [Menu("Window/Console", "Ctrl+Shift+C")]
        public static void Open()
        {
            Open<ConsoleWindow>();
        }
    }
}
