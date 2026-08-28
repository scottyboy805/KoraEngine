using System;
using System.Collections;
using internal KoraPlayer;
using KoraPlayer.Graphics;
using KoraPlayer.Scripting;

namespace KoraPlayer;

public sealed class GameObject : GameElement
{
	// Private
	private bool active = true;
	private Scene scene;
	private GameObject parent;
	private List<GameObject> children;
	private List<Component> components;

	private Transform localTransform = Transform.Identity;
	private Transform? worldTransform = null;

	// Properties
	public bool Active => active;
	public bool ActiveInScene
	{
		get
		{
			if(active == false)
				return false;

			// Check for parent
			if(parent != null)
				return parent.ActiveInScene;

			// Check for scene
			if(scene == null || scene.Active == false)
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

			// Update children
			if (children != null)
			{
			    for (GameObject child in children)
			        child.scene = value;
			}

			// Add to new scene
			if (value != null)
			    value.gameObjects.Add(this);

			// Check for active now
			if(wasActive != ActiveInScene)
			    DoGameObjectEnabledEvent(this, ActiveInScene);
		}
	}

	public GameObject Parent
	{
		get => parent;
		set
		{
			if(value == this)
			{
				Debug.LogError("Cannot set parent to self", .Game, this);
				return;
			}

			// Check for active
			bool wasActive = ActiveInScene;

			// Remove from current
			if(parent != null && parent.children != null)
				parent.children.Remove(this);

			scene = null;
			parent = value;

			// Add to new parent
			if(parent != null)
			{
				// Create children on demand
				if(parent.children == null)
					parent.children = new .();

				// Add as child
				parent.children.Add(this);
				scene = parent.scene;
			}

			// Check for active now
			if(ActiveInScene != wasActive)
				DoGameObjectEnabledEvent(this, ActiveInScene);

			// Force transform update
			InvalidateTransform();
		}
	}

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
			if(worldTransform == null)
			{
				worldTransform = parent != null
					? Transform(parent.WorldTransform.TransformMatrix, localTransform.Position, localTransform.Rotation, localTransform.Scale)
					: localTransform;
			}
			return worldTransform.Value;
		}
		set
		{
			matrix4 worldToLocal = parent != null
				? parent.WorldTransform.InverseTransformMatrix
				: matrix4.Identity;

			float4 p = worldToLocal * float4(value.Position, 1f);

			quaternion parentRot = parent != null
				? parent.WorldTransform.Rotation
				: quaternion.Identity;

			quaternion localRot = quaternion.Inverse(parentRot) * value.Rotation;

			// NOTE: scale conversion assumes no shear / non-uniform complexity case
			float3 localScale = value.Scale;

			localTransform = Transform(p.XYZ, localRot, localScale);
			InvalidateTransform();
		}
	}

	public float3 LocalPosition
	{
		get => localTransform.Position;
		set
		{
			localTransform = Transform(value, localTransform.Rotation, localTransform.Scale);
			InvalidateTransform();
		}
	}

	public quaternion LocalRotation
	{
		get => localTransform.Rotation;
		set
		{
			localTransform = Transform(localTransform.Position, value, localTransform.Scale);
			InvalidateTransform();
		}
	}

	public float3 LocalEulerRotation
	{
		get => localTransform.EulerRotation;
		set
		{
			localTransform = Transform(localTransform.Position, quaternion.Euler(value), localTransform.Scale);
			InvalidateTransform();
		}
	}

	public float3 LocalScale
	{
		get => localTransform.Scale;
		set
		{
			localTransform = Transform(localTransform.Position, localTransform.Rotation, value);
			InvalidateTransform();
		}
	}

	public float3 WorldPosition
	{
		get => WorldTransform.Position;
		set
		{
			float4 local = WorldToLocalMatrix * float4(value, 1f);

			localTransform = Transform(local.XYZ, localTransform.Rotation, localTransform.Scale);
			InvalidateTransform();
		}
	}

	public quaternion WorldRotation
	{
		get => WorldTransform.Rotation;
		set
		{
			quaternion parentRot = parent != null
				? parent.WorldTransform.Rotation
				: quaternion.Identity;

			quaternion local = quaternion.Inverse(parentRot) * value;

			localTransform = Transform(localTransform.Position, local, localTransform.Scale);
			InvalidateTransform();
		}
	}

	public float3 WorldEulerRotation
	{
		get => WorldTransform.EulerRotation;
		set
		{
			quaternion worldRot = quaternion.Euler(value);

			quaternion parentRot = parent != null
				? parent.WorldTransform.Rotation
				: quaternion.Identity;

			quaternion localRot = quaternion.Inverse(parentRot) * worldRot;

			localTransform = Transform(localTransform.Position, localRot, localTransform.Scale);
			InvalidateTransform();
		}
	}

	public matrix4 LocalToWorldMatrix => WorldTransform.TransformMatrix;
	public matrix4 WorldToLocalMatrix => WorldTransform.InverseTransformMatrix;
