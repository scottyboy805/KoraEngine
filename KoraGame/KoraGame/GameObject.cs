using KoraGame.Graphics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace KoraGame
{
    [DataContract]
    public sealed class GameObject : GameElement
    {
        // Private
        [DataMember(Name = "Active")]
        private bool active = false;
        //[DataMember(Name = "Children")]
        private List<GameObject> children = null;
        [DataMember(Name = "Components")]
        private List<Component> components = null;

        private Scene scene = null;
        private GameObject parent = null;

        private Vector3F localPosition = Vector3F.Zero;
        private QuaternionF localRotation = QuaternionF.Identity;
        private Vector3F localScale = Vector3F.One;
        private Matrix4F? localToWorldMatrix = null;
        private Matrix4F? worldToLocalMatrix = null;

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

        [DataMember]
        public Vector3F LocalPosition
        {
            get => localPosition;
            set
            {
                localPosition = value;
                InvalidateTransform();
            }
        }

        public QuaternionF LocalRotation
        {
            get => localRotation;
            set
            {
                localRotation = value;
                InvalidateTransform();
            }
        }

        [DataMember]
        public Vector3F LocalEuler
        {
            set { }
        }

        [DataMember]
        public Vector3F LocalScale
        {
            get => localScale;
            set
            {
                localScale = value;
                InvalidateTransform();
            }
        }

        public Vector3F WorldPosition
        {
            get
            {
                return parent != null
                    ? parent.TransformPoint(localPosition)
                    : localPosition;
            }
            set
            {
                localPosition = parent != null
                    ? parent.InverseTransformPoint(value)
                    : value;
                InvalidateTransform();
            }
        }

        public QuaternionF WorldRotation
        {
            get
            {
                QuaternionF rot = localRotation;
                GameObject current = parent;

                while(current != null)
                {
                    rot = current.localRotation * rot;
                    current = current.parent;
                }
                return rot;
            }
            set
            {
                localRotation = parent != null
                    ? QuaternionF.Inverse(parent.WorldRotation) * value 
                    : value;
                InvalidateTransform();
            }
        }

        public Matrix4F LocalToWorldMatrix
        {
            get
            {
                // Check for rebuild
                if (localToWorldMatrix == null)
                {
                    // Create our matrix
                    Matrix4F mat = Matrix4F.TRS(localPosition, localRotation, localScale);

                    // Check for parent
                    if (parent != null)
                        mat = parent.LocalToWorldMatrix * mat;

                    // Update the matrix
                    localToWorldMatrix = mat;
                }
                return localToWorldMatrix.Value;
            }
        }
        public Matrix4F WorldToLocalMatrix
        {
            get
            {
                // Check for rebuild
                if (worldToLocalMatrix == null)
                {
                    // Update the matrix
                    worldToLocalMatrix = Matrix4F.Inverse(LocalToWorldMatrix);
                }
                return worldToLocalMatrix.Value;
            }
        }

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

        #region Transform
        public Vector3F TransformPoint(in Vector3F point)
        {
            Vector4F v = (Vector4F)point;

            // Multiply by matrix
            v = LocalToWorldMatrix * v;

            return v.XYZ;
        }

        public Vector3F InverseTransformPoint(in Vector3F point)
        {
            Vector4F v = (Vector4F)point;

            // Multiply by matrix
            v = WorldToLocalMatrix * v;

            return v.XYZ;
        }
        #endregion

        private void InvalidateTransform()
        {
            // Clear matrix
            localToWorldMatrix = null;

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
