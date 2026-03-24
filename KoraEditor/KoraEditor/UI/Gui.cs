using ImGuiNET;
using KoraGame;
using System.Numerics;

namespace KoraEditor.UI
{
    public static class Gui
    {
        // Methods
        public static void Label(GuiContent content)
        {
            ImGui.Text(content.Text);
            DoContent(content);
        }

        public static void Button(GuiContent content, Action onClick)
        {            
            if (ImGui.Button(content.Text))
                onClick();
            DoContent(content);
        }

        public static void Toggle(GuiContent content, ref bool value)
        {
            ImGui.Checkbox(content.Text, ref value);
            DoContent(content);
        }

        public static void Input(GuiContent content, ref string value)
        {
            ImGui.InputText(content.Text, ref value, 256);
            DoContent(content);
        }

        public static void InputNumber(GuiContent content, ref int value)
        {
            ImGui.InputInt(content.Text, ref value);
            DoContent(content);
        }

        public static void InputNumber(GuiContent content, ref float value)
        {
            ImGui.InputFloat(content.Text, ref value);
            DoContent(content);
        }   

        public static void Slider(GuiContent content, ref int value, int min, int max)
        {
            ImGui.SliderInt(content.Text, ref value, min, max);
            DoContent(content);
        }

        public static void Slider(GuiContent content, ref float value, float min, float max)
        {
            ImGui.SliderFloat(content.Text, ref value, min, max);
            DoContent(content);
        }

        public static void ColorPicker(GuiContent content, ref Color color)
        {
            Vector4 colorVec = (Vector4)color;
            if (ImGui.ColorPicker4(content.Text, ref colorVec))
                color = (Color)colorVec;
            DoContent(content);
        }

        public static void ColorPicker(GuiContent content, ref Color32 color)
        {
            Vector4 colorVec = (Vector4)(Color)color;
            if (ImGui.ColorPicker4(content.Text, ref colorVec))
                color = (Color32)(Color)colorVec;
            DoContent(content);
        }

        public static void Separator()
        {
            ImGui.Separator();
        }

        public static void Space()
        {
            ImGui.Spacing();
        }

        private static void DoContent(GuiContent content)
        {
            // Tooltip
            if (content.Tooltip != null)
                ImGui.SetItemTooltip(content.Tooltip);
        }


    }
}
