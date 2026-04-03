using KoraGame;
using KoraGame.Graphics;
using System.Diagnostics;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("KoraEditor")]

namespace KoraPipeline
{
    public sealed class AssetDatabase : AssetProvider
    {
        // Events
        public event Action OnAssetModified;
        public event Action<string> OnAssetImported;              // Path
        public event Action<string> OnAssetDeleted;               // Path
        public event Action<string, string> OnAssetMoved;         // Old Path, New Path
        public event Action<string> OnAssetFolderCreated;         // Path
        public event Action<string> OnAssetFolderDeleted;         // Path

        // Private
        private FileSystemWatcher assetWatcher = null;
        private Dictionary<string, AssetMetadata> assetMetas = new();   // Guid, Meta
        private Dictionary<string, string> assetPaths = new();          // Path, Guid
        private Dictionary<GameElement, string> assetGuids = new();     // Element, Guid        
        private HashSet<string> assetDirtyGuids = new();

        // Constructor
        public AssetDatabase(ScriptableProvider scriptable, GraphicsDevice graphics, string assetDirectory, bool useWebRequest) 
            : base(scriptable, graphics, assetDirectory, useWebRequest)
        {
            // Create content watcher
            assetWatcher = new FileSystemWatcher(assetDirectory, "*");
            assetWatcher.EnableRaisingEvents = true;
            assetWatcher.IncludeSubdirectories = true;
            assetWatcher.Created += OnAssetWatcherCreated;
            assetWatcher.Deleted += OnAssetWatcherDeleted;
        }

        // Methods
        public string GetAssetPath(string guid)
        {         
            // Check for empty
            if(string.IsNullOrEmpty(guid) == true)
                throw new ArgumentException(nameof(guid) + " cannot be null or empty");

            // Try to lookup guid
            if (assetMetas.TryGetValue(guid, out AssetMetadata meta) == true)
                return meta.AssetPath;

            // Not found
            return null;
        }

        public override string GetAssetPath(GameElement element)
        {
            // Check for nunll
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            // Try to lookup guid and then path
            if (assetGuids.TryGetValue(element, out string guid) == true && assetMetas.TryGetValue(guid, out AssetMetadata meta) == true)
                return meta.AssetPath;

            // Not found
            return null;
        }

        public string GetAssetRelativePath(string assetFullPath)
        {
            // Get the new path
            string newPath = Path.GetRelativePath(AssetDirectory, assetFullPath);

            // Convert separator
            newPath = newPath.Replace('\\', '/');
            return newPath;
        }

        public void Refresh()
        {
            // Process all asset files
            foreach (string file in Directory.EnumerateFiles(AssetDirectory, "*.*", SearchOption.AllDirectories)
                .Where(f => Path.GetExtension(f) != AssetMetadata.AssetMetaExtension))
            {
                // Get the relative path
                string contentPath = GetAssetRelativePath(file);

                // Import the asset
                Import(contentPath);
            }

            // Process no longer existing content - create clone here so we can modify collections
            foreach (KeyValuePair<string, string> contentPath in assetPaths.ToDictionary())
            {
                // Get the content path
                string fullPath = Path.Combine(AssetDirectory, contentPath.Key);

                // Check for content no longer available
                if (File.Exists(fullPath) == false)
                {
                    // Check for content file
                    if (File.Exists(fullPath + AssetMetadata.AssetMetaExtension) == true)
                        File.Delete(fullPath + AssetMetadata.AssetMetaExtension);

                    // Remove from cache
                    assetPaths.Remove(contentPath.Key);
                    assetMetas.Remove(contentPath.Value);

                    // Trigger event
                    Game.DoEvent(OnAssetModified);
                    Game.DoEvent(OnAssetDeleted, contentPath.Key);
                }
            }
        }

