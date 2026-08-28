using System;

namespace KoraPlayer;

public typealias NativeObject = void*;
public typealias ManagedObject = void*;

public abstract class ScriptBinding
{
	// Private
	private NativeObject nativeObject;
	private ManagedObject managedObject;

	// Properties
	public NativeObject Native => nativeObject;
	public ManagedObject Managed
	{
		get => managedObject;
		internal set => managedObject = value;
	}

	// Constructor
	protected this()
	{
		nativeObject = Internal.UnsafeCastToPtr(this);
	}

	public ~this()
	{
		nativeObject = null;
		managedObject = null;
	}

	// Methods
	public static T GetNativeObject<T>(NativeObject native) where T : ScriptBinding
	{
		return (T)Internal.UnsafeCastToObject(native);
	}
}