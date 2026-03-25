using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace KoraGame
{
    public abstract class GameElement
    {
        // Private
        [DataMember(Name = "Name")]
        private string name;

        private Game game;
        private bool isDestroying = false;
        private bool isDestroyed = false;
        private bool isInstance = false;

        private GCHandle weakHandle = default;

        // Internal
        internal readonly Type elementType;

        // Properties
        public string Name
        {
            get => name;
            set
            {
                name = value;
            }
        }
        public Game Game => game;
        public bool IsDestroyed => isDestroying == true || isDestroyed == true;

        public virtual bool IsAsset => !isInstance;

        internal GCHandle WeakHandle
        {
            get
            {
                // Create the handle
                if (weakHandle.IsAllocated == false && this.isDestroyed == false)
                    weakHandle = GCHandle.Alloc(this, GCHandleType.Weak);

                // Get the weak handle
                return weakHandle;
            }
        }

        internal IntPtr WeakPtr
        {
            get
            {
                GCHandle handle = WeakHandle;

                // Get ptr or null
                return handle.IsAllocated == true
                    ? GCHandle.ToIntPtr(handle) 
                    : IntPtr.Zero;
            }
        }

        // Constructor
        protected GameElement()
        {
            this.game = Game.Instance;
            this.elementType = GetType();
        }

        protected GameElement(string name)
        {
            this.game = Game.Instance;
            this.elementType = GetType();

            this.name = name;
            this.isInstance = true;
        }

        ~GameElement()
        {
            // Free handle
            if (weakHandle.IsAllocated == true)
                weakHandle.Free();
        }

        // Methods
        public override string ToString()
        {
            return $"{GetType().Name} ({Name})";
        }

        public override bool Equals(object obj)
        {
            // Check for same element
            if (obj is GameElement element)
                return this == element;

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        internal virtual void CloneInstantiate(GameElement element)
        {
            element.name = name;
            element.game = game;
        }

        internal virtual T OnInstantiate<T>() where T : GameElement, new()
        {
            // Create copy
            T clone = new T();

            // Perform clone
            CloneInstantiate(clone);
            return clone;
        }
        protected virtual void OnDestroy() { }

        public static T Instantiate<T>(T element) where T : GameElement, new()
        {
            // Check for null
            if (element == null)
                return null;

            // Create instance
            T clone = element.OnInstantiate<T>();

            // Setup as clone
            clone.isInstance = true;
            clone.isDestroying = false;

            return clone;
        }

        public static void Destroy(GameElement element)
        {
            // Check for null
            if (element == null || element.IsDestroyed == true)
                return;

            // Check for asset
            if (element.IsAsset == true)
                throw new InvalidOperationException("Destroy assets is not permitted. Use 'Assets.Unload' if you really want to destroy the asset");

            // Set destroying flag so that the object is marked as destroyed for game logic
            element.isDestroying = true;

            // Add to destroy queue
            element.Game?.DestroyDelayed(element);
        }

        internal static void DestroyImmediate(GameElement element)
        {
            // Check for null
            if (element == null || element.isDestroyed == true)
                return;

            // Set destroyed state
            element.isDestroying = true;
            element.isDestroyed = true;

            // Trigger object destroy
            element.OnDestroy();
        }

        internal static T FromWeakHandle<T>(in GCHandle handle) where T : GameElement
        {
            // Try to get referenced object as T
            if (handle.IsAllocated == true && handle.Target != null && handle.Target is T t)
                return t;

            // handle is invald or was collected
            return null;
        }

        internal static T FromWeakPtr<T>(in IntPtr ptr) where T : GameElement
        {
            if(ptr != IntPtr.Zero)
            {
                // Get from intptr
                GCHandle handle = GCHandle.FromIntPtr(ptr);

                // Try to get object
                return FromWeakHandle<T>(handle);
            }
            return null;
        }

        public static bool operator==(GameElement a, GameElement b)
        {
            // Check for null A
            if (a is null && b is not null)
                return b.IsDestroyed == true;

            // Check for null B
            if(b is null && a is not null)
                return a.IsDestroyed == true;

            // Fallback to default
            return ReferenceEquals(a, b);
        }

        public static bool operator!=(GameElement a, GameElement b)
        {
            return !(a == b);
        }
    }
}
