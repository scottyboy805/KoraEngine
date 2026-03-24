using System.Reflection;
using System.Runtime.Serialization;

namespace KoraGame
{
    [Serializable]
    public class GameEvent<T0, T1, T2, T3> : GameEventBase
    {
        // Delegate
        public delegate void GameAction(T0 arg0, T1 arg1, T2 arg2, T3 arg3);

        // Constructor
        public GameEvent() { }

        // Methods
        public void Raise(T0 arg0, T1 arg1, T1 arg2, T3 arg3)
        {
            DynamicInvoke(new object[] { arg0, arg1, arg2, arg3 });
        }

        public void AddListener(GameAction action)
        {
            AddListener(action.Target, action.Method);
        }

        public void RemoveListener(GameAction action)
        {
            RemoveListener(action.Target, action.Method);
        }
    }

    [Serializable]
    public class GameEvent<T0, T1, T2> : GameEventBase
    {
        // Delegate
        public delegate void GameAction(T0 arg0, T1 arg1, T2 arg2);

        // Constructor
        public GameEvent() { }

        // Methods
        public void Raise(T0 arg0, T1 arg1, T1 arg2)
        {
            DynamicInvoke(new object[] { arg0, arg1, arg2 });
        }

        public void AddListener(GameAction action)
        {
            AddListener(action.Target, action.Method);
        }

        public void RemoveListener(GameAction action)
        {
            RemoveListener(action.Target, action.Method);
        }
    }

    [Serializable]
    public class GameEvent<T0, T1> : GameEventBase
    {
        // Delegate
        public delegate void GameAction(T0 arg0, T1 arg1);

        // Constructor
        public GameEvent() { }

        // Methods
        public void Raise(T0 arg0, T1 arg1)
        {
            DynamicInvoke(new object[] { arg0, arg1 });
        }

        public void AddListener(GameAction action)
        {
            AddListener(action.Target, action.Method);
        }

        public void RemoveListener(GameAction action)
        {
            RemoveListener(action.Target, action.Method);
        }
    }

    [Serializable]
    public class GameEvent<T> : GameEventBase
    {
        // Delegate
        public delegate void GameAction(T arg);

        // Constructor
        public GameEvent() { }

        // Methods
        public void Raise(T arg)
        {
            DynamicInvoke(new object[] { arg });
        }

        public void AddListener(GameAction action)
        {
            AddListener(action.Target, action.Method);
        }

        public void RemoveListener(GameAction action)
        {
            RemoveListener(action.Target, action.Method);
        }
    }

    [Serializable]
    public class GameEvent : GameEventBase
    {
        // Delegate
        public delegate void GameAction();

        // Constructor
        public GameEvent() { }

        // Methods
        public void Raise()
        {
            DynamicInvoke();
        }

        public void AddListener(GameAction action)
        {
            AddListener(action.Target, action.Method);
        }

        public void RemoveListener(GameAction action)
        {
            AddListener(action.Target, action.Method);
        }
    }


    [Serializable]
    public abstract class GameEventBase
    {
        // Protected
        [DataMember(Name = "Listeners")]
        protected List<GameEventListener> listeners = new List<GameEventListener>();

        // Properties
        public IReadOnlyList<GameEventListener> Listeners
        {
            get { return listeners; }
        }

        // Methods
        public void DynamicInvoke()
        {
            foreach (GameEventListener listener in listeners)
            {
                try
                {
                    listener.DynamicInvoke();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        public void DynamicInvoke(object[] args)
        {
            foreach (GameEventListener listener in listeners)
            {
                try
                {
                    listener.DynamicInvoke(args);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        public void RemoveAllListeners()
        {
            listeners.Clear();
        }

        protected void AddListener(object instance, MethodInfo method)
        {
            if (instance is GameElement)
            {
                listeners.Add(new GameEventPersistentListener((GameElement)instance, method));
            }
            else
            {
                listeners.Add(new GameEventListener(instance, method));
            }
        }

        protected void RemoveListener(object instance, MethodInfo method)
        {
            for (int i = 0; i < listeners.Count; i++)
            {
                if (listeners[i].TargetInstance == instance && listeners[i].Method == method)
                {
                    listeners.RemoveAt(i);
                    break;
                }
            }
        }
    }
}
