using KoraGame;
using KoraGame.Assets;
using System.Collections.Concurrent;
using System.Reflection;

namespace KoraPipeline
{
    internal class AssetBuildContext
    {
        // Private
        private readonly Random random;
        private readonly ConcurrentDictionary<string, ThreadLocal<AssetImporter>> assetImporters = new();
        private readonly ConcurrentDictionary<string, Task<AssetBuildInfo>> assetBuildTasks = new();
        private readonly ConcurrentBag<AssetBuildInfo> assetsBuilt = new();
        private readonly ConcurrentBag<ulong> assetIds = new();

        private string assetsDirectory;
        private string outputDirectory;

        // Properties
        public string AssetsDirectory => assetsDirectory;
        public string OutputDirectory => outputDirectory;

        public IEnumerable<AssetBuildInfo> AssetsBuilt => assetsBuilt;

        // Constructor
        public AssetBuildContext(string assetsDirectory, string outputDirectory)
        {
            // Use input location as seed to get deterministic results
            this.random = new Random(assetsDirectory.GetHashCode());

            this.assetsDirectory = assetsDirectory;
            this.outputDirectory = outputDirectory;

            // Create output directory
            if (Directory.Exists(outputDirectory) == false)
                Directory.CreateDirectory(outputDirectory);

            // Initialize writers
            InitializeAssetWriters();
        }

        // Methods
        public async Task BuildAssetsAsync()
        {
            List<Task<GameElement>> loadTasks = new();

            // Load all assets first
            foreach (string assetPath in Directory.GetFiles(assetsDirectory, "*.*", SearchOption.AllDirectories))
            {
                // Build the asset
                AssetBuildInfo buildInfo = await BuildAssetAsync(assetPath);

                // Copy to output?
            }

            // Wait for all loaded
            await Task.WhenAll(loadTasks);
        }

        public async Task<AssetBuildInfo> BuildAssetAsync(string assetPath, CancellationToken cancellationToken = default)
        {
            // Register asset build
            if (assetBuildTasks.TryGetValue(assetPath, out Task<AssetBuildInfo> buildTask) == false)
            {
                // Start building the asset
                buildTask = BuildAssetOnDiskAsync(assetPath, cancellationToken);
            }

            // Register the build
            assetBuildTasks[assetPath] = buildTask;

            // Wait for completion
            await buildTask;

            // Add build result
            if(buildTask.IsCompletedSuccessfully == true && buildTask.Result != null)
                assetsBuilt.Add(buildTask.Result);

            // Get the result
            return buildTask.Result;
        }

        private async Task<AssetBuildInfo> BuildAssetOnDiskAsync(string assetPath, CancellationToken cancellationToken)
        {
            // Get extension
            string extension = Path.GetExtension(assetPath);

            // Try to get the writer
            if(assetImporters.TryGetValue(extension.ToLower(), out ThreadLocal<AssetImporter> threadImporter) == false)
            {
                Debug.LogWarning($"Asset extension: '{extension}' is not recognized as an importable asset", LogFilter.Assets);
                return null;
            }

            // Get the relative path
            string assetRelativePath = Path.GetRelativePath(assetsDirectory, assetPath);

            // Get the output location
            string assetOutputPath = Path.Combine(outputDirectory, Path.ChangeExtension(assetRelativePath, KoraAssetReader.AssetExtension));

            try
            {
                // Get importer
                AssetImporter importer = threadImporter.Value;

                // Read the file
                using (Stream importStream = File.OpenRead(assetPath))
                {
                    // Create the context
                    AssetImportContext context = new AssetImportContext(this, assetPath);

                    // Import the asset
                     await importer.ImportAssetAsync(context, importStream, cancellationToken);

                    // Write the file
                    using (Stream outputStream = File.Create(assetOutputPath))
                    {
                        // Create the packed writer
                        KoraPackedWriter packedWriter = new KoraPackedWriter(importer);

                        // Write the content and wait for result
                        await packedWriter.BuildAssetAsync(context, outputStream, cancellationToken);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            // Generate the asset id
            ulong assetId = GenerateAssetId();

            // Create the build info
            AssetBuildInfo buildInfo = new AssetBuildInfo(
                assetOutputPath, 
                //assetMetadata,
                assetId);

            return buildInfo;
        }

        private ulong GenerateAssetId()
        {
            ulong value = 0;
            do
            {
                // Get next id value
                value = (ulong)random.NextInt64();
            }
            while (assetIds.Contains(value) == true);

            return value;
        }

        private void InitializeAssetWriters()
        {
            // Get this assembly name
            AssemblyName thisAssembly = Assembly.GetExecutingAssembly().GetName();

            try
            {
                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    // Check if we are scanning an external assembly
                    if (assembly != Assembly.GetExecutingAssembly())
                    {
                        // Check if the assembly references sharpity
                        AssemblyName[] referenceNames = assembly.GetReferencedAssemblies();
                        bool referenced = false;

                        foreach (AssemblyName assemblyName in referenceNames)
                        {
                            if (thisAssembly.FullName == assemblyName.FullName)
                            {
                                referenced = true;
                                break;
                            }
                        }

                        // Check for referenced
                        if (referenced == false)
                            continue;
                    }

                    foreach (Type type in assembly.GetTypes())
                    {
                        foreach (AssetImporterForAttribute attrib in type.GetCustomAttributes<AssetImporterForAttribute>())
                        {
                            // Check for derived type
                            if (typeof(AssetImporter).IsAssignableFrom(type) == false)
                            {
                                Debug.LogError($"Asset importer must derive from {typeof(AssetImporter)}: {type}", LogFilter.Assets);
                                continue;
                            }

                            // Check for overwrite content reader
                            if (assetImporters.ContainsKey(attrib.Extension) == true)
                            {
                                Debug.LogError($"An asset importer already exists for extension: `{attrib.Extension}`- {type}", LogFilter.Assets);
                                continue;
                            }

                            // Store reader type
                            assetImporters[attrib.Extension] = new ThreadLocal<AssetImporter>(() => (AssetImporter)Activator.CreateInstance(type));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e, LogFilter.Assets);
            }
        }

        public string GetTypeId(Type type)
        {
            // Check for S2d - just use the type name only
            if (type.Assembly == typeof(Game).Assembly)
                return type.FullName;

            // Else use the name and assembly
            return string.Concat(type.FullName, ", ", type.Assembly.GetName().Name);
        }

        public Type ResolveType(string typeId)
        {
            throw new NotSupportedException("Not supported during build");
        }
    }
}