#endregion

	// Constructor
	public this(StringView name)
		: base(name)
	{
	}

	// Methods
#region AddComponent
	public Result<ScriptableBehaviour> AddScriptComponent(StringView assemblyQualifiedTypeName)
	{
		// Create the component
		ScriptableBehaviour script = ScriptableBehaviour.CreateScriptInstance(assemblyQualifiedTypeName);

		// Add the component
		Try!(AddComponent(script));

		// Get the instance
		return script;
	}

	public Result<T> AddComponent<T>() where T : Component, new
	{
		// Create instance
		T instance = new T();

		// Add the component
		Try!(AddComponent(instance));

		// Get the instance
		return instance;
	}

	public Result<Component> AddComponent(Type type)
	{
		// Check type - must be a component
		if(type.IsSubtypeOf(typeof(Component)) == false)
			return .Err;

		// Create instance
		Component instance = (Component)type.CreateObject();

		// Add the component
		Try!(AddComponent(instance));

		// Get the instance
		return instance;
	}

	public Result<Component> AddComponent(Component component)
	{
		// Check for invalid component
		if(component == null || component.IsDestroyed == true)
			return .Err;

		// Init components
		if(this.components == null)
			this.components = new .();

		// Add to list
		this.components.Add(component);
		component.gameObject = this;

		// Set active
		if(ActiveInScene == true && component.Active == true)
			Component.DoComponentEnabledEvent(component, true);

		// Get the component
		return component;
	}
#endregion

