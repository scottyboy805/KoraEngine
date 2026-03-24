using KoraGame.Assets;
using KoraGame.Graphics;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;

namespace KoraGame
{
    public interface IAssetSerialize
    {
        void OnSerialize();
        void OnDeserialize();
    }

    public sealed class AssetProvider
    {
        // Private
        private const int webDefaultTimeoutSeconds = 5;         // 5 Seconds
        private static readonly HttpClient webClient = new();

        private readonly ScriptableProvider scriptable;
        private readonly GraphicsDevice graphics;
        private readonly string assetDirectory = "";
        private readonly bool useWebRequest = false;
        private readonly ConcurrentDictionary<string, ThreadLocal<IAssetImporter>> assetImporters = new();
        private readonly ConcurrentDictionary<Type, ThreadLocal<IAssetReader>> assetReaders = new();
        private readonly ConcurrentDictionary<Type, ThreadLocal<IAssetReader>> assetSubClassReaders = new();

        private readonly ConcurrentDictionary<(string, Type), GameElement> loadedAssets = new();
        private readonly ConcurrentDictionary<string, AssetPak> loadedPaks = new();

        // Properties
        public ScriptableProvider Scriptable => scriptable;
        public GraphicsDevice Graphics => graphics;
        public string AssetDirectory => assetDirectory;

        // Constructor
        public AssetProvider(ScriptableProvider scriptable, GraphicsDevice graphics, string assetDirectory, bool useWebRequest)
        {
            this.scriptable = scriptable;
            this.graphics = graphics;
            this.assetDirectory = assetDirectory;
            this.useWebRequest = useWebRequest;            

            // Get readers
            InitializeAssetImportersAndReaders();            
        }

        // Methods
        public void Unload(GameElement element)
        {
            // Check for null - but not destroyed
            if (element is null)
                return;

            // Report the unload
            Debug.Log($"Unload asset: '{element}'", LogFilter.Assets);

            // Destroy the asset
            if(element.IsDestroyed == false)
                GameElement.DestroyImmediate(element);

            // Remove from cache
            var cacheKey = loadedAssets.FirstOrDefault(K => K.Value == element);

            // Check for found
            if (cacheKey.Value is not null)
                loadedAssets.Remove(cacheKey.Key, out _);
        }

        public void UnloadAll()
        {
            // Process all cached
            foreach(var cacheKey in loadedAssets)
            {
                // Get the element
                GameElement element = cacheKey.Value;

                // Report the unload
                Debug.Log($"Unload asset: '{element}'", LogFilter.Assets);

                // Destroy the asset
                if (element.IsDestroyed == false)
                    GameElement.DestroyImmediate(element);
            }

            // Clear the cache
            loadedAssets.Clear();
        }

        public async Task<T> LoadAsync<T>(string assetRelativePath, string assetOrPakName = null, CancellationToken cancellationToken = default) where T : GameElement
        {
            // Check for cached
            if (loadedAssets.TryGetValue((assetRelativePath, typeof(T)), out GameElement element) == true)
                return element as T;

            // Check for asset name
            string name = Path.GetFileName(assetRelativePath);

            // Create the context
            AssetReadContext context = new AssetReadContext(this, typeof(T), name, null);

            // Report the request
            Debug.Log($"Load asset: '{assetRelativePath}' - {context.AssetType.FullName}", LogFilter.Assets);

            // Load the context
            return await LoadContextAsync(context, assetRelativePath, assetOrPakName, cancellationToken, false) as T;
        }

        public async Task<GameElement> LoadAsync(string assetRelativePath, Type assetType = null, string assetOrPakName = null, CancellationToken cancellationToken = default)
        {
            // Check type
            if (assetType == null)
                assetType = typeof(GameElement);

            // Check for cached
            if (loadedAssets.TryGetValue((assetRelativePath, assetType), out GameElement element) == true)
                return element;

            // Check for asset name
            string name = Path.GetFileName(assetRelativePath);

            // Create the context
            AssetReadContext context = new AssetReadContext(this, assetType, name, null);

            // Report the request
            Debug.Log($"Load asset: '{assetRelativePath}' - {context.AssetType.FullName}", LogFilter.Assets);

            // Load the context
            return await LoadContextAsync(context, assetRelativePath, assetOrPakName, cancellationToken, false);
        }

