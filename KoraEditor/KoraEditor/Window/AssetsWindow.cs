using KoraEditor.UI;
using KoraGame.Graphics;

namespace KoraEditor
{
    public sealed class AssetsWindow : EditorWindow
    {
        // Private
        private Texture folderNormalIcon;
        private Texture folderOpenIcon;

        // Methods
        protected async override void OnOpen()
        {
            folderNormalIcon = await EditorAssets.LoadAsync<Texture>("Icon/FolderNormal.png");
            folderOpenIcon = await EditorAssets.LoadAsync<Texture>("Icon/FolderOpen.png");
        }

        protected override void OnGui()
        {
            //// Check for open
            //if(Editor.IsProjectOpen == false)
            //{
            //    Gui.Label("No project open");
            //    return;
            //}

            // Display project tree
            OnAssetTreeGui();
        }

        private void OnAssetTreeGui()
        {
            if(Gui.BeginTreeNode("Assets", false, folderNormalIcon))
            {
                if (Gui.BeginTreeNode("Sub", true, folderOpenIcon))
                    Gui.EndTreeNode();


                Gui.EndTreeNode();
            }
            

            //// Display content folder
            //if (Gui.TreeNode(folderNormalIcon, folderOpenIcon, "Content"))
            //{
            //    // Display content
            //    Gui.TreePop();
            //}
            //// Display scripts folder
            //if (Gui.TreeNode(folderNormalIcon, folderOpenIcon, "Scripts"))
            //{
            //    // Display scripts
            //    Gui.TreePop();
            //}
        }

        [Menu("Window/Assets")]
        public static void Open()
        {
            Open<AssetsWindow>();
        }
    }
}
