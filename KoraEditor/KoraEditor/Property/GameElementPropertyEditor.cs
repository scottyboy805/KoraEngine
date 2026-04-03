using KoraEditor.UI;
using KoraGame;

namespace KoraEditor
{
    [PropertyEditorFor(typeof(GameElement), true)]
    internal sealed class GameElementPropertyEditor : PropertyEditor
    {
        protected override void OnValueGui()
        {
            string text = "Object Field Here";
            Gui.Input(ref text);
        }
    }
}
