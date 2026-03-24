using System.Runtime.Serialization;

namespace KoraGame
{
    public abstract class Component : GameElement
    {
        // Private
        [DataMember(Name = "Active")]
        private bool active = false;

        // Internal
        internal GameObject gameObject = null;

        // Properties
        public bool Active => active;
        public bool ActiveInScene => active == true && gameObject != null && gameObject.ActiveInScene == true;

        public Scene Scene => gameObject?.Scene;
        public GameObject GameObject => gameObject;

        // Methods 
        protected virtual void OnEnable() { }
        protected virtual void OnDisable() { }

        internal virtual void RegisterSubSystems() { }
        internal virtual void UnregisterSubSystems() { }

        internal override void CloneInstantiate(GameElement element)
        {
            // Clone base
            base.CloneInstantiate(element);

            // Clone component
            Component component = (Component)element;

            // Copy active
            component.active = active;
        }

        public void SetActive(bool on)
        {
            // Check for no change
            if (active == on)
                return;

            this.active = on;

            // Trigger event
            if (on == true)
            {
                // Register the component with the scene
                RegisterSubSystems();

                try
                {
                    // Trigger enable
                    OnEnable();
                }
                catch(Exception e)
                {
                    Debug.LogException(e);
                }
            }
            else
            {
                try
                {
                    // Trigger disable
                    OnDisable();
                }
                catch(Exception e)
                {
                    Debug.LogException(e);
                }

                // Unregister the component
                UnregisterSubSystems();
            }
        }
    }
}
