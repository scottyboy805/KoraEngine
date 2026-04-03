
namespace KoraGame
{
    /// <summary>
    /// Used to indicate that a field or property should be hidden in the editor. This is useful for fields that are used for internal state or caching, and should not be exposed to the user.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class EditorHiddenAttribute : Attribute
    {
        // Empty class
    }

    /// <summary>
    /// Used to indicate that a field or property should be visible in the editor but not modifiable. This is useful for fields that are used for internal state or caching, and should not be modified by the user.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class EditorReadOnlyAttribute : Attribute
    {
        // Empty class
    }

    /// <summary>
    /// Used to give a field or property a different display name in the editor.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class EditorNameAttribute : Attribute
    {
        // Public
        public readonly string DisplayName;

        // Constructor
        public EditorNameAttribute(string displayName)
        {
            this.DisplayName = displayName;
        }
    }

    /// <summary>
    /// Used to give a UI tooltip for a field or property in the editor. This is useful for providing additional information about the field or property to the user, such as what it does or how it should be used.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class EditorTooltipAttribute : Attribute
    {
        // Public
        public readonly string Tooltip;

        // Constructor
        public EditorTooltipAttribute(string tooltip)
        {
            this.Tooltip = tooltip;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class EditorIconAttribute : Attribute
    {
        // Public
        public readonly string Path;

        // Constructor
        public EditorIconAttribute(string path)
        {
            this.Path = path;
        }
    }

    /// <summary>
    /// Used to specify that a UI editor for a field or property should be a slider with a specified range. This is useful for fields that have a known range of valid values, such as a percentage or a value that should be clamped between two limits.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class EditorRangeAttribute : Attribute
    {
        // Public
        public readonly float Min;
        public readonly float Max;

        // Constructor
        public EditorRangeAttribute(float min, float max)
        {
            this.Min = min;
            this.Max = max;
        }

        public EditorRangeAttribute(int min, int max)
        {
            this.Min = min;
            this.Max = max;
        }
    }
}
