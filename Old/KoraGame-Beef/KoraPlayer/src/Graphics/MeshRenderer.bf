using System.Collections;

namespace KoraPlayer.Graphics;

public class MeshRenderer : Renderer
{
	// Private
	private Mesh mesh;
	private List<Material> materials = new .() ~delete _;

	// Properties
	public Mesh Mesh
	{
		get => mesh;
		set
		{
			mesh = value;
		}
	}

	public Material Material
	{
		get => materials.Count > 0 ? materials[0] : null;
	}

	public uint MaterialCount => (uint)materials.Count;

	// Constructor
	public this()
	{
		// Always have 1 material slot
		materials.Add(null);
	}

	// Methods
	public void SetMaterial(Material material, uint slot = 0)
	{
		// Get the bounds from the mesh, or allow 1 slot otherwise
		uint upperLimit = mesh != null
			? mesh.SubmeshCount
			: 1;

		// Check bounds
		if(slot >= upperLimit)
			return;

		// Ensure enough capacity
		while(materials.Count < (int)upperLimit)
			materials.Add(null);

		// Set the material
		materials[(int)slot] = material;
	}

	public Material GetMaterial(uint slot = 0)
	{
		// Check bounds
		if((int)slot < materials.Count)
			return materials[(int)slot];

		// No material assigned
		return null;
	}

	public override void Draw(GraphicsBatch graphics)
	{
		// Check for mesh
		if(mesh == null || mesh.HasVertices == false)
			return;

		// Get the model matrix
		matrix4 modelMatrix = GameObject.LocalToWorldMatrix;

		// Draw all submeshes
		for(uint32 submesh = 0; submesh < mesh.SubmeshCount; submesh++)
		{
			// Get the submesh material
			Material material = GetMaterial(submesh);

			// Check for none
			if(material == null)
				continue;

			// Draw mesh
			graphics.Draw(modelMatrix, material, mesh, submesh, 1);
		}
	}
}