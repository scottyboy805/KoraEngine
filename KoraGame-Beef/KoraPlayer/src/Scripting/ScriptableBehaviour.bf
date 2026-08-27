using System;
using KoraPlayer;
using internal KoraPlayer;

namespace KoraPlayer.Scripting;

public sealed class ScriptableBehaviour : Component
{
	// Type
	private typealias CreateManaged = function ManagedObject(NativeObject native, char8* assemblyQualifiedName);
	private typealias StartFunction = function void(ManagedObject managed);
	private typealias UpdateFunction = function void(ManagedObject managed);

	// Bindings
	private static CreateManaged createManagedFn = null;
	private static StartFunction startFn = null;
	private static UpdateFunction updateFn = null;

	// Private
	private int32 priority = 0;
	private String assemblyQualifiedName = new .() ~delete _;
	private bool didStart = false;

	// Properties
	public int32 Priority => priority;

	// Constructor
	static this()
	{
		InitBindings();
	}



	// Methods
	internal void OnStart()
	{
		if(didStart == false)
		{
			didStart = true;
			startFn(Managed);
		}
	}

	internal void OnUpdate()
	{
		updateFn(Managed);
	}

	public static ScriptableBehaviour CreateScriptInstance(StringView assemblyQualifiedName)
	{
		// Create new instance
		ScriptableBehaviour instance = new .();

		// Update instance
		instance.assemblyQualifiedName.Set(assemblyQualifiedName);

		// Create the managed object
		instance.Managed = createManagedFn(instance.Native, assemblyQualifiedName.ToScopeCStr!());

		// Get the instance
		return instance;
	}

	private static void InitBindings()
	{
	}


}