using System;
using System.Collections;
using System.IO;
using internal KoraPlayer;
using KoraPlayer.Graphics;

namespace KoraPlayer.Assets;

public class AssetProvider
{
	// Private
	private readonly StringView assetDirectory;
	private readonly GraphicsDevice graphics;

	private readonly Dictionary<String, IAssetImporter> assetImporters = new .();
	private readonly Dictionary<Type, IAssetReader> assetReaders = new .();
	private readonly Dictionary<Type, IAssetReader> assetSubTypeReaders = new .();

	private readonly List<GameElement> loadedAssets = new .();
	private readonly Dictionary<StringView, AssetPak> loadedAssetPaks = new .();

	// Constructor
	public this(GraphicsDevice graphics, StringView assetDirectory)
	{
		this.graphics = graphics;
		this.assetDirectory = assetDirectory;

		// Init importers and readers
		InitializeAssetImportersAndReaders();
	}

	protected internal ~this()
	{
		// Cleanup importers
		for(let importer in assetReaders)
		{
			delete importer.key;
			delete importer.value;
		}

		delete assetImporters;

		// Cleanup readers
		for(let reader in assetReaders)
			delete reader.value;

		for(let reader in assetSubTypeReaders)
			delete reader.value;

		delete assetReaders;
		delete assetSubTypeReaders;

		// Cleanup loaded assets
		for(let loaded in loadedAssets)
		{
			GameElement.DestroyImmediate(loaded);
			delete loaded;
		}

		delete loadedAssets;

		// Cleanup loaded paks
		for(let pak in loadedAssetPaks)
		{
			GameElement.DestroyImmediate(pak.value);
			delete pak.value;
		}

		delete loadedAssetPaks;
	}

	// Methods
	public T LoadAsset<T>(StringView assetRelativePath, StringView assetOrPakName = null) where T : GameElement
	{
		// Get asset name and extension
		String name = scope .();
		String ext = scope .();

		Path.GetFileNameWithoutExtension(assetRelativePath, name);
		Path.GetExtension(assetRelativePath, ext);

		// Create load context
		AssetLoadContext loadContext = AssetLoadContext(graphics, typeof(T), name, ext);

		// Report the request
		Debug.Log(scope $"Load asset: '{assetRelativePath}' - {loadContext.AssetType}", LogFilter.Assets);

		// Load from context
		return (T)LoadAssetFromContext(loadContext, assetRelativePath, assetOrPakName);
	}

	internal GameElement LoadAssetFromContext(in AssetLoadContext context, StringView assetRelativePath, StringView assetOrPakName = null)
	{
		// Check for packed
		AssetPak pak = GetAssetPak(assetRelativePath, assetOrPakName);

		// Start timing
		System.Diagnostics.Stopwatch timer = .StartNew();


		GameElement result = null;

		// Check for pak
		if(pak != null)
		{

		}
		else
		{
			// Require extension
			if(context.AssetExtension.IsEmpty == true)
			{
				Debug.LogError("File extension must be provided when importing", .Assets);
				return null;
			}

			// Get the importer
			IAssetImporter assetImporter;
			if(GetAssetImporter(context.AssetExtension, out assetImporter) == false)
			{
				Debug.LogError(scope $"Failed to find importer for extension: {context.AssetExtension}");
				return null;
			}

			// Get the stream
			Stream stream;
			if(GetAssetStream(assetRelativePath, out stream) == false)
			{
				Debug.LogError(scope $"Failed to open stream for asset: {assetRelativePath}. Make sure the asset exists!");
				return null;
			}

			// Import the asset
			let importResult = assetImporter.ImportAsset(context, stream);

			// Cleanup the stream
			stream.Close();
			delete stream;

			// Try to get the result
			if(importResult case .Ok)
				result = importResult;
		}

		// Stop timing
		double elapsedMs = timer.Elapsed.TotalMilliseconds;
		delete timer;

		// Add loaded asset
		if(result != null)
		{
			loadedAssets.Add(result);
			Debug.Log(scope $"Loaded asset: '{assetRelativePath}' in {elapsedMs}ms", LogFilter.Assets);
		}

		return result;
	}