#region GetComponent
	public Component GetComponent(Type type, bool includeInactive = false)
	{
		// Check for any
		if(components != null)
		{
			for(Component component in components)
			{
				if(CheckComponent(component, includeInactive) == true && component.GetType().IsSubtypeOf(type))
					return component;
			}
		}
		return null;
	}

	public T GetComponent<T>(bool includeInactive = false) where T : class
	{
		// Check for any
		if(components != null)
		{
			for(Component component in components)
			{
				if(CheckComponent(component, includeInactive) == true && component is T)
					return component as T;
			}
		}
		return null;
	}

	public uint32 GetComponents(Type type, List<Component> outComponents, bool includeInactive = false)
	{
		// Check for any
		uint32 count = 0;
		if(components != null)
		{
			for(Component component in components)
			{
				if(CheckComponent(component, includeInactive) == true && component.GetType().IsSubtypeOf(type))
				{
					outComponents.Add(component);
					count++;
				}
			}
		}
		return count;
	}

	public uint32 GetComponents<T>(List<T> outComponents, bool includeInactive = false) where T : class
	{
		// Check for any
		uint32 count = 0;
		if(components != null)
		{
			for(Component component in components)
			{
				if(CheckComponent(component, includeInactive) == true && component is T)
				{
					outComponents.Add(component as T);
					count++;
				}
			}
		}
		return count;
	}

	public Component GetComponentInChildren(Type type, bool includeInactive = false)
	{
		Component result;
		BFSSearchComponentsInChildren<Component>(this, type, includeInactive, 1, null, out result);

		return result;
	}

	public T GetComponentInChildren<T>(bool includeInactive = false) where T : Component
	{
		Component result;
		BFSSearchComponentsInChildren<Component>(this, typeof(T), includeInactive, 1, null, out result);

		return (T)result;
	}

	public uint32 GetComponentsInChildren(Type type, List<Component> components, bool includeInactive = false)
	{
		Component result;
		return BFSSearchComponentsInChildren(this, type, includeInactive, uint32.MaxValue, components, out result);
	}

	public uint32 GetComponentsInChildren<T>(List<T> components, bool includeInactive = false) where T : Component
	{
		Component result;
		return BFSSearchComponentsInChildren(this, typeof(T), includeInactive, uint32.MaxValue, components, out result);
	}

	public Component GetComponentInParent(Type type, bool includeInactive = false)
	{
		Component result;
		BFSSearchComponentsInParent<Component>(this, type, includeInactive, 1, null, out result);

		return result;
	}

	public T GetComponentInParent<T>(bool includeInactive = false) where T : Component
	{
		Component result;
		BFSSearchComponentsInParent<Component>(this, typeof(T), includeInactive, 1, null, out result);

		return (T)result;
	}

	public uint32 GetComponentsInParent(Type type, List<Component> components, bool includeInactive = false)
	{
		Component result;
		return BFSSearchComponentsInParent(this, type, includeInactive, uint32.MaxValue, components, out result);
	}

	public uint32 GetComponentsInParent<T>(List<T> components, bool includeInactive = false) where T : Component
	{
		Component result;
		return BFSSearchComponentsInParent(this, typeof(T), includeInactive, uint32.MaxValue, components, out result);
	}

	private static uint32 BFSSearchComponentsInChildren<T>(GameObject current, Type type, bool includeInactive, uint32 maxComponents, List<T> components, out Component lastComponent) where T : Component
	{
		uint32 matchedComponents = 0;
		lastComponent = null;

		// Check for any components
		if (current.components != null && current.components.Count > 0)
		{
		    // Search all
		    for (let component in current.components)
		    {
		        if (component.GetType().IsSubtypeOf(type) == true && CheckComponent(component, includeInactive) == true)
				{
					if(components != null) components.Add(component);
					lastComponent = component;
					matchedComponents++;

					// Check for too many
					if(matchedComponents >= maxComponents)
						break;
				}
		    }

		    // Search deeper
		    for (let component in current.components)
		    {
		        // Search inside child component
				Component lastChildComponent = null;
		        matchedComponents += BFSSearchComponentsInChildren(component.GameObject, type, includeInactive, (uint32)(maxComponents - matchedComponents), components, out lastChildComponent);

				// Update last component
				if(lastChildComponent != null)
					lastComponent = lastChildComponent;
			}
		}
		// Get number of found components
		return matchedComponents;
	}

	private static uint32 BFSSearchComponentsInParent<T>(GameObject current, Type type, bool includeInactive, uint32 maxComponents, List<T> components, out Component lastComponent) where T : Component
	{
		uint32 matchedComponents = 0;
		lastComponent = null;

		// Check for any components
		if(current.components != null && current.components.Count > 0)
		{
			// Search all
			for(let component in current.components)
			{
				if (component.GetType().IsSubtypeOf(type) == true && CheckComponent(component, includeInactive) == true)
				{
					if(components != null) components.Add(component);
					lastComponent = component;
					matchedComponents++;

					// Check for too many
					if(matchedComponents >= maxComponents)
						break;
				}
			}

			// Search parent
			if(current.parent != null)
			{
				Component lastParentComponent = null;
				matchedComponents += BFSSearchComponentsInParent(current, type, includeInactive, (uint32)(maxComponents - matchedComponents), components, out lastParentComponent);

				// Update last component
				if(lastParentComponent != null)
					lastComponent = lastParentComponent;
			}
		}
		// Get number of found components
		return maxComponents;
	}
#endregion

	public void SetActive(bool on)
	{
		// Check for changed
		if(active == on)
			return;

		// Check was active
		bool wasActive = ActiveInScene;
		active = on;

		// Check for active now
		if(ActiveInScene != wasActive)
			DoGameObjectEnabledEvent(this, on);
	}

	private void InvalidateTransform()
	{
		// Clear world transform
		worldTransform = null;

		// Update children
		if(children != null)
		{
			// Invalidate all children too
			for(let child in children)
				child.InvalidateTransform();
		}
	}

	public static GameObject CreatePrimitiveCube(GraphicsDevice graphics, float3? extents = null)
	{
		// Create the object
		GameObject go = new .("Cube");

		// Create renderer
		MeshRenderer renderer = go.AddComponent<MeshRenderer>();
		renderer.Mesh = Mesh.CreatePrimitiveCube(graphics, extents != null ? extents.Value : float3.One);

		return go;
	}

	[Inline]
	private static bool CheckComponent(Component component, bool includeInactive)
	{
		return includeInactive == true || component.Active == true;
	}

	internal static void DoGameObjectEnabledEvent(GameObject go, bool on)
	{
		// Update components at this level first
		if(go.components != null)
		{
			for(let component in go.components)
				Component.DoComponentEnabledEvent(component, on);
		}

		// Update children
		if(go.children != null)
		{
			for(let child in go.children)
				DoGameObjectEnabledEvent(child, on);
		}
	}
}