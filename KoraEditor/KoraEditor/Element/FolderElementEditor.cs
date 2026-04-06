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
        private Texture folderEmptyIcon;

        private EditorSerializedProperty pathElement;
        private string folderPath = "";
        private int subfolderCount = 0;

        // Methods
        protected async override void OnCreate()
        {
            // Get the folder
            pathElement = Layout.FindProperty(nameof(EditorFolder.FolderPath));
            folderPath = pathElement.GetValue<string>();

            // Load icons
            folderNormalIcon = await EditorAssets.LoadAsync<Texture>("Icon/FolderNormal.png");
            folderEmptyIcon = await EditorAssets.LoadAsync<Texture>("Icon/FolderEmpty.png");
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
                Gui.Label(folderPath);
            }
            Gui.EndLayout();
        }
    }
}
