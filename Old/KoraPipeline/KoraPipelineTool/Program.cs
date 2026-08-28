
namespace KoraPipeline
{
    public static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            if (args.Length < 2 || args.Length > 3 || args[0] == "--help" || args[0] == "-h")
            {
                ShowHelp();
                return 1;
            }

            string inputPath = args[0];
            string outputFolder = args[1];
            bool isKpakBuild = args.Length == 3 && args[2].Equals("-kpak", StringComparison.OrdinalIgnoreCase);

            if (!File.Exists(inputPath) && !Directory.Exists(inputPath))
            {
                Console.Error.WriteLine($"Error: Path '{inputPath}' does not exist.");
                return 1;
            }

            if (isKpakBuild && !Directory.Exists(inputPath))
            {
                Console.Error.WriteLine($"Error: -kpak flag requires the input path to be a folder.");
                return 1;
            }

            if (!isKpakBuild && Directory.Exists(inputPath))
            {
                Console.Error.WriteLine($"Error: To build a folder, you must use the -kpak flag.");
                return 1;
            }

            try
            {
                // Ensure output folder exists
                Directory.CreateDirectory(outputFolder);

                if (isKpakBuild)
                {
                    Console.WriteLine($"Building all assets in folder '{inputPath}' into '{outputFolder}'...");
                    await AssetPipeline.BuildAssetsAsync(inputPath, outputFolder);
                    Console.WriteLine("All assets built successfully.");
                }
                else
                {
                    Console.WriteLine($"Building single asset '{inputPath}' into '{outputFolder}'...");
                    await AssetPipeline.BuildAssetAsync(inputPath, outputFolder);
                    Console.WriteLine("Asset built successfully.");
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error building assets: {ex.Message}");
                return 1;
            }
        }

        private static void ShowHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  KoraBuild <assetPath> <outputFolder>");
            Console.WriteLine("  KoraBuild <folderPath> <outputFolder> -kpak");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  KoraBuild wall.png Build/Assets");
            Console.WriteLine("  KoraBuild Assets/ Build/Assets -kpak");
        }
    }
}