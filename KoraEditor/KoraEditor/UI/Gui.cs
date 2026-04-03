using ImGuiNET;
using KoraGame;
using System.Numerics;

namespace KoraEditor.UI
{
    [Flags]
    public enum GuiLayoutOptions
    {
        None = 0,
        Border = 1,
        AutoSizeX = 0x10,
        AutoSizeY = 0x20,
        Frame = 0x80,
        Vertical = 0x200,
        Horizontal = 0x400,
        Continue = 0x800,
        Empty = 0x1600,
    }

    [Flags]
    public enum GuiTreeOptions
    {
        None = 0,
        Selected = 1,
        Framed = 2,
        OpenDefault = 0x20,
        NoArrow = 0x100,
    }

    public static class Gui
    {
        // Type
        [Flags]
        private enum GuiLayoutKind
        {
            Vertical = 1,
            Horizontal = 2,

            Child = 1 << 24,
        }

        private struct GuiStyle
        {
            // Public
            public Color? NormalColor;
            public Color? HoverColor;
            public Color? ActiveColor;
        }

        // Private
        private static int idCounter = 1;
        private static Stack<GuiLayoutKind> layouts = new();
        private static Stack<GuiStyle> styles = new();

        // Properties
        public static Vector2F Position
        {
            get => (Vector2F)ImGui.GetCursorPos();
            set => ImGui.SetCursorPos((Vector2)value);
        }
        public static Vector2F AvailableSize => (Vector2F)ImGui.GetContentRegionAvail();
        public static float PropertyLableWidth => AvailableSize.X * 0.4f;
        public static float PropertyValueWidth => AvailableSize.X * 0.6f;

        // Methods
        internal static void NewFrame()
        {
            idCounter = 1;
        }

        internal static void EndFrame()
        {
            if (layouts.Count > 0)
                throw new InvalidOperationException("Expected closing Gui layout: " + layouts.Peek());
        }

        public static void ItemWidth(float width)
        {
            ImGui.PushItemWidth(width);
        }

        public static void NextItemColor(Color? normal = null, Color? hover = null, Color? active = null)
        {
            if (normal == null && hover == null && active == null)
                return;

            // Push the style
            styles.Push(new GuiStyle
            {
                NormalColor = normal,
                HoverColor = hover,
                ActiveColor = active
            });
        }

        #region PrimitiveShapes
        public static void DrawRectangle(Vector2F position, Vector2F size, Color color)
        {
            // Get the draw list
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();

            // Get the window pos
            position += (Vector2F)ImGui.GetCursorPos();

            // Add rect
            drawList.AddRectFilled(((Vector2)position), (Vector2)(position + size), ((Color32)color).RGBA);
        }
        #endregion

        public static void Label(GuiContent content)
        {
            BeginControl(content, ImGuiCol.Text, null, null);
            ImGui.Text(content.Text);
            EndControl(content);
        }

        public static void PropertyLabel(EditorSerializedProperty element)
        {
            // Create content
            GuiContent content = new GuiContent(
                element.DisplayName,
                element.Tooltip);

            Label(content);
        }

        public static void Button(GuiContent content, Action onClick = null)
        {
            BeginControl(content, ImGuiCol.Button, ImGuiCol.ButtonHovered, ImGuiCol.ButtonActive);
            if (ImGui.Button(content.Text) == true && onClick != null)
                onClick();
            EndControl(content);
        }

        public static void Image(GuiContent content, Vector2F size)
        {
            BeginControl(content, null, null, null);
            ImGui.Image(content.IconPtr, (Vector2)size);
            EndControl(content);
        }

        public static void ImageButton(GuiContent content, Vector2F size, Action onClick = null)
        {
            BeginControl(content, ImGuiCol.Button, ImGuiCol.ButtonHovered, ImGuiCol.ButtonHovered);
            if (ImGui.ImageButton("", content.IconPtr, (Vector2)size) && onClick != null)
                onClick();
            EndControl(content);
        }

        public static bool Toggle(ref bool value, GuiContent content = default)
        {
            BeginControl(content, ImGuiCol.CheckMark, null, null);
            bool changed = ImGui.Checkbox("", ref value);
            EndControl(content);
            return changed;
        }