	internal AssetPak GetAssetPak(StringView nameOrPath, StringView pakName = null)
	{
		// Check for pak name
		if(pakName.IsEmpty == false)
		{
			// Try to get asset pack
			AssetPak assetPak = null;
			if(loadedAssetPaks.TryGetValue(pakName, out assetPak) == true)
				return assetPak;
		}

		// Check all asset packs
		for(let pak in loadedAssetPaks.Values)
		{
			if(pak.HasAsset(nameOrPath) == true)
				return pak;
		}

		return null;
	}

	internal bool GetAssetImporter(StringView ext, out IAssetImporter importer)
	{
		importer = null;

		// Get lower str
		String extLower = scope .(ext);
		extLower.ToLower();

		// Try to get importer
		return assetImporters.TryGetValue(extLower, out importer);
	}

	internal bool GetAssetStream(StringView assetName, out Stream stream)
	{
		stream = null;

		// Build relative path
		String assetFullPath = scope .();
		Path.Combine(assetFullPath, assetDirectory, assetName);

		// Check for exists
		if(File.Exists(assetFullPath) == true)
		{
			// Get file stream
			FileStream fs = new .();
			fs.Open(assetFullPath, .Read, .Read);
			
			// Open the stream
			stream = fs;
			return true;
		}

		// Could not find stream
		return false;
	}

	private void InitializeAssetImportersAndReaders()
	{
		for(let type in Type.Types)
		{
			// Skip some types
			if(type.IsPrimitive == true || type.IsPointer == true || !(type is System.Reflection.TypeInstance))
				continue;

			InitializeAssetImporter(type);
			InitializeAssetReader(type);
		}
	}

	private void InitializeAssetImporter(Type type)
	{
	    for (CustomAssetImporterAttribute attrib in type.GetCustomAttributes<CustomAssetImporterAttribute>())
	    {
	        // Check for derived type
	        if (type.ImplementsInterface(typeof(IAssetImporter)) == false)
	        {
	            Debug.LogError(scope $"Asset importer must implement {typeof(IAssetImporter)}: {type}", LogFilter.Assets);
	            continue;
	        }

	        // Check for overwrite content reader
	        if (assetImporters.ContainsKey(attrib.Extension) == true)
	        {
	            Debug.LogError(scope $"An importer reader already exists for extension: `{attrib.Extension}`- {type}", LogFilter.Assets);
	            continue;
	        }

			// Allocate key
			String extLower = new .(attrib.Extension);
			extLower.ToLower();

	        // Store reader type
	        assetImporters[extLower] = (IAssetImporter)type.CreateObject().Value;
	    }
	}

	private void InitializeAssetReader(Type type)
	{
	    for (CustomAssetReaderAttribute attrib in type.GetCustomAttributes<CustomAssetReaderAttribute>())
	    {
	        // Check for derived type
	        if (type.ImplementsInterface(typeof(IAssetReader)) == false)
	        {
	            Debug.LogError(scope $"Asset reader must implement {typeof(IAssetReader)}: {type}", LogFilter.Assets);
	            continue;
	        }

	        // Get extension
	        Type forType = attrib.Type;

	        // Check for overwrite content reader
	        if (assetReaders.ContainsKey(forType) == true)
	        {
	            Debug.LogError(scope $"An asset reader already exists for extension: `{attrib.Type}`- {type}", LogFilter.Assets);
	            continue;
	        }

	        // Check for sub class
	        if (attrib.SubType == true)
	        {
	            // Store reader type for derived type
	            assetSubTypeReaders[forType] = (IAssetReader)type.CreateObject().Value;
	        }
	        else
	        {
	            // Store reader type
	            assetReaders[forType] = (IAssetReader)type.CreateObject().Value;
	        }
	    }
	}
}