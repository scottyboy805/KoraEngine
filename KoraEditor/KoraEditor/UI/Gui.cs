using ImGuiNET;
using KoraGame;
using KoraGame.Graphics;
using System.Numerics;

namespace KoraEditor.UI
{
    public static class Gui
    {
        // Private
        private static int idCounter = 1;

        // Methods
        internal static void NewFrame()
        {
            idCounter = 1;
        }

        public static void Label(GuiContent content)
        {
            BeginControl(content);
            ImGui.Text(content.Text);
            EndControl(content);
        }

        public static void Button(GuiContent content, Action onClick)
        {
            BeginControl(content);
            if (ImGui.Button(content.Text))
                onClick();
            EndControl(content);
        }

        public unsafe static void Image(Texture texture, Vector2F size, GuiContent content = default)
        {
            BeginControl(content);
            ImGui.Image(texture.WeakPtr, (Vector2)size);
            EndControl(content);
        }

        public static bool Toggle(ref bool value, GuiContent content = default)
        {
            BeginControl(content);
            bool changed = ImGui.Checkbox("", ref value);
            EndControl(content);
            return changed;
        }

        public static bool Input(ref string value, uint maxLength = 256, GuiContent content = default)
        {
            BeginControl(content);
            bool changed = ImGui.InputText("", ref value, maxLength);
            EndControl(content);
            return changed;
        }

        public static bool InputMultiline(ref string value, uint maxLength = 1024, GuiContent content = default)
        {
            BeginControl(content);
            bool changed = ImGui.InputTextMultiline("", ref value, maxLength, new Vector2(-1, 100));
            EndControl(content);
            return changed;
        }

        public static bool InputNumber(ref int value, GuiContent content = default)
        {
            BeginControl(content);
            bool changed = ImGui.InputInt("", ref value);
            EndControl(content);
            return changed;
        }

        public static bool InputNumber(ref float value, GuiContent content = default)
        {
            BeginControl(content);
            bool changed = ImGui.InputFloat("", ref value);
            EndControl(content);
            return changed;
        }   

        public static bool Slider(ref int value, int min, int max, GuiContent content = default)
        {
            BeginControl(content);
            bool changed = ImGui.SliderInt("", ref value, min, max);
            EndControl(content);
            return changed;
        }

        public static bool Slider(ref float value, float min, float max, GuiContent content = default)
        {
            BeginControl(content);
            bool changed = ImGui.SliderFloat("", ref value, min, max);
            EndControl(content);
            return changed;
        }

        public static bool ColorPicker(ref Color color, GuiContent content = default)
        {
            BeginControl(content);
            Vector4 colorVec = (Vector4)color;
            bool changed = false;
            if (ImGui.ColorPicker4("", ref colorVec))
            {
                color = (Color)colorVec;
                changed = true;
            }
            EndControl(content);
            return changed;
        }

        public static bool ColorPicker(ref Color32 color, GuiContent content = default)
        {
            BeginControl(content);
            Vector4 colorVec = (Vector4)(Color)color;
            bool changed = false;
            if (ImGui.ColorPicker4("", ref colorVec))
            {
                color = (Color32)(Color)colorVec;
                changed = true;
            }
            EndControl(content);
            return changed;
        }

        public static bool Popup(ref int selected, string[] items, GuiContent content = default)
        {
            // Check for any
            if(items == null || items.Length == 0)
                return false;                       

            BeginControl(content);
            bool changed = ImGui.Combo("", ref selected, items, items.Length);
            EndControl(content);

            return changed;
        }

        public static bool EnumPopup(ref Enum value, GuiContent content = default)
        {
            BeginControl(content);

            // Get the option names
            string[] names = Enum.GetNames(value.GetType());

            // Display combo
            int current = Array.IndexOf(names, value.ToString());
            bool changed = ImGui.Combo("", ref current, names, names.Length);

            // Update changed
            if(changed == true)
                value = (Enum)Enum.Parse(value.GetType(), names[current]);

            EndControl(content);
            return changed;
        }

        public static bool EnumPopup<T>(ref T value, GuiContent content = default) where T : Enum
        {
            BeginControl(content);

            // Get the option names
            string[] names = Enum.GetNames(value.GetType());

            // Display combo
            int current = Array.IndexOf(names, value.ToString());
            bool changed = ImGui.Combo("", ref current, names, names.Length);

            // Update changed
            if (changed == true)
                value = (T)Enum.Parse(value.GetType(), names[current]);

            EndControl(content);
            return changed;
        }

        public static void Separator()
        {
            ImGui.Separator();
        }

        public static void Space()
        {
            ImGui.Spacing();
        }

        private static void BeginControl(in GuiContent content)
        {
            // Get id
            if (string.IsNullOrEmpty(content.Id) == false)
            {
                ImGui.PushID(content.Id);
            }
            else
            {
                ImGui.PushID(idCounter++);
            }
        }

        private static void EndControl(in GuiContent content)
        {
            // Tooltip
            if (string.IsNullOrEmpty(content.Tooltip) == false)
                ImGui.SetItemTooltip(content.Tooltip);

            ImGui.PopID();
        }
    }
}