        public static bool Input(ref string value, uint maxLength = 256, GuiContent content = default)
        {
            // Check for null
            if (value == null)
                value = "";

            BeginControl(content, ImGuiCol.Text, null, null);
            bool changed = ImGui.InputText("", ref value, maxLength);
            EndControl(content);
            return changed;
        }

        public static bool InputMultiline(ref string value, uint maxLength = 1024, GuiContent content = default)
        {            
            BeginControl(content, ImGuiCol.Text, null, null);
            bool changed = ImGui.InputTextMultiline("", ref value, maxLength, new Vector2(-1, 100));
            EndControl(content);
            return changed;
        }

        public static bool InputNumber(ref int value, GuiContent content = default)
        {
            BeginControl(content, ImGuiCol.Text, null, null);
            bool changed = ImGui.InputInt("", ref value);
            EndControl(content);
            return changed;
        }

        public static bool InputNumber(ref float value, GuiContent content = default)
        {
            BeginControl(content, ImGuiCol.Text, null, null);
            bool changed = ImGui.InputFloat("", ref value);
            EndControl(content);
            return changed;
        }   

        public static bool Slider(ref int value, int min, int max, GuiContent content = default)
        {
            BeginControl(content, ImGuiCol.SliderGrab, null, ImGuiCol.SliderGrabActive);
            bool changed = ImGui.SliderInt("", ref value, min, max);
            EndControl(content);
            return changed;
        }

        public static bool Slider(ref float value, float min, float max, GuiContent content = default)
        {
            BeginControl(content, ImGuiCol.SliderGrab, null, ImGuiCol.SliderGrabActive);
            bool changed = ImGui.SliderFloat("", ref value, min, max);
            EndControl(content);
            return changed;
        }

        public static bool ColorPicker(ref Color color, GuiContent content = default)
        {
            BeginControl(content, null, null, null);
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
            BeginControl(content, null, null, null);
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

            BeginControl(content, ImGuiCol.Text, null, null);
            bool changed = ImGui.Combo("", ref selected, items, items.Length);
            EndControl(content);

            return changed;
        }

        public static bool EnumPopup(ref Enum value, GuiContent content = default)
        {
            BeginControl(content, ImGuiCol.Text, null, null);

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
            BeginControl(content, ImGuiCol.Text, null, null);

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

        public static void Space()
        {
            BeginControl(default, null, null, null);
            ImGui.Spacing();
            EndControl(default);
        }

        public static void BeginLayout(GuiLayoutOptions options = 0, Vector2F size = default, string id = null)
        {
            GuiLayoutKind kind = (options & GuiLayoutOptions.Horizontal) != 0
                ? GuiLayoutKind.Horizontal : GuiLayoutKind.Vertical;

            // Check for child
            if ((options & GuiLayoutOptions.Empty) == 0)
                kind |= GuiLayoutKind.Child;

            // Check for continue
            if ((options & GuiLayoutOptions.Continue) != 0)
                ImGui.SameLine();

            // Push layout
            layouts.Push(kind);

            // Don't pass extra flags to imgui
            options &= ~(GuiLayoutOptions.Vertical | GuiLayoutOptions.Horizontal | GuiLayoutOptions.Continue | GuiLayoutOptions.Empty);

            // Get layout flags
            ImGuiChildFlags flags = (ImGuiChildFlags)options | ImGuiChildFlags.AutoResizeY | ImGuiChildFlags.NavFlattened;

            // Start a child?
            if ((kind & GuiLayoutKind.Child) != 0)
            {
                ImGui.BeginChild(string.IsNullOrEmpty(id) == true
                    ? (idCounter++).ToString()
                    : id, (Vector2)size, flags);
            }
        }

        public static void EndLayout()
        {
            // Pop layout
            if (layouts.Count == 0)
                throw new InvalidOperationException("Attempted to end too many layouts");

            GuiLayoutKind kind = layouts.Pop();

            // End child if it was started
            if((kind & GuiLayoutKind.Child) != 0)
                ImGui.EndChild();            
        }

        public static void BeginTableLayout(int columns, IList<float> columnWidths = null, string id = null)
        {
            // Check resizable
            ImGuiTableFlags flags = ImGuiTableFlags.None;

            // Check resizable
            if (columnWidths == null)
                flags |= ImGuiTableFlags.Resizable;

            // Create table
            ImGui.BeginTable(string.IsNullOrEmpty(id) == true
                ? (idCounter++).ToString()
                : id, columns, flags);

            // Setup columns widths
            if(columnWidths != null)
            {
                for (int i = 0; i < columns && i < columnWidths.Count; i++)
                    ImGui.TableSetupColumn(i.ToString(), ImGuiTableColumnFlags.WidthFixed, columnWidths[i]);
            }

            // Start first column
            ImGui.TableNextColumn();
        }

        public static void BeginTableLayout(IList<string> columnNames, IList<float> columnWidths = null, string id = null)
        {
            // Check resizable
            ImGuiTableFlags flags = ImGuiTableFlags.None;

            // Check resizable
            if (columnWidths == null)
                flags |= ImGuiTableFlags.Resizable;

            // Create table

            ImGui.BeginTable(string.IsNullOrEmpty(id) == true
                ? (idCounter++).ToString()
                : id, columnNames.Count, ImGuiTableFlags.Borders);

            // Setup columns widths
            if (columnWidths != null)
            {
                for (int i = 0; i < columnNames.Count && i < columnWidths.Count; i++)
                    ImGui.SetColumnWidth(i, columnWidths[i]);
            }

            // Create headers
            foreach (string header in columnNames)
                ImGui.TableSetupColumn(header);

            // Draw headers
            ImGui.TableHeadersRow();

            // Start first column
            ImGui.TableNextColumn();
        }

        public static void EndTableLayout()
        {
            ImGui.EndTable();
        }

        public static void ColumnSeparator()
        {
            ImGui.TableNextColumn();
        }

        public static void RowSeparator()
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
        }

