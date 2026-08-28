using System;
using System.Interop;

namespace KoraPlayer.Scripting;

internal sealed class Hostfxr
{
	// Type
	private typealias hostfxr_handle = void*;
	private typealias hostfxr_context = void*;

	// Initializes the hosting components for a dotnet command line running an application
	// This function does not load the runtime.
	private typealias initialize_for_runtime_config_fn = function [CallingConvention(.Stdcall)]
		int32(c_wchar* runtimeConfigPath, hostfxr_initialize_parameters* parameters, hostfxr_context* hostContextHandle);

	private typealias hostfxr_close_fn = function [CallingConvention(.Stdcall)]
		int32(hostfxr_context hostContextHandle);

	// Gets a typed delegate from the currently loaded CoreCLR or from a newly created one.
	private typealias get_runtime_delegate_fn = function [CallingConvention(.Stdcall)]
		int32(hostfxr_context hostContextHandle, hostfxr_delegate_type type, void** hostDelegate);

	// Signature of delegate returned by coreclr_delegate_type::load_assembly_and_get_function_pointer
	private typealias load_assembly_and_get_function_pointer_fn = function [CallingConvention(.Stdcall)]
		int32(c_wchar* assemblyPath, c_wchar* assemblyQualifiedTypeName, c_wchar* methodName, c_wchar* assemblyQualifiedDelegateTypeName, void* reserved, void** clrDelegate);

	
	public typealias get_function_pointer_fn = function [CallingConvention(.Stdcall)]
		int32(c_wchar* assemblyQualifiedTypeName, c_wchar* methodName, c_wchar* assemblyQualifiedDelegateTypeName, void* loadContext, void* reserved, void** cldDelegate);

	public typealias load_assembly_fn = function [CallingConvention(.Stdcall)]
		int32(c_wchar* assemblyPath, void* loadContext, void* reserved);

	public typealias load_assembly_bytes_fn = function [CallingConvention(.Stdcall)]
		int32(void* assemblyBytes, c_size assemblyBytesLength, void* symbolBytes, c_size symbolBytesLength, void* loadContext, void* reserved);

	enum hostfxr_delegate_type : c_int
	{
	    hdt_com_activation,
	    hdt_load_in_memory_assembly,
	    hdt_winrt_activation,
	    hdt_com_register,
	    hdt_com_unregister,
	    hdt_load_assembly_and_get_function_pointer,
	    hdt_get_function_pointer,
	    hdt_load_assembly,
	    hdt_load_assembly_bytes,
	};

	struct hostfxr_initialize_parameters
	{
	    c_size size;
	    c_wchar* host_path;
	    c_wchar* dotnet_root;
	};

	// Private
	private const String hostLibraryName = "hostfxr.dll";
	private const String initFnName = "hostfxr_initialize_for_runtime_config";
	private const String closeFnName = "hostfxr_close";
	private const String getDelegateFnName = "hostfxr_get_runtime_delegate";

	private hostfxr_handle hostLib;
	private hostfxr_context hostContext;
	private initialize_for_runtime_config_fn initHostFn;
	private hostfxr_close_fn closeHostFn;
	private get_runtime_delegate_fn getHostDelegateFn;

	// Public
	public const char16* UnmanagedCallers = (char16*)(void*)-1;

	// Constructor
	public this(String hostfxrPath, String configPath)
	{
		// Load library
		hostLib = Internal.LoadSharedLibrary(hostfxrPath);

		// Check for error
		if(hostLib == null)
			Runtime.FatalError(("Failed to load: " + hostLibraryName));

		// Get functions
		initHostFn = (initialize_for_runtime_config_fn)Internal.GetSharedProcAddress(hostLib, initFnName);
		closeHostFn = (hostfxr_close_fn)Internal.GetSharedProcAddress(hostLib, closeFnName);
		getHostDelegateFn = (get_runtime_delegate_fn)Internal.GetSharedProcAddress(hostLib, getDelegateFnName);

		// Init from config
		int32 result = initHostFn(configPath.ToScopedNativeWChar!(), null, &hostContext);

		// Check for error
		if(result != 0 || hostContext == null)
			Runtime.FatalError("Failed to initialize: " + initFnName);
	}

	// Methods
	public get_function_pointer_fn GetFunctionPointerDelegate()
	{
		// Get delegate for get function pointer
		get_function_pointer_fn call = null;
		int32 result = getHostDelegateFn(hostContext, .hdt_get_function_pointer, (void**)&call);

		// Check for error
		if(result != 0 || call == null)
			Runtime.FatalError("Failed to load procedure: get_function_pointer_fn");

		return call;
	}

	public load_assembly_fn LoadAssemblyDelegate()
	{
		// Get delegate for load assembly
		load_assembly_fn call = null;
		int32 result = getHostDelegateFn(hostContext, .hdt_load_assembly, (void**)&call);

		// Check for error
		if(result != 0 || call == null)
			Runtime.FatalError("Failed to load procedure: load_assembly_fn");

		return call;
	}

	public load_assembly_bytes_fn LoadAssemblyBytesDelegate()
	{
		// Get delegate for load assembly bytes
		load_assembly_bytes_fn call = null;
		int32 result = getHostDelegateFn(hostContext, .hdt_load_assembly_bytes, (void**)&call);

		// Check for error
		if(result != 0 || call == null)
			Runtime.FatalError("Failed to load procedure: load_assembly_bytes_fn");

		return call;
	}
}
