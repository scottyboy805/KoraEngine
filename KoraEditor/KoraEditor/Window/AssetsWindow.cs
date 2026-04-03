using KoraEditor.UI;
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
            public bool IsFolder;
            public List<AssetNode> Children = new();
        }

        // Private
        private static readonly string[] detailColumnNames = { "Name", "Type", "Size", "Modified" };

        private Texture folderNormalIcon;
        private Texture folderOpenIcon;

        private AssetNode rootAssetNode = new AssetNode { Name = "Assets", IsFolder = true };
        private float previewSize = 0.5f;

        // Constructor
        public AssetsWindow()
        {
            Title = "Assets";
        }

        // Methods
        protected async override void OnOpen()
        {
            folderNormalIcon = await EditorAssets.LoadAsync<Texture>("Icon/FolderNormal.png");
            folderOpenIcon = await EditorAssets.LoadAsync<Texture>("Icon/FolderOpen.png");

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
            // Display preview area
            Gui.BeginTableLayout(1);
            {
                OnProjectViewHeaderGui();

                Gui.BeginTableLayout(2);
                {
                    // Display project tree
                    OnProjectTreeGui();

                    // H separator
                    Gui.ColumnSeparator();

                

                    // Check grid size
                    if(previewSize <= 0)
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

        private void OnProjectViewHeaderGui()
        {
            Gui.BeginLayout(GuiLayout.Horizontal);
            {
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
                    ? folderNormalIcon : folderOpenIcon;

            GuiTreeOptions options = 0;

            if (isFolder == false)
                options |= GuiTreeOptions.NoArrow;

            // Display the node
            if (Gui.BeginTreeNode(node.Name, options, icon) == true)
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

                // Check for folder
                bool isFolder = AssetDatabase.IsFolder(assetPath);

                // Split into parts
                string[] parts = assetPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                AssetNode parent = rootAssetNode;

                for (int i = 0; i < parts.Length; i++)
                {
                    string part = parts[i];
                    // Intermediate parts are folders. The last part is a folder only if AssetDatabase reports it as such.
                    bool partIsFolder = (i < parts.Length - 1) || isFolder;

                    // Try to find existing child
                    AssetNode child = parent.Children.FirstOrDefault(c => string.Equals(c.Name, part, StringComparison.OrdinalIgnoreCase)
                        && c.IsFolder == partIsFolder);

                    // Create if missing
                    if (child == null)
                    {
                        child = new AssetNode { Name = part, IsFolder = partIsFolder };
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

        [Menu("Window/Assets")]
        public static void Open()
        {
            Open<AssetsWindow>();
        }
    }
}