        public static bool BeginTreeNode(GuiContent content, GuiTreeOptions options = 0, Action onSelect = null)
        {
            ImGuiTreeNodeFlags flags = (ImGuiTreeNodeFlags)options | ImGuiTreeNodeFlags.SpanFullWidth;

            BeginControl(content, null, null, null);
            bool expanded = ImGui.TreeNodeEx("", flags);

            // Trigger click
            if (onSelect != null && ImGui.IsItemClicked(ImGuiMouseButton.Left) == true)
                onSelect();

            // Image
            if(content.Icon != null)
            {
                ImGui.SameLine();
                Position += new Vector2F(0, 4);
                ImGui.Image(content.IconPtr, new Vector2(32, 32));
            }

            // Label
            ImGui.SameLine();
            Label(content);
            EndControl(content);
            return expanded;
        }

        public static void EndTreeNode()
        {
            ImGui.TreePop();
        }

        private static void BeginControl(in GuiContent content, ImGuiCol? normal, ImGuiCol? hover, ImGuiCol? active)
        {
            // Check layout
            if(layouts.Count > 0 && (layouts.Peek() & GuiLayoutKind.Horizontal) != 0)
                ImGui.SameLine();

            // Get id
            if (string.IsNullOrEmpty(content.Id) == false)
            {
                ImGui.PushID(content.Id);
            }
            else
            {
                ImGui.PushID(idCounter++);
            }

            // Push styles
            if(styles.Count > 0)
            {
                GuiStyle style = styles.Peek();

                // Push normal color style
                if(style.NormalColor != null && normal != null)
                    ImGui.PushStyleColor(normal.Value, (Vector4)style.NormalColor.Value);

                // Push hover color style
                if(style.HoverColor != null && hover != null)
                    ImGui.PushStyleColor(hover.Value, (Vector4)style.HoverColor.Value);

                // Push active color style
                if(style.ActiveColor != null && active != null)
                    ImGui.PushStyleColor(active.Value, (Vector4)style.ActiveColor.Value);
            }
        }

        private static void EndControl(in GuiContent content)
        {
            // Tooltip
            if (string.IsNullOrEmpty(content.Tooltip) == false)
                ImGui.SetItemTooltip(content.Tooltip);

            ImGui.PopID();

            // Pop styles
            if(styles.Count > 0)
            {
                GuiStyle style = styles.Pop();

                // Count styles
                int count = 0;

                if (style.NormalColor != null) count++;
                if (style.HoverColor != null) count++;
                if (style.ActiveColor != null) count++;

                // Pop styles
                ImGui.PopStyleColor(count);
            }
        }
    }
}
