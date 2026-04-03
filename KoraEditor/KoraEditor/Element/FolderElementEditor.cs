using KoraEditor.UI;
using KoraGame;
using KoraGame.Graphics;

namespace KoraEditor
{
    [ElementEditorFor(typeof(EditorFolder))]
    internal sealed class FolderElementEditor : ElementEditor
    {
        // Private
        private Texture folderNormalIcon;
        private Texture folderOpenIcon;

        private EditorSerializedProperty pathElement;

        // Methods
        protected async override void OnCreate()
        {
            folderNormalIcon = await EditorAssets.LoadAsync<Texture>("Icon/FolderNormal.png");
            folderOpenIcon = await EditorAssets.LoadAsync<Texture>("Icon/FolderOpen.png");

            // Get the folder
            pathElement = Layout.FindProperty(nameof(EditorFolder.FolderPath));
        }

        protected override void OnGui()
        {
            // Select icon
            Texture icon = folderNormalIcon;

            // Display folder path
            Gui.BeginLayout(GuiLayoutOptions.Horizontal);
            {
                // Icon
                Gui.Image(icon, new Vector2F(32, 32));

                // Folder name
                Gui.Label(pathElement.GetValue<string>());
            }
            Gui.EndLayout();
        }
    }
}
