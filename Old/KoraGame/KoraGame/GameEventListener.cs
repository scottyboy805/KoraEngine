using System.Reflection;
using System.Runtime.Serialization;

namespace KoraGame
{
    [DataContract]
    public class GameEventPersistentListener : GameEventListener, IAssetSerialize
    {
        // Private
        [DataMember(Name = "InvokeElement")]
        private GameElement invokeElement = null;
        [DataMember(Name = "InvokeElement")]
        private string methodName = "";

        // Properties
        public GameElement TargetElement
        {
            get { return invokeElement; }
        }

        public string MethodName
        {
            get { return methodName; }
        }

        // Constructor
        internal GameEventPersistentListener() { }

        internal GameEventPersistentListener(GameElement targetInstance, MethodInfo targetMethod)
            : base(targetInstance, targetMethod)
        {
            // Check for target method
            if (targetMethod == null)
                throw new ArgumentNullException(nameof(targetMethod));

            this.invokeElement = targetInstance;
        }

        // Methods
        public void ResolveTargetMethod()
        {
            if (invokeMethod == null)
            {
                if (invokeElement != null)
                {
                    // Try to get method
                    invokeMethod = invokeElement.GetType().GetMethod(methodName);
                }
            }
        }

        void IAssetSerialize.OnSerialize()
        {
            methodName = invokeMethod.Name;
        }

        void IAssetSerialize.OnDeserialize()
        {
            ResolveTargetMethod();
        }
    }

    public class GameEventListener
    {
        // Protected
        protected object invokeInstance = null;
        protected MethodInfo invokeMethod = null;

        // Properties      
        public object TargetInstance
        {
            get { return invokeInstance; }
        }

        public MethodBase Method
        {
            get { return invokeMethod; }
        }

        // Constructor
        internal GameEventListener() { }

        internal GameEventListener(object targetInstance, MethodInfo targetMethod)
        {
            this.invokeInstance = targetInstance;
            this.invokeMethod = targetMethod;
        }

        // Methods
        public void DynamicInvoke()
        {
            if (invokeMethod != null)
            {
                if (invokeMethod.IsStatic == true)
                {
                    invokeMethod.Invoke(null, null);
                }
                else if (invokeInstance != null)
                {
                    invokeMethod.Invoke(invokeInstance, null);
                }
            }
        }

        public void DynamicInvoke(object[] args)
        {
            if (invokeMethod != null)
            {
                if (invokeMethod.IsStatic == true)
                {
                    invokeMethod.Invoke(null, args);
                }
                else if (invokeInstance != null)
                {
                    invokeMethod.Invoke(invokeInstance, args);
                }
            }
        }
    }
}
