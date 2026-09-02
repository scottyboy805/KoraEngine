using System;

namespace KoraPlayer;

public abstract class NativeElement
{
	// Type
	protected typealias NativePointer = void*;

	// Private
	private NativePointer ptr;

	// Properties
	public NativePointer Ptr => ptr;

	// Constructor
	public this()
	{
		// Get pointer
		ptr = Internal.UnsafeCastToPtr(this);
	}

	// Methods
	public static void Destroy()
	{

	}

	public static T Get<T>(NativePointer ptr) where T : NativeElement
	{
		// Try to get object
		return Internal.UnsafeCastToObject(ptr) as T;
	}
}