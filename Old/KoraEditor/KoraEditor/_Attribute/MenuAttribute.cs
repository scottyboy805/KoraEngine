
namespace KoraEditor
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class MenuAttribute : Attribute
    {
        // Private
        private readonly string menuPath;
        private readonly string shortcut;
        private readonly bool separatorBefore = false;

        private string[] menuTree;

        // Properties
        public string[] MenuTree
        {
            get
            {
                // Get the tree
                if(menuTree == null)
                    menuTree = menuPath.Split('/');

                return menuTree;
            }
        }

        public string Shortcut => shortcut;
        public bool SeparatorBefore => separatorBefore;

        // Constructor
        public MenuAttribute(string menuPath, string shotcut = "", bool separatorBefore = false)
        {
            this.menuPath = menuPath;
            this.shortcut = shotcut;
            this.separatorBefore = separatorBefore;
        }
    }
}
