using KoraEditor.UI;
using KoraGame;
using KoraGame.Graphics;

namespace KoraEditor
{
    public sealed class AssetsWindow : EditorWindow
    {
        // Type
        private class AssetNode
        {
            // Public
            public string Name;
            public string Path;
            public bool IsFolder;
            public List<AssetNode> Children = new();
        }

        // Private
        private static readonly string[] detailColumnNames = { "Name", "Type", "Size", "Modified" };

        private Texture navigateBack;
        private Texture navigateForward;
        private Texture navigateUp;

        private Texture folderNormalIcon;
        private Texture folderEmptyIcon;

        private AssetNode rootAssetNode = new AssetNode { Name = "Assets", Path = "Assets", IsFolder = true };
        private float previewSize = 0.5f;

        // Public
        public const int iconSize = 24;

        // Constructor
        public AssetsWindow()
        {
            Title = "Assets";
        }

        // Methods
        protected async override void OnOpen()
        {
            navigateBack = await Editor.LoadEditorIconAsync("Icon/Left.png");
            navigateForward = await Editor.LoadEditorIconAsync("Icon/Right.png");
            navigateUp = await Editor.LoadEditorIconAsync("Icon/Up.png");

            folderNormalIcon = await Editor.LoadEditorIconAsync("Icon/FolderNormal.png");
            folderEmptyIcon = await Editor.LoadEditorIconAsync("Icon/FolderEmpty.png");

            // Rebuid the tree
            RebuildAssetTree();

            // Listen for events
            Editor.OnProjectChanged += RebuildAssetTree;
        }

        protected override void OnClose()
        {
            // Remove events
            Editor.OnProjectChanged -= RebuildAssetTree;
        }

        protected override void OnGui()
        {
            //OnProjectViewHeaderGui();
            // Display preview area
            Gui.BeginTableLayout(1);
            {
                

                Gui.BeginTableLayout(2);
                {
                    // Display project tree
                    OnProjectTreeGui();

                    // H separator
                    Gui.ColumnSeparator();

                    OnProjectViewHeaderGui();

                    // Check grid size
                    if (previewSize <= 0)
                    {
                        OnProjectViewDetailsGui();
                    }
                    else
                    {
                        OnProjectViewGridGui();
                    }                
                }
                Gui.EndTableLayout();

            }
            Gui.EndTableLayout();
        }

        private void OnProjectTreeGui()
        {
            if (Editor.IsProjectOpen == true)
            {
                OnAssetTreeGui(rootAssetNode);
            }
        }

        bool on;
        private void OnProjectViewHeaderGui()
        {
            Gui.BeginLayout(GuiLayoutOptions.Horizontal | GuiLayoutOptions.Frame | GuiLayoutOptions.Empty);
            {
                // Navigate buttons
                Gui.ImageButton(new GuiContent(navigateBack, "Back"), new Vector2F(24f));
                Gui.ImageButton(new GuiContent(navigateForward, "Forward"), new Vector2F(24f));
                Gui.ImageButton(new GuiContent(navigateUp, "Go up a level"), new Vector2F(24f));

                Gui.Button("Path A", () => { });
                Gui.Button("Path B", () => { });
                Gui.Button("Path C", () => { });

                Gui.Slider(ref previewSize, 0f, 1f);
            }
            Gui.EndLayout();
        }

        private void OnProjectViewDetailsGui()
        {
            Gui.BeginTableLayout(detailColumnNames);
            {
                Gui.Label("Name here");
                Gui.ColumnSeparator();
                Gui.Label("Type here");
                Gui.ColumnSeparator();
                Gui.Label("Size here");
                Gui.ColumnSeparator();
                Gui.Label("Modified here");
            }
            Gui.EndTableLayout();
        }

        private void OnProjectViewGridGui()
        {

        }

