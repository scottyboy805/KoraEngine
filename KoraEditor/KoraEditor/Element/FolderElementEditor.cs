using KoraEditor.UI;
using KoraGame;
using KoraGame.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace KoraEditor
{
    [ElementEditorFor(typeof(EditorFolder))]
    internal sealed class FolderElementEditor : ElementEditor
    {
        // Private
        private Texture folderNormalIcon;
        private Texture folderOpenIcon;

        private EditorSerializedElement pathElement;

        // Methods
        protected async override void OnCreate()
        {
            folderNormalIcon = await EditorAssets.LoadAsync<Texture>("Icon/FolderNormal.png");
            folderOpenIcon = await EditorAssets.LoadAsync<Texture>("Icon/FolderOpen.png");

            // Get the folder
            pathElement = Layout.FindElement(nameof(EditorFolder.FolderPath));
        }

        protected override void OnGui()
        {
            // Select icon
            Texture icon = folderNormalIcon;

            // Display folder path
            Gui.BeginLayout(GuiLayout.Horizontal);
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