        internal async Task<GameElement> LoadContextAsync(AssetReadContext context, string assetRelativePath, string assetOrPakName, CancellationToken cancellationToken = default, bool checkCache = true)
        {
            // Check for cached
            if (checkCache == true && loadedAssets.TryGetValue((assetRelativePath, context.AssetType), out GameElement element) == true)
                return element;

            // Check for packed
            AssetPak pak = GetAssetPak(assetRelativePath, assetOrPakName);

            // Start timing
            Stopwatch timer = Stopwatch.StartNew();
            GameElement result = null;

            // Check for pak found
            if (pak != null)
            {
                // Get the stream
                pak.GetAssetStream(assetRelativePath, out Stream stream, out Type assetType);

                // Get the reader
                if (GetAssetReader(assetType, out IAssetReader reader) == false)
                    return null;

                try
                {
                    // Read the asset
                    using (stream)
                    {
                        // Load the content and wait for result
                        result = await reader.ReadAsync(context, stream, cancellationToken);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            else
            {
                // Check for any
                if (string.IsNullOrEmpty(context.AssetExtension) == true)
                    throw new ArgumentException("File extension must be provided when importing assets");

                // Get importer
                if (GetAssetImporter(context.AssetExtension, out IAssetImporter importer) == false)
                    return null;

                // Get the full path
                string assetFullPath = Path.Combine(assetDirectory, assetRelativePath);

                // Check for no extension
                if (string.IsNullOrEmpty(assetRelativePath) == true)
                    assetFullPath += KoraAssetReader.AssetExtension;

                try
                {
                    // Read the file
                    using (Stream stream = await RequestAssetStreamAsync(context.SearchDirectories, assetFullPath, cancellationToken))
                    {
                        // Load the content and wait for result
                        result = await importer.ImportAsync(context, stream, cancellationToken);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            // Wait for graphics upload
            await context.SubmitAsync();

            // Check for loaded
            if (result != null)
            {
                // Add to cache
                loadedAssets[(assetRelativePath, result.GetType())] = result;

                Debug.Log($"{new string('\t', context.DependencyDepth)}Loaded asset: '{assetRelativePath}' in {timer.Elapsed.TotalMilliseconds}ms", LogFilter.Assets);
            }
            else
            {
                Debug.LogError($"{new string('\t', context.DependencyDepth)}Could not load asset: '{assetRelativePath}", LogFilter.Assets);

                // Return explicit null here - it is possible that the reference is not null but the asset was destroyed, so it is not available
                return null;
            }
            return result;
        }

        internal bool GetAssetImporter(string ext, out IAssetImporter importer)
        {
            importer = null;

            // Check for no extension
            if(string.IsNullOrEmpty(ext) == true)
            {
                if(assetImporters.TryGetValue(KoraAssetReader.AssetExtension, out ThreadLocal<IAssetImporter> foundDefaultImporter) == true)
                {
                    importer = foundDefaultImporter.Value;
                    return true;
                }
            }

            // Get the asset importer
            if (assetImporters.TryGetValue(ext.ToLower(), out ThreadLocal<IAssetImporter> foundImporter) == false)
            {
                Debug.LogWarning($"Asset extension: '{ext}' is not recognized", LogFilter.Assets);
                return false;
            }

            // Importer was found
            importer = foundImporter.Value;
            return true;
        }

        internal bool GetAssetReader(Type type, out IAssetReader reader)
        {
            reader = null;

            // Get the asset reader
            if (assetReaders.TryGetValue(type, out ThreadLocal<IAssetReader> foundReader) == false)
            {
                // Check for derived
                foreach(KeyValuePair<Type, ThreadLocal<IAssetReader>> subClassEntry in assetSubClassReaders)
                {
                    if(subClassEntry.Key.IsAssignableFrom(type) == true)
                    {
                        foundReader = subClassEntry.Value;
                        reader = foundReader.Value;
                        return true;
                    }
                }

                Debug.LogWarning($"Asset type: '{type}' is not recognized", LogFilter.Assets);
                return false;
            }

            // Read was found
            reader = foundReader.Value;
            return true;
        }

        internal AssetPak GetAssetPak(string assetName, string pakName = null)
        {
            // Try to lookup
            if (string.IsNullOrEmpty(pakName) == false && loadedPaks.TryGetValue(pakName, out AssetPak assetPak) == true)
                return assetPak;

            // Check all paks - a bit slow??
            return loadedPaks.Values.FirstOrDefault(p => p.assetPaths.Contains(assetName));
        }

        internal string GetAssetPath(GameElement element)
        {
            foreach (var cacheKey in loadedAssets)
            {
                if (cacheKey.Value == element)
                    return cacheKey.Key.Item1;
            }
            return string.Empty;
        }

        private async Task<Stream> RequestAssetStreamAsync(IList<string> searchDirectories, string assetPath, CancellationToken cancellationToken)
        {
            // Check for web request
            if(useWebRequest == true)
            {
                // Set a small timeout for loading
                webClient.Timeout = TimeSpan.FromSeconds(webDefaultTimeoutSeconds);

                // Make the request
                return await webClient.GetStreamAsync(assetPath, cancellationToken);
            }

            // Just read from file
            if (File.Exists(assetPath) == true)
            {
                // Push search directory
                searchDirectories.Add(Directory.GetParent(assetPath).FullName);
                return File.OpenRead(assetPath);
            }

            // Get the file name
            string assetName = Path.GetFileName(assetPath);

            // Check search directories
            foreach (string directory in searchDirectories)
            {
                // Get the full path
                string searchPath = Path.Combine(directory, assetName);

                // Check for exists
                if (File.Exists(searchPath) == true)
                {
                    // Push search directory
                    searchDirectories.Add(Directory.GetParent(searchPath).FullName);
                    return File.OpenRead(searchPath);
                }
            }

            throw new IOException("Could not find asset path: " + assetPath);
        }

        private void InitializeAssetImportersAndReaders()
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
                        InitializeAssetImporter(type);
                        InitializeAssetReader(type);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e, LogFilter.Assets);
            }
        }

        private void InitializeAssetImporter(Type type)
        {
            foreach (AssetImporterAttribute attrib in type.GetCustomAttributes<AssetImporterAttribute>())
            {
                // Check for derived type
                if (typeof(IAssetImporter).IsAssignableFrom(type) == false)
                {
                    Debug.LogError($"Asset importer must implement {typeof(IAssetImporter)}: {type}", LogFilter.Assets);
                    continue;
                }

                // Get extension
                string ext = attrib.Extension.ToLower();

                // Check for overwrite content reader
                if (assetImporters.ContainsKey(ext) == true)
                {
                    Debug.LogError($"An importer reader already exists for extension: `{attrib.Extension}`- {type}", LogFilter.Assets);
                    continue;
                }

                // Store reader type
                assetImporters[ext] = new ThreadLocal<IAssetImporter>(() => (IAssetImporter)Activator.CreateInstance(type));
            }
        }

        private void InitializeAssetReader(Type type)
        {
            foreach (AssetReaderAttribute attrib in type.GetCustomAttributes<AssetReaderAttribute>())
            {
                // Check for derived type
                if (typeof(IAssetReader).IsAssignableFrom(type) == false)
                {
                    Debug.LogError($"Asset reader must implement {typeof(IAssetReader)}: {type}", LogFilter.Assets);
                    continue;
                }

                // Get extension
                Type forType = attrib.Type;

                // Check for overwrite content reader
                if (assetReaders.ContainsKey(forType) == true)
                {
                    Debug.LogError($"An asset reader already exists for extension: `{attrib.Type}`- {type}", LogFilter.Assets);
                    continue;
                }

                // Check for sub class
                if (attrib.SubType == true)
                {
                    // Store reader type for derived type
                    assetSubClassReaders[forType] = new ThreadLocal<IAssetReader>(() => (IAssetReader)Activator.CreateInstance(type));
                }
                else
                {
                    // Store reader type
                    assetReaders[forType] = new ThreadLocal<IAssetReader>(() => (IAssetReader)Activator.CreateInstance(type));
                }
            }
        }
    }
}
