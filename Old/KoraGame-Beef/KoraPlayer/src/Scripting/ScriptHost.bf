using System;
using System.Interop;
using internal KoraPlayer.Scripting;

namespace KoraPlayer.Scripting;

internal sealed class ScriptHost
{
	typealias SayHelloFn = function [CallingConvention(.Cdecl)] void();

	// Private
	private const String hostLibraryName = "hostfxr.dll";
	private const String initFnName = "hostfxr_initialize_for_runtime_config";
	private const String closeFnName = "hostfxr_close";
	private const String getDelegateFnName = "hostfxr_get_runtime_delegate";
	//private const String loadAssemblyPathFnName = "load_assembly_fn";
	//private const String loadAssemblyBytesFnName = "load_assembly_bytes_fn";

	

	private Nethost nethost;
	private Hostfxr hostfxr;

	
	private Hostfxr.get_function_pointer_fn getFunctionPointerFn;
	private Hostfxr.load_assembly_fn loadAssemblyPathFn;
	private Hostfxr.load_assembly_bytes_fn loadAssemblyBytesFn;

	// Constructor
	public ~this()
	{
		delete nethost;
		delete hostfxr;
	}

	// Methods
	public void LoadRuntime(String configName)
	{
		System.IO.Directory.SetCurrentDirectory("D:/VisualStudio/KoraEngine/KoraGame-Beef/build/Release_Win64/KoraPlayer-Desktop/");

		// Create the host
		nethost = new .();

		// Get hostfxr path
		String hostFxrPath = scope .();
		nethost.GetHostFxrPath(hostFxrPath);

		// Create host
		hostfxr = new .(hostFxrPath, "runtimeconfig.json");

		// Get delegates
		getFunctionPointerFn = hostfxr.GetFunctionPointerDelegate();
		loadAssemblyPathFn = hostfxr.LoadAssemblyDelegate();
		loadAssemblyBytesFn = hostfxr.LoadAssemblyBytesDelegate();


//		hostfxr.load_assembly_and_get_function_pointer_fn loadDelegate = null;
//		int32 rc = getHostDelegateFn(hostfxr, .hdt_load_assembly_and_get_function_pointer, (void**)&loadDelegate);
//
//		if(rc != 0 || loadDelegate == null)
//			Runtime.FatalError("Failed get delegate");
//
//		// Init from config
//		hostfxr_handle hostfxr = null;
//		int32 rc = initFunc(config.ToScopedNativeWChar!(), null, &hostfxr);
//
//		if(rc != 0 || hostfxr == null)
//			Runtime.FatalError("Failed init");

		// Get delegate
		

		// Load assembly
//		SayHelloFn call = null;
//		rc = loadDelegate(
//			"ScriptEngine.dll".ToScopedNativeWChar!(),
//			"ScriptEngine.Bridge, ScriptEngine".ToScopedNativeWChar!(),
//			"SayHello".ToScopedNativeWChar!(),
//			hostfxr_unmanagedcallers,
//			null,
//			(void**)&call);
//
//		if(rc != 0 || call == null)
//			Runtime.FatalError("Failed load delegate");
//
//		call();
	}

	public bool LoadAssembly(String assemblyPath)
 	{
		 // Try to load assembly
		 int32 result = loadAssemblyPathFn(assemblyPath.ToScopedNativeWChar!(), null, null);

		 // Check for error
		 if(result != 0 || getFunctionPointerFn == null)
			Runtime.FatalError("Failed to load script assembly");

		 // Get result
		 return result == 0;
	}

	public bool LoadAssembly(Span<uint8> assemblyBytes, Span<uint8> symbolBytes = null)
	{
		// Try to load assembly
		int32 result = loadAssemblyBytesFn(
			assemblyBytes.Ptr,
			(uint)assemblyBytes.Length,
			symbolBytes.IsEmpty == false ? symbolBytes.Ptr : null,
			symbolBytes.IsEmpty == false ? (uint)symbolBytes.Length : 0,
			null, null);

		// Check for error
		if(result != 0 || getFunctionPointerFn == null)
			Runtime.FatalError("Failed to load script assembly");

		// Get result
		return result == 0;
	}

	public bool GetFunctionPointer(String assemblyQualifiedTypeName, String methodName, void** delegateFn)
	{
		// Try to get delegate
		int32 result = getFunctionPointerFn(
			assemblyQualifiedTypeName.ToScopedNativeWChar!(),
			methodName.ToScopedNativeWChar!(),
			Hostfxr.UnmanagedCallers,
			null,
			null,
			delegateFn);

		// Check for error
		if(result != 0 || delegateFn == null)
			Runtime.FatalError("Failed to load delegate");

		// Get result
		return result == 0;
	}
}