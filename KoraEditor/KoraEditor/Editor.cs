using SDL;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("KoraEditor-Windows")]

namespace KoraEditor
{
    public sealed class Editor
    {
        // Private
        private static Editor instance;

        private bool quit = false;

        // Properties
        internal static Editor Instance => instance;

        public bool Quit => quit;

        // Constructor
        internal Editor()
        {
            instance = this;
        }

        ~Editor()
        {
            if (instance == this)
                instance = null;
        }

        // Methods
        internal void DoInitialize()
        {

        }

        internal void DoUpdate()
        {

        }

        internal void DoShutdown()
        {

        }

        internal void DoEvent(in SDL_Event evt)
        {

        }
    }
}
