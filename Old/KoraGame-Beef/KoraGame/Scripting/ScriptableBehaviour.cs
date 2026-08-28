using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace KoraGame.Scripting
{
    public abstract class ScriptableBehaviour : GameElement
    {
        // Properties
        public int Priority
        {
            get
            {
                binding.CheckNative();
                return ScriptableBehaviour_GetPriority(binding.NativePtr);
            }
            set
            {
                binding.CheckNative();
                ScriptableBehaviour_SetPriority(binding.NativePtr, value);
            }
        }

        // Constructor
        private ScriptableBehaviour(IntPtr nativePtr) 
        {
            binding.NativePtr = nativePtr;
        }

        protected unsafe ScriptableBehaviour()
        {
            using(Utf8String rawString = GetType().AssemblyQualifiedName)
                binding.NativePtr = ScriptableBehaviour_CreateNative(binding.ManagedPtr, rawString.Ptr);
        }

        // Methods
        protected virtual void OnStart() { }
        protected virtual void OnUpdate() { }

        // Bindings
        [UnmanagedCallersOnly]
        internal static unsafe IntPtr ScriptableBehaviour_CreateManaged(IntPtr nativePtr, byte* assemblyQualifiedName)
        {
            // Get type name
            string name = Utf8StringMarshaller.ConvertToManaged(assemblyQualifiedName);

            // Get the type
            Type type = Type.GetType(name);

            // Create new managed script
            ScriptableBehaviour script = (ScriptableBehaviour)Activator.CreateInstance(type, new[] { nativePtr });

            // Get managed
            return script.binding.ManagedPtr;
        }

        [UnmanagedCallersOnly]
        internal static void ScriptableBehaviour_DoScriptStart(IntPtr managed)
        {
            // Get the managed object
            ScriptableBehaviour behaviour = NativeBinding.GetManagedObject<ScriptableBehaviour>(managed);
                        
            try
            {
                // Invoke
                behaviour.OnStart();
            }
            catch(Exception e)
            {
                Debug.LogException(e, LogFilter.Script, behaviour);
            }
        }

        [UnmanagedCallersOnly]
        internal static void ScriptableBehaviour_DoScriptUpdate(IntPtr managed)
        {
            // Get the managed object
            ScriptableBehaviour behaviour = NativeBinding.GetManagedObject<ScriptableBehaviour>(managed);

            try
            {
                // Invoke
                behaviour.OnUpdate();
            }
            catch (Exception e)
            {
                Debug.LogException(e, LogFilter.Script, behaviour);
            }
        }

        [DllImport("KoraGame")]
        internal static extern unsafe IntPtr ScriptableBehaviour_CreateNative(IntPtr managedPtr, byte* assemblyQualifiedName);

        [DllImport("KoraGame")]
        internal static extern int ScriptableBehaviour_GetPriority(IntPtr nativePtr);

        [DllImport("KoraGame")]
        internal static extern void ScriptableBehaviour_SetPriority(IntPtr nativePtr, int priority);
    }
}