        public IEnumerable<string> SearchAssets(string searchFolder = null, string search = null, SearchOption option = SearchOption.TopDirectoryOnly, IEnumerable<Type> ofTypes = null)
        {
            // Get full search path
            if (string.IsNullOrEmpty(searchFolder) == true)
            {
                searchFolder = AssetDirectory;

            }
            else
            {
                CheckAssetPathValid(searchFolder, nameof(searchFolder));
                searchFolder = Path.Combine(AssetDirectory, searchFolder);
            }

            // Check for empty search
            if (string.IsNullOrEmpty(search) == true)
            {
                search = "*.*";
            }
            else if (Path.HasExtension(search) == false)
            {
                search += ".*";
            }

            // Process all content files
            foreach (string file in Directory.EnumerateFiles(searchFolder, search, option)
                .Where(f => Path.GetExtension(f) != AssetMetadata.AssetMetaExtension))
            {
                // Get the relative path
                string contentPath = GetAssetRelativePath(file);

                // Try to get guid
                string guid;
                if (assetPaths.TryGetValue(contentPath, out guid) == true)
                {
                    // Check for type
                    if (ofTypes != null)
                    {
                        //// Get the meta
                        //ContentMeta meta;
                        //contentMetas.TryGetValue(guid, out meta);

                        //// Check for matching type
                        //if (meta == null || type.IsAssignableFrom(GetContentType(guid)) == false)
                        //    continue;
#warning Todo - fix type condition
                    }

                    // The content is a match
                    yield return guid;
                }
            }
        }

        public IEnumerable<string> SearchFolders(string searchFolder = null, string search = null, SearchOption option = SearchOption.TopDirectoryOnly)
        {
            // Get full search path
            if (string.IsNullOrEmpty(searchFolder) == true)
            {
                searchFolder = AssetDirectory;

            }
            else
            {
                CheckAssetPathValid(searchFolder, nameof(searchFolder));
                searchFolder = Path.Combine(AssetDirectory, searchFolder);
            }

            // Check for empty search
            if (string.IsNullOrEmpty(search) == true)
            {
                search = "*.*";
            }

            // Get Directories
            foreach(string fullPath in Directory.EnumerateDirectories(searchFolder, search, option))
            {
                // Get the relative path
                yield return GetAssetRelativePath(fullPath);
            }
        }

        public void Import(string assetOrFolderPath)
        {
            // Make sure path is valid and exists
            string fullPath = CheckAssetPath(assetOrFolderPath);// false);

            // Check for directory
            if ((File.GetAttributes(fullPath) & FileAttributes.Directory) != 0)
            {
                // List the files inside
                foreach (string file in Directory.EnumerateFiles(fullPath, "*.*", SearchOption.TopDirectoryOnly)
                    .Where(f => Path.GetExtension(f) != AssetMetadata.AssetMetaExtension))
                {
                    // Get the relative path
                    string relativePath = GetAssetRelativePath(file);

                    // Import the content
                    Import(relativePath);
                }

                // Process directories
                foreach (string directory in Directory.EnumerateDirectories(fullPath, "*", SearchOption.TopDirectoryOnly))
                {
                    // Get the relative path
                    string relativePath = GetAssetRelativePath(directory);

                    // Import the content
                    Import(relativePath);
                }

                // Trigger event
                Game.DoEvent(OnAssetModified);
                Game.DoEvent(OnAssetFolderCreated, assetOrFolderPath);
            }
            // Must be a file
            else
            {
                // Check for supported
                //if (IsContentSupported(path) == false)
                //{
                //    Debug.LogWarning("Content is not supported: " + path);
                //    return;
                //}

                // Get the content meta
                AssetMetadata meta = GetOrCreateMetadata(assetOrFolderPath);

                // Update cache
                assetMetas[meta.Guid] = meta;
                assetPaths[assetOrFolderPath] = meta.Guid;

                //// Check if importer is avialable for this type of content
                //if (meta.Importer != null)
                //{
                //    // Unload existing content - so it can be loaded again later
                //    UnloadAsset(assetOrFolderPath);

                //    // Build the content
                //    BuildContent(assetOrFolderPath);
                //}

                // Trigger event
                Game.DoEvent(OnAssetModified);
                Game.DoEvent(OnAssetImported, assetOrFolderPath);
            }
        }

        public void Delete(string assetOrFolderPath)
        {
            // Make sure path is valid
            string fullPath = CheckAssetPath(assetOrFolderPath);

            // Check for directory
            if ((File.GetAttributes(fullPath) & FileAttributes.Directory) != 0)
            {
                // List the files inside
                foreach (string file in Directory.EnumerateFiles(fullPath, "*.*", SearchOption.TopDirectoryOnly)
                    .Where(f => Path.GetExtension(f) != AssetMetadata.AssetMetaExtension))
                {
                    // Get the relative path
                    string relativePath = GetAssetRelativePath(file);

                    // Delete the content
                    Delete(relativePath);
                }

                // Process directories
                foreach (string directory in Directory.EnumerateDirectories(Path.Combine(AssetDirectory, assetOrFolderPath), "*", SearchOption.TopDirectoryOnly))
                {
                    // Get the relative path
                    string relativePath = GetAssetRelativePath(directory);

                    // Delete the content
                    Delete(relativePath);
                }

                // Finally delete the directory
                Directory.Delete(Path.Combine(AssetDirectory, assetOrFolderPath));

                // Trigger event
                Game.DoEvent(OnAssetModified);
                Game.DoEvent(OnAssetFolderDeleted, assetOrFolderPath);
            }
            // Must be a file
            else
            {
                // Unload the asset
                //Unload(assetOrFolderPath);
#warning Todo - unload asset before delete

                // Check for registered
                if (assetPaths.ContainsKey(assetOrFolderPath) == true)
                {
                    string contentFullPath = Path.Combine(AssetDirectory, assetOrFolderPath);

                    //// Clean intermediate and output content
                    //pipelineManager.CleanContent(contentFullPath);

                    // Delete files
                    File.Delete(contentFullPath);
                    File.Delete(contentFullPath + ".content");
                }

                // Get the guid
                string guid;
                assetPaths.TryGetValue(assetOrFolderPath, out guid);

                // Update cache
                assetMetas.Remove(guid);
                assetPaths.Remove(assetOrFolderPath);

                // Trigger event
                Game.DoEvent(OnAssetModified);
                Game.DoEvent(OnAssetDeleted, assetOrFolderPath);
            }
        }

        public void Move(string currentAssetOrFolderPath, string newPath)
        {
            // Make sure paths are valid - note that we check if the source path exists here
            string sourcePath = CheckAssetPath(currentAssetOrFolderPath, nameof(currentAssetOrFolderPath));
            CheckAssetPathValid(newPath, nameof(newPath));

            // TODO - move file or directory??


            // Trigger event
            Game.DoEvent(OnAssetModified);
            Game.DoEvent(OnAssetMoved, currentAssetOrFolderPath, newPath);
        }

        public void OpenAsset(string path)
        {
            // Make sure path is valid
            string fullPath = CheckAssetPath(path);

            // Open process
            using (Process.Start("explorer", "\"" + fullPath.Replace('/', '\\') + "\"")) ;
        }

        public bool IsFolder(string path)
        {
            string fullPath = CheckAssetPath(path);

            return (File.GetAttributes(fullPath) & FileAttributes.Directory) != 0;
        }

        public bool IsAssetDirty(string guid)
        {
            // Check empty
            if(string.IsNullOrEmpty(guid)) 
                return false;

            return assetDirtyGuids.Contains(guid);
        }

        public bool IsAssetDirty(GameElement element)
        {
            // Check for null
            if (element == null)
                return false;

            // Try to lookup guid and then path
            if (assetGuids.TryGetValue(element, out string guid) == true && assetDirtyGuids.Contains(guid) == true)
                return true;

            return false;
        }

        public void SetAssetDirty(string guid)
        {
            // Check empty
            if (string.IsNullOrEmpty(guid))
                return;

            if (assetDirtyGuids.Contains(guid) == false)
                assetDirtyGuids.Add(guid);
        }

