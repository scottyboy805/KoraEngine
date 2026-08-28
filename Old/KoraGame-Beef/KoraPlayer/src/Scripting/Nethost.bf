using System;
using System.Interop;
using System.IO;

namespace KoraPlayer.Scripting;

internal sealed class Nethost
{
	// Type
	private typealias nethost_handle = void*;

	// Get the path to the hostfxr library
	private typealias get_hostfxr_path_fn = function [CallingConvention(.Stdcall)]
		int32(c_wchar* buffer, c_size* bufferSize, void* parameters);

	// Private
	private const String nethostName = "nethost.dll";
	private const String getPathFnName = "get_hostfxr_path";

	private nethost_handle hostLib;
	private get_hostfxr_path_fn getPathFn;

	// Constructor
	public this(String basePath = null)
	{
		// Get the load path
		String loadPath = nethostName;

		// Check for base path provided
		if(basePath != null)
		{
			// Combine the full path
			Path.Combine(loadPath, basePath, nethostName);
		}

		// Load the assembly
		hostLib = Internal.LoadSharedLibrary(loadPath);

		// Check for error
		if(hostLib == null)
			Runtime.FatalError("Failed to load: " + nethostName);

		// Get function
		getPathFn = (get_hostfxr_path_fn)Internal.GetSharedProcAddress(hostLib, getPathFnName);

		// Check for error
		if(hostLib == null)
			Runtime.FatalError("Failed to load procedure: " + getPathFnName);
	}

	// Methods
	public void GetHostFxrPath(String hostfxrPath)
	{
		// Create buffer
		c_wchar[] buffer = scope .[255];
		uint64 bufferSize = 255;

		// Try to get the path
		int32 result = getPathFn(&buffer[0], &bufferSize, null);

		// Check for error
		if(result != 0 || bufferSize == 0)
			Runtime.FatalError("Failed to get hostfxr path");

		// Create span
		Span<c_wchar> rawString = .(buffer, 0, (int)bufferSize);

		// Create temp string
		String str = scope String(rawString);

		// Update result
		hostfxrPath.Set(str);
	}
}