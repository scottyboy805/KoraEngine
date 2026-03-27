using System.Reflection;
using ImGuiNET;
using KoraGame;

namespace KoraEditor
{
    internal sealed class Menu
    {
        // Type
        private sealed class MenuNode
        {
            // Public
            public string Name;
            public string Shortcut;
            public List<MenuNode> Children = new();
            public MethodInfo Action;
            public bool SeparatorBefore;

            // Properties
            public bool IsLeaf => Action != null;
        }

        // Private
        private readonly AttributeProvider<MenuAttribute> menuAttributes = new();
        private readonly List<MenuNode> rootNodes = new();

        // Methods
        public void RebuildMenu()
        {
            rootNodes.Clear();

            foreach (var (attr, method) in menuAttributes.GetMethodAttributes())
            {
                var parts = attr.MenuTree ?? Array.Empty<string>();
                if (parts.Length == 0)
                    continue;

                // Check for static
                if(method.IsStatic == false)
                {
                    Debug.LogError("Menu action method must be static: " + method.ToString());
                    continue;
                }

                var nodes = rootNodes;
                MenuNode current = null;
                for (int i = 0; i < parts.Length; i++)
                {
                    string part = parts[i];
                    var node = nodes.Find(n => n.Name == part);
                    if (node == null)
                    {
                        node = new MenuNode { Name = part };
                        nodes.Add(node);
                    }

                    current = node;
                    nodes = node.Children;
                }

                if (current != null)
                {
                    current.Action = method;
                    current.Shortcut = attr.Shortcut;
                    current.SeparatorBefore = attr.SeparatorBefore;
                }
            }
        }

        public void DisplayMenu()
        {
            if (!ImGui.BeginMainMenuBar())
                return;

            foreach (var node in rootNodes)
            {
                RenderNode(node);
            }

            ImGui.EndMainMenuBar();

            void RenderNode(MenuNode node)
            {
                if (node.SeparatorBefore == true)
                    ImGui.Separator();

                if (node.Children.Count > 0)
                {
                    if (ImGui.BeginMenu(node.Name))
                    {
                        if (node.Action != null)
                        {
                            if (ImGui.MenuItem(node.Name, node.Shortcut))
                                Invoke(node.Action);

                            ImGui.Separator();
                        }

                        foreach (var child in node.Children)
                            RenderNode(child);

                        ImGui.EndMenu();
                    }
                }
                else if (node.Action != null)
                {
                    if (ImGui.MenuItem(node.Name, node.Shortcut))
                        Invoke(node.Action);
                }
            }

            void Invoke(MethodInfo method)
            {
                try
                {
                    if (method.GetParameters().Length > 0)
                    {
                        Debug.Log($"Menu method '{method.Name}' requires parameters; skipping invocation.");
                        return;
                    }

                    object? target = null;
                    if (!method.IsStatic)
                    {
                        var decl = method.DeclaringType;
                        if (decl != null)
                        {
                            try
                            {
                                target = Activator.CreateInstance(decl);
                            }
                            catch (Exception ex)
                            {
                                Debug.Log($"Failed to create instance of '{decl.FullName}' to invoke menu method '{method.Name}': {ex.Message}");
                                return;
                            }
                        }
                    }

                    method.Invoke(target, null);
                }
                catch (Exception ex)
                {
                    Debug.Log($"Exception invoking menu method '{method.Name}': {ex.Message}");
                }
            }

        }
     }
}