        public void SetAssetDirty(GameElement element)
        {
            // Check for null
            if (element == null)
                return;

            // Try to lookup guid and then path
            if (assetGuids.TryGetValue(element, out string guid) == true && assetDirtyGuids.Contains(guid) == false)
                assetDirtyGuids.Add(guid);
        }

        private AssetMetadata GetOrCreateMetadata(string assetPath)
        {
            // Check the path
            string fullPath = CheckAssetPath(assetPath);

            // Check for exists
            bool exists = File.Exists(fullPath + AssetMetadata.AssetMetaExtension);

            // Get guid
            AssetMetadata meta = null;
            if(assetPaths.TryGetValue(assetPath, out string guid) == false
                || assetMetas.TryGetValue(guid, out meta) == false
                || exists == false)
            {
                // Create new
                if(exists == false)
                {
                    // Create the metadata
                    meta = new AssetMetadata(assetPath);

                    // Write to disk
                    WriteMetadata(meta);
                }
            }

            // Load meta from disk
            if (meta == null)
                meta = ReadMetadata(assetPath);

            // Update asset path and guid
            meta.AssetPath = assetPath;
            meta.EnsureGuid();

            return meta;
        }

        private void WriteMetadata(AssetMetadata meta)
        {
            // Get the path
            string fullPath = JoinAssetPath(AssetDirectory, meta.MetaPath);
            //// Write to disk
            //SerializedJson.WriteToFile(fullPath + AssetMetadata.AssetMetaExtension, meta);
        }

        private AssetMetadata ReadMetadata(string assetPath)
        {
            // Get the meta path
            string metaPath = JoinAssetPath(AssetDirectory, assetPath + AssetMetadata.AssetMetaExtension);

            // Read from disk
            string json = File.ReadAllText(metaPath);

            // Deserialize meta
            return SerializedJson.DeserializeAsync<AssetMetadata>(json).Result;
        }

        private async void OnAssetWatcherCreated(object sender, FileSystemEventArgs e)
        {
            // Get the relative path
            string relativePath = GetAssetRelativePath(e.FullPath);

            // Wait for time for IO to update
            await Task.Delay(25);

            // Import the content
            Import(relativePath);
        }

        private void OnAssetWatcherDeleted(object sender, FileSystemEventArgs e)
        {
            // Do a full refresh to scan for deleted content
            Refresh();
        }

        private string CheckAssetPath(string path, string hintName = null)
        {
            if (hintName == null)
                hintName = nameof(path);

            // Make sure path is valid
            CheckAssetPathValid(path, hintName);

            // Check for content path
            string assetPath = JoinAssetPath(AssetDirectory, path);

            // Check for exists
            if (File.Exists(assetPath) == true || Directory.Exists(assetPath) == true)
                return assetPath;

            // Path was not found
            throw new IOException(hintName + " could not be found: " + path);
        }

        internal static void CheckAssetPathValid(string path, string hintName = null)
        {
            if (hintName == null)
                hintName = nameof(path);

            // Check for null or empty
            if (string.IsNullOrEmpty(path) == true)
                throw new ArgumentException("Path cannot be null or empty");

            // Check for invalid backslash
            if (path.Contains('\\') == true)
                throw new FormatException("Path should not use '\\' separator: Use '/' instead");

            // Check for rooted
            if (Path.IsPathRooted(path) == true)
                throw new ArgumentException("Path must be relative to the assets folder");
        }

        internal static string JoinAssetPath(string a, string b)
        {
            return Path.Combine(a, b).Replace('\\', '/');
        }

        internal static string JoinAssetPath(string a, string b, string c)
        {
            return Path.Combine(a, b, c).Replace('\\', '/');
        }

        internal static string JoinAssetPath(string a, string b, string c, string d)
        {
            return Path.Combine(a, b, c, d).Replace('\\', '/');
        }
    }
}
