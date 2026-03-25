
namespace KoraEditor
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class PropertyEditorForAttribute : Attribute
    {
        // Private
        private Type forType = null;
        private bool forDerivedTypes = false;

        // Properties
        public Type ForType
        {
            get { return forType; }
        }

        public bool ForDerivedTypes
        {
            get { return forDerivedTypes; }
        }

        // Constructor
        public PropertyEditorForAttribute(Type forType, bool forDerivedTypes = false)
        {
            this.forType = forType;
            this.forDerivedTypes = forDerivedTypes;
        }
    }
}
