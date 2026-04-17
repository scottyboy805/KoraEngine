using System.Reflection;
using System.Runtime.Loader;

namespace KoraGame
{
    public sealed class ScriptableProvider
    {
        // Private
        private readonly List<AssemblyLoadContext> loadedContexts = new();                

        // Methods
        public object CreateInstance(Type type)
        {
            return Activator.CreateInstance(type, true);
        }

        public T CreateInstance<T>(Type instanceType = null)
        {
            // Check type
            if(instanceType == null)
                instanceType = typeof(T);

            // Check derived
            if (typeof(GameElement).IsAssignableFrom(instanceType) == false)
                throw new ArgumentException("Type must be GameElement");

            // Create the instance
            return (T)Activator.CreateInstance(instanceType, true);
        }

        public T CreateInstanceAs<T>(Type type)
        {
            try
            {
                return (T)Activator.CreateInstance(type);
            }
            catch(InvalidCastException)
            {
                return default;
            }
        }

        public Assembly LoadAssembly(string assemblyPath)
        {
            // Create context
            AssemblyLoadContext context = new AssemblyLoadContext(null);

            // Load the assembly context
            Assembly asm = context.LoadFromAssemblyPath(assemblyPath);

            // Register context
            if(loadedContexts.Contains(context) == false)
                loadedContexts.Add(context);

            return asm;
        }

        public Assembly LoadAssembly(byte[] assemblyImage, byte[] symbolsImage = null)
        {
            // Create context
            AssemblyLoadContext context = new AssemblyLoadContext(null);

            // Load the assembly context
            Assembly asm = context.LoadFromStream(
                new MemoryStream(assemblyImage),
                symbolsImage != null ? new MemoryStream(symbolsImage) : null);

            // Register context
            if (loadedContexts.Contains(context) == false)
                loadedContexts.Add(context);

            return asm;
        }

        public string GetTypeId(Type type)
        {
            return type.Assembly != typeof(ScriptableProvider).Assembly
                ? $"{type.FullName}, {type.Assembly.GetName().Name}"
                : type.FullName;
        }

        public Type ResolveType(string typeId)
        {
            return Type.GetType(typeId, false);
        }
    }
}
