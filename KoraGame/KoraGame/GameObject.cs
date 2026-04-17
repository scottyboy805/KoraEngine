using KoraGame.Graphics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace KoraGame
{
    [DataContract]
    [EditorIcon("Icon/Object.png")]
    public sealed class GameObject : GameElement
    {
        // Private
        [DataMember(Name = "Active")]
        private bool active = false;

        [EditorHidden]
        //[DataMember(Name = "Children")]
        private List<GameObject> children = null;

        [EditorHidden]
        [DataMember(Name = "Components")]        
        private List<Component> components = null;

        private Scene scene = null;
        private GameObject parent = null;
        private Transform localTransform = Transform.Identity;
        private Transform? worldTransform = null;

        // Properties
        public bool Active => active;
        public bool ActiveInScene
        {
            get
            {
                if (active == false)
                    return false;

                // Check for parent
                if(parent != null)
                    return parent.ActiveInScene;

                // Check for scene
                if (scene == null || scene.Active == false)
                    return false;

                // Must be active
                return true;
            }
        }

        public Scene Scene
        {
            get => scene;
            set
            {
                // Get active state
                bool wasActive = ActiveInScene;

                // Remove from current
                if (this.scene != null)
                    this.scene.gameObjects.Remove(this);

                // Change scene
                this.scene = value;

                // Add to new scene
                if (value != null)
                    value.gameObjects.Add(this);
            }
        }
        public GameObject Parent => parent;
        public IReadOnlyList<GameObject> Children => children != null ? children : Array.Empty<GameObject>();
        public bool HasChildren => children != null;
        public bool HasComponents => components != null;

        #region Transform
        public Transform LocalTransform         
        {
            get => localTransform;
            set
            {
                localTransform = value;
                InvalidateTransform();
            }
        }

        public Transform WorldTransform
        {
            get
            {
                if (worldTransform == null)
                {
                    if (parent != null)
                    {
                        worldTransform = new Transform(
                            parent.WorldTransform.TransformMatrix,
                            localTransform.Position,
                            localTransform.Rotation,
                            localTransform.Scale);
                    }
                    else
                        worldTransform = localTransform;
                }
                return worldTransform.Value;
            }
            set
            {
                Matrix4F worldToLocal = parent != null
                    ? parent.WorldTransform.InverseTransformMatrix
                    : Matrix4F.Identity;

                Vector4F p = worldToLocal * new Vector4F(value.Position.X, value.Position.Y, value.Position.Z, 1f);

                QuaternionF parentRot = parent != null
                    ? parent.WorldTransform.Rotation
                    : QuaternionF.Identity;

                QuaternionF localRot = QuaternionF.Inverse(parentRot) * value.Rotation;

                // NOTE: scale conversion assumes no shear / non-uniform complexity case
                Vector3F localScale = value.Scale;

                localTransform = new Transform(
                    new Vector3F(p.X, p.Y, p.Z),
                    localRot,
                    localScale
                );

                InvalidateTransform();
            }
        }

        [DataMember]
        [EditorName("Position")]
        [EditorTooltip("Position in local space")]
        public Vector3F LocalPosition
        {
            get => localTransform.Position;
            set
            {
                localTransform = new Transform(value, localTransform.Rotation, localTransform.Scale);
                InvalidateTransform();
            }
        }

        public QuaternionF LocalRotation
        {
            get => localTransform.Rotation;
            set
            {
                localTransform = new Transform(localTransform.Position, value, localTransform.Scale);
                InvalidateTransform();
            }
        }

        [DataMember]
        [EditorName("Rotation")]
        [EditorTooltip("Rotation in local space")]
        public Vector3F LocalEulerRotation
        {
            get => localTransform.EulerRotation;
            set
            {
                localTransform = new Transform(localTransform.Position, QuaternionF.Euler(value), localTransform.Scale);
                InvalidateTransform();
            }
        }

        [DataMember]
        [EditorName("Scale")]
        public Vector3F LocalScale
        {
            get => localTransform.Scale;
            set
            {
                localTransform = new Transform(localTransform.Position, localTransform.Rotation, value);
                InvalidateTransform();
            }
        }

        public Vector3F WorldPosition
        {
            get => WorldTransform.Position;
            set
            {
                Vector4F local = WorldToLocalMatrix * (Vector4F)value;

                localTransform = new Transform(
                    local.XYZ,
                    localTransform.Rotation,
                    localTransform.Scale
                );

                InvalidateTransform();
            }
        }

        public QuaternionF WorldRotation
        {
            get => WorldTransform.Rotation;
            set
            {
                QuaternionF parentRot = parent != null
                    ? parent.WorldTransform.Rotation
                    : QuaternionF.Identity;

                QuaternionF local = QuaternionF.Inverse(parentRot) * value;

                localTransform = new Transform(
                    localTransform.Position,
                    local,
                    localTransform.Scale
                );
                InvalidateTransform();
            }
        }

        public Vector3F WorldEulerRotation
        {
            get => WorldTransform.EulerRotation;
            set
            {
                QuaternionF worldRot = QuaternionF.Euler(value);

                QuaternionF parentRot = parent != null
                    ? parent.WorldTransform.Rotation
                    : QuaternionF.Identity;

                QuaternionF localRot = QuaternionF.Inverse(parentRot) * worldRot;

                localTransform = new Transform(
                    localTransform.Position,
                    localRot,
                    localTransform.Scale
                );

                InvalidateTransform();
            }
        }

        public Vector3F Forward => WorldTransform.Forward;
        public Vector3F Up => WorldTransform.Up;
        public Vector3F Right => WorldTransform.Right;

        public Matrix4F LocalToWorldMatrix => WorldTransform.TransformMatrix;
        public Matrix4F WorldToLocalMatrix => WorldTransform.InverseTransformMatrix;
        #endregion

        // Constructor
        private GameObject() { }
        public GameObject(string name, bool addToActiveScene = true)
            : base(name)
        {
            // Add to scene
            if(addToActiveScene == true && Game?.Scene != null)
                this.Scene = Game.Scene;
        }

        // Methods
        #region AddComponent
        public T AddComponent<T>() where T : Component, new()
        {
            T instance = Game?.Scriptable.CreateInstance<T>();
            AddComponent(instance);
            return instance;
        }

        public Component AddComponent(Type type)
        {
            // Check type
            if (typeof(Component).IsAssignableFrom(type) == false)
                throw new ArgumentException(nameof(type) + " must be a component");

            // Create instance
            Component instance = Game?.Scriptable.CreateInstanceAs<Component>(type);
            AddComponent(instance);
            return instance;
        }

        public void AddComponent(Component component)
        {
            // Check for null
            if (component == null)
                throw new ArgumentNullException(nameof(Component));

            // Create list
            if (components == null)
                components = new();

            // Add to list
            components.Add(component);
            component.gameObject = this;

            // Set active
            if(ActiveInScene == true)
                component.SetActive(true);
        }
        #endregion

        #region GetComponent
        public Component GetComponent(Type type, bool includeInactive = false)
        {
            // Get components
            if (components != null)
            {
                foreach (Component component in components)
                {
                    if (type.IsAssignableFrom(component.elementType) == true && CheckComponent(component, includeInactive) == true)
                        return component;
                }
            }
            return null;
        }

        public T GetComponent<T>(bool includeInactive = false) where T : class
        {
            // Get components
            if (components != null)
            {
                foreach (Component component in components)
                {
                    if (component is T match && CheckComponent(component, includeInactive) == true)
                        return match;
                }
            }
            return null;
        }

        public T[] GetComponents<T>(bool includeInactive = false) where T : class
        {
            // Check for none
            if (components == null || components.Count == 0)
                return Array.Empty<T>();

            // Get components
            return components
                .Where(c => CheckComponent(c, includeInactive) == true)
                .OfType<T>()
                .ToArray();
        }

        public int GetComponents<T>(IList<T> results, bool includeInactive = false) where T : class
        {
            // Track count
            int count = 0;

            // Get components
            if (components != null)
            {
                foreach (Component component in components)
                {
                    if (component is T match && CheckComponent(component, includeInactive) == true)
                    {
                        results.Add(match);
                        count++;
                    }
                }
            }
            // Get count
            return count;
        }

        public T GetComponentInChildren<T>(bool includeInactive = false, string tag = null) where T : class
        {
            // Search for component
            return BFSSearchComponentChildren<T>(this, includeInactive, tag);
        }

        public T[] GetComponentsInChildren<T>(bool includeInactive = false, string tag = null) where T : class
        {
            // Search for components
            return BFSSearchComponentsChildren<T>(this, includeInactive, tag)
                .ToArray();
        }

        public int GetComponentsInChildren<T>(IList<T> results, bool includeInactive = false, string tag = null) where T : class
        {
            // Search for components
            return BFSSearchComponentsChildren<T>(this, results, includeInactive, tag);
        }

        public T GetComponentInParent<T>(bool includeInactive = false, string tag = null) where T : class
        {
            // Search for component
            return BFSSearchComponentParent<T>(this, includeInactive, tag);
        }

        public T[] GetComponentsInParent<T>(bool includeInactive = false, string tag = null) where T : class
        {
            // Search for components
            return BFSSearchComponentsParent<T>(this, includeInactive, tag)
                .ToArray();
        }

        public int GetComponentsInParent<T>(IList<T> results, bool includeInactive = false, string tag = null) where T : class
        {
            // Search for components
            return BFSSearchComponentsParent<T>(this, results, includeInactive, tag);
        }
        #endregion

        #region SearchComponents(T)
        private static T BFSSearchComponentChildren<T>(GameObject current, bool includeInactive, string tag) where T : class
        {
            // Check for any components
            if (current.components != null && current.components.Count > 0)
            {
                // Search all
                foreach (Component component in current.components)
                {
                    if (component is T match && CheckComponent(component, includeInactive, tag) == true)
                        return match;
                }

                // Search deeper
                foreach (Component component in current.components)
                {
                    // Search inside child component
                    T result = BFSSearchComponentChildren<T>(component.GameObject, includeInactive, tag);

                    // Check for match
                    if (result != null)
                        return result;
                }
            }
            // Not found
            return null;
        }

        private static IEnumerable<T> BFSSearchComponentsChildren<T>(GameObject current, bool includeInactive, string tag) where T : class
        {
            // Search all components
            foreach (Component component in current.components)
            {
                // Check for match
                if (component is T match && CheckComponent(component, includeInactive, tag) == true)
                    yield return match;
            }

            // Search deeper
            foreach (Component component in current.components)
            {
                // Search inside child components
                foreach (T result in BFSSearchComponentsChildren<T>(component.GameObject, includeInactive, tag))
                    yield return result;
            }
        }

        private static int BFSSearchComponentsChildren<T>(GameObject current, IList<T> results, bool includeInactive, string tag) where T : class
        {
            int count = 0;

            // Search all components
            foreach (Component component in current.components)
            {
                // Check for match
                if (component is T match && CheckComponent(component, includeInactive, tag) == true)
                {
                    results.Add(match);
                    count++;
                }
            }

            // Search deeper
            foreach (Component component in current.components)
            {
                // Search inside child components
                count += BFSSearchComponentsChildren<T>(component.GameObject, results, includeInactive, tag);
            }
            return count;
        }

        private static T BFSSearchComponentParent<T>(GameObject current, bool includeInactive, string tag) where T : class
        {
            if (current.components != null && current.components.Count > 0)
            {
                // Search all components
                foreach (Component component in current.components)
                {
                    // Check for match
                    if (component is T match && CheckComponent(component, includeInactive, tag) == true)
                        return match;
                }
            }

            // Search parent
            if (current.Parent != null)
            {
                // Search inside parent components
                T result = BFSSearchComponentParent<T>(current.Parent, includeInactive, tag);

                // Check for found
                if (result != null)
                    return result;
            }
            // Not found
            return null;
        }

        private static IEnumerable<T> BFSSearchComponentsParent<T>(GameObject current, bool includeInactive, string tag) where T : class
        {
            // Search all components
            foreach (Component component in current.components)
            {
                // Check for match
                if (component is T match && CheckComponent(component, includeInactive, tag) == true)
                    yield return match;
            }

            // Search parent
            if (current.Parent != null)
            {
                // Search inside parent components
                foreach (T result in BFSSearchComponentsParent<T>(current.Parent, includeInactive, tag))
                    yield return result;
            }
        }

        private static int BFSSearchComponentsParent<T>(GameObject current, IList<T> results, bool includeInactive, string tag) where T : class
        {
            int count = 0;

            // Search all components
            foreach (Component component in current.components)
            {
                // Check for match
                if (component is T match && CheckComponent(component, includeInactive, tag) == true)
                {
                    results.Add(match);
                    count++;
                }
            }

            // Search parent
            if (current.Parent != null)
                count += BFSSearchComponentsParent<T>(current.Parent, results, includeInactive, tag);

            return count;
        }
        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool CheckComponent(Component component, bool includeInactive)
        {
            return (includeInactive == true || component.Active == true);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool CheckComponent(Component component, bool includeInactive, string tag)
        {
            return (includeInactive == true || component.Active == true)
                ;//&& (tag == null || component.CompareTag(tag) == true);
        }


        public void SetActive(bool on)
        {
            // Check for changed
            if (active == on)
                return;

            this.active = on;

            // Update children
            if (children != null)
            {
			    foreach(GameObject child in children)
                    child.SetActive(on);
            }

            // Update components
            if (components != null)
            {
			    foreach(Component component in components)
                    component.SetActive(on);
            }
        }

        private void InvalidateTransform()
        {
            // Clear world transform
            worldTransform = null;

            // Invalidate children also if our transform was changed
            if (children != null)
            {
			    foreach(GameObject go in children)
                    go.InvalidateTransform();
            }
        }

        public static GameObject PrimitiveQuad(GraphicsDevice graphics, Vector2F? extents = null)
        {
            // Create the object
            GameObject go = new GameObject("Quad");

            // Create renderer
            MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();
            meshRenderer.Mesh = Mesh.PrimitiveQuad(graphics,
                extents != null ? extents.Value : Vector2F.One);

            return go;
        }

        public static GameObject PrimitiveCube(GraphicsDevice graphics, Vector3F? extents = null)
        {
            // Create the object
            GameObject go = new GameObject("Cube");

            // Create renderer
            MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();
            meshRenderer.Mesh = Mesh.PrimitiveCube(graphics, 
                extents != null ? extents.Value : Vector3F.One);

            return go;
        }

        public static GameObject PrimitiveSphere(GraphicsDevice graphics, float? radius = null, float? segments = null)
        {
            // Create the object
            GameObject go = new GameObject("Sphere");

            // Create renderer
            MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();
            meshRenderer.Mesh = Mesh.PrimitiveSphere(graphics, 
                radius != null ? radius.Value: 0.5f, 
                segments != null ? segments.Value : 12);

            return go;
        }
    }
}
