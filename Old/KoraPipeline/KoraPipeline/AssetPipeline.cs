
namespace KoraPipeline
{
    public static class AssetPipeline
    {
        // Methods
        public static async Task BuildAssetsAsync(string assetsDirectory, string outputDirectory)
        {
            // Create the context
            AssetBuildContext context = new AssetBuildContext(assetsDirectory, outputDirectory);

            // Create build tasks
            List<Task> buildTasks = new();

            // Get all assets in directory
            foreach (string assetPath in Directory.GetFiles(assetsDirectory, "*.*", SearchOption.AllDirectories))
            {
                // Build the asset
                buildTasks.Add(context.BuildAssetAsync(assetPath));
            }

            // Wait for all
            await Task.WhenAll(buildTasks);
        }

        public static Task BuildAssetAsync(string assetPath, string outputDirectory, string assetsDirectory = null)
        {
            // Get assets directory
            if (string.IsNullOrEmpty(assetsDirectory) == true)
                assetsDirectory = Directory.GetParent(assetPath).FullName;

            // Create the context
            AssetBuildContext context = new AssetBuildContext(assetsDirectory, outputDirectory);

            // Build the asset
            return context.BuildAssetAsync(assetPath);
        }
    }
}