        private void OnAssetTreeGui(AssetNode node)
        {
            // Select icon
            Texture icon = null;

            // Check for folder
            bool isFolder = node.IsFolder;

            if(isFolder == true)
                icon = node.Children.Count > 0
                    ? folderNormalIcon : folderEmptyIcon;

            GuiTreeOptions options = 0;

            if (isFolder == false)
                options |= GuiTreeOptions.NoArrow;

            // Display the node
            if (Gui.BeginTreeNode(new GuiContent(node.Name, null, icon), options, () =>
            {
                // Select the folder
                Selection.Select(new EditorFolder(node.Path));
                Debug.Log("Path = " + node.Path);
            }) == true)
            {
                // Display children
                foreach(var child in node.Children)
                    OnAssetTreeGui(child);

                // End the node
                Gui.EndTreeNode();
            }
        }

        public void Refresh()
        {
            // Rebuild the tree
            RebuildAssetTree();
        }


        private void RebuildAssetTree()
        {
            // Clear sub nodes
            rootAssetNode.Children.Clear();

            // Check for no project
            if (Editor.IsProjectOpen == false)
                return;

            // Find all folders
            //foreach (string folder in AssetDatabase.SearchFolders())
            //{
            //    RebuildAssetTreeNode(folder, rootAssetNode);
            //}
            RebuildAssetTreeNode("", rootAssetNode);
            return;

            // Search in folder
            IEnumerable<string> assetGuids = AssetDatabase.SearchAssets(null, null, SearchOption.AllDirectories);

            // Process all assets
            foreach (string assetGuid in assetGuids)
            {
                // Get the asset path
                string assetPath = AssetDatabase.GetAssetPath(assetGuid);

                // Skip invalid
                if (string.IsNullOrEmpty(assetPath))
                    continue;

                // Split into parts
                string[] parts = assetPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                AssetNode parent = rootAssetNode;

                for (int i = 0; i < parts.Length; i++)
                {
                    string part = parts[i];
                    // Intermediate parts are folders. The last part is a folder only if AssetDatabase reports it as such.
                    bool isFolder = (i < parts.Length - 1);

                    // Try to find existing child
                    AssetNode child = parent.Children.FirstOrDefault(c => string.Equals(c.Name, part, StringComparison.OrdinalIgnoreCase)
                        && c.IsFolder == isFolder);

                    // Create if missing
                    if (child == null)
                    {
                        string path = "";
                        for(int j = 0; j < i; j++)
                        {
                            path += parts[j];
                            if(j < i - 1) path += "/";
                        }    

                        child = new AssetNode { Name = part, Path = path, IsFolder = isFolder };
                        parent.Children.Add(child);
                    }

                    parent = child;
                }
            }

            // Sort children so folders come first, then files, both alphabetically
            void SortNodes(AssetNode node)
            {
                node.Children.Sort((a, b) =>
                {
                    if (a.IsFolder != b.IsFolder) return a.IsFolder ? -1 : 1;
                    return string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
                });

                foreach (var c in node.Children)
                    SortNodes(c);
            }

            SortNodes(rootAssetNode);
        }

        private void RebuildAssetTreeNode(string searchPath, AssetNode parent)
        {
            // Find all folders
            foreach(string folder in AssetDatabase.SearchFolders(searchPath))
            {
                // Get the folder name
                string folderName = Path.GetFileName(folder);

                // Create a new node
                AssetNode current = new AssetNode
                {
                    Name = folderName,
                    Path = KoraPipeline.AssetDatabase.JoinAssetPath(parent.Path, folderName),
                    IsFolder = true,
                };

                // Add the node
                parent.Children.Add(current);

                // Visit children
                RebuildAssetTreeNode(KoraPipeline.AssetDatabase.JoinAssetPath(searchPath, folderName), current);
            }

            // Find all assets
            foreach (string assetGuid in AssetDatabase.SearchAssets(searchPath))
            {
                // Get asset path
                string assetPath = AssetDatabase.GetAssetPath(assetGuid);

                // Get the asset name
                string assetName = Path.GetFileName(assetPath);

                // Create a new node
                AssetNode leaf = new AssetNode
                {
                    Name = assetName,
                    Path = KoraPipeline.AssetDatabase.JoinAssetPath(parent.Path, assetName),
                    IsFolder = false,
                };

                // Add the node
                parent.Children.Add(leaf);
            }
        }

        [Menu("Window/Assets")]
        public static void Open()
        {
            Open<AssetsWindow>();
        }
    }
}
