using KoraGame.Graphics;
using KoraGame.Input;
using KoraGame.Physics;
using System.Runtime.Serialization;

namespace KoraGame
{
    public abstract class ScriptableBehaviour : Component
    {
        // Type
        internal sealed class BehavourComparer : IComparer<ScriptableBehaviour>
        {
            // Methods
            public int Compare(ScriptableBehaviour x, ScriptableBehaviour y)
            {
                return x.priority.CompareTo(y.priority);
            }
        }

        // Private
        [DataMember(Name = "Priority")]
        private int priority = 0;

        private bool didStart = false;

        // Internal
        internal static readonly BehavourComparer behaviourComparer = new();

        // Properties
        public int Priority
        {
            get => priority;
            set
            {
                priority = value;
            }
        }

        public Screen Screen => Game?.Screen;
        public GraphicsDevice Graphics => Game?.GraphicsDevice;
        public InputProvider Input => Game?.Input;
        public PhysicsSimulation Physics => Game?.Physics;

        // Methods
        protected virtual void OnStart() { }
        protected virtual void OnUpdate() { }

        #region AddComponent
        public T AddComponent<T>() where T : Component, new() => GameObject?.AddComponent<T>();
        public Component AddComponent(Type type) => GameObject?.AddComponent(type);
        public void AddComponent(Component component) => GameObject?.AddComponent(component);
        #endregion

        #region GetComponent
        public Component GetComponent(Type type, bool includeInactive = false) => GameObject?.GetComponent(type, includeInactive);
        public T GetComponent<T>(bool includeInactive = false) where T : class => GameObject?.GetComponent<T>(includeInactive);
        public T[] GetComponents<T>(bool includeInactive = false) where T : class => GameObject?.GetComponents<T>(includeInactive);
        public int GetComponents<T>(IList<T> results, bool includeInactive = false) where T : class => GameObject?.GetComponents<T>(results, includeInactive) ?? 0;
        public T GetComponentInChildren<T>(bool includeInactive = false, string tag = null) where T : class => GameObject?.GetComponentInChildren<T>(includeInactive, tag);
        public T[] GetComponentsInChildren<T>(bool includeInactive = false, string tag = null) where T : class => GameObject?.GetComponentsInChildren<T>(includeInactive, tag);
        public int GetComponentsInChildren<T>(IList<T> results, bool includeInactive = false, string tag = null) where T : class => GameObject?.GetComponentsInChildren<T>(results, includeInactive, tag) ?? 0;
        public T GetComponentInParent<T>(bool includeInactive = false, string tag = null) where T : class => GameObject?.GetComponentInParent<T>(includeInactive, tag);
        public T[] GetComponentsInParent<T>(bool includeInactive = false, string tag = null) where T : class => GameObject?.GetComponentsInParent<T>(includeInactive, tag);
        public int GetComponentsInParent<T>(IList<T> results, bool includeInactive = false, string tag = null) where T : class => GameObject?.GetComponentsInParent<T>(results, includeInactive, tag) ?? 0;
        #endregion

        internal override void RegisterSubSystems()
        {
            Scene?.activeBehaviours.Add(this);
            Scene?.activeBehaviours.Sort(behaviourComparer);
        }

        internal override void UnregisterSubSystems()
        {
            Scene?.activeBehaviours.Remove(this);
        }

        internal void DoStart()
        {
            if(didStart == false)
            {
                didStart = true;

                try
                {
                    OnStart();
                }
                catch(Exception e)
                {
                    Debug.LogException(e, LogFilter.Script);
                }
            }
        }

        internal void DoUpdate()
        {
            try
            {
                OnUpdate();
            }
            catch (Exception e)
            {
                Debug.LogException(e, LogFilter.Script);
            }
        }
    }
}
