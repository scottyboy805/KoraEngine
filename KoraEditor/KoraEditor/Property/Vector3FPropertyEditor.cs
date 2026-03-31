using KoraEditor.UI;
using KoraGame;

namespace KoraEditor.Property
{
    [PropertyEditorFor(typeof(Vector3F))]
    internal sealed class Vector3FPropertyEditor : PropertyEditor
    {
        // Private
        private Vector3F defaultValue = default;
        private Vector3F vectorValue;
        private float[] columnSizes = new float[3];

        // Public
        public static readonly Color xNormal = new Color(0.8f, 0.1f, 0.15f);
        public static readonly Color xHover = new Color(0.9f, 0.2f, 0.2f);

        public static readonly Color yNormal = new Color(0.2f, 0.7f, 0.2f);
        public static readonly Color yHover = new Color(0.3f, 0.8f, 0.3f);

        public static readonly Color zNormal = new Color(0.1f, 0.25f, 0.8f);
        public static readonly Color zHover = new Color(0.2f, 0.35f, 0.9f);

        // Methods
        protected override void OnCreate()
        {
            // Try to get vector value
            vectorValue = Element.GetValue<Vector3F>();
        }

        protected override void OnValueGui()
        {
            for (int i = 0; i < columnSizes.Length; i++)
                columnSizes[i] = Gui.AvailableSize.X / 3f;

            Gui.BeginTableLayout(3, columnSizes);
            {
                Gui.BeginLayout(GuiLayout.Horizontal);
                {
                    // X value
                    Gui.NextItemColor(xNormal, xHover, xNormal);
                    Gui.Button("X", ResetX);
                    if (Gui.InputNumber(ref vectorValue.X) == true)
                    {
                        // Set value
                        Element.SetValue(vectorValue);

                        // Set modified
                        SetModified();
                    }
                }
                Gui.EndLayout();

                Gui.ColumnSeparator();

                Gui.BeginLayout(GuiLayout.Horizontal);
                {
                    // Y value
                    Gui.NextItemColor(yNormal, yHover, yNormal);
                    Gui.Button("Y", ResetY);
                    if (Gui.InputNumber(ref vectorValue.Y) == true)
                    {
                        // Set value
                        Element.SetValue(vectorValue);

                        // Set modified
                        SetModified();
                    }
                }
                Gui.EndLayout();

                Gui.ColumnSeparator();

                Gui.BeginLayout(GuiLayout.Horizontal);
                {
                    // Z value
                    Gui.NextItemColor(zNormal, zHover, zNormal);
                    Gui.Button("Z", ResetZ);
                    if (Gui.InputNumber(ref vectorValue.Z) == true)
                    {
                        // Set value
                        Element.SetValue(vectorValue);

                        // Set modified
                        SetModified();
                    }
                }
                Gui.EndLayout();
            }
            Gui.EndTableLayout();
        }

        private void ResetX()
        {
            // Apply modify
            vectorValue.X = defaultValue.X;
            Element.SetValue(vectorValue);

            // Set dirty
            SetModified();
        }

        private void ResetY()
        {
            // Apply modify
            vectorValue.Y = defaultValue.Y;
            Element.SetValue(vectorValue);

            // Set dirty
            SetModified();
        }

        private void ResetZ()
        {
            // Apply modify
            vectorValue.Z = defaultValue.Z;
            Element.SetValue(vectorValue);

            // Set dirty
            SetModified();
        }
    }
}
