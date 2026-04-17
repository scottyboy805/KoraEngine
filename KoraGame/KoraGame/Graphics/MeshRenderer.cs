using System.Runtime.Serialization;

namespace KoraGame.Graphics
{
    [EditorIcon("Icon/Mesh.png")]
    public class MeshRenderer : Renderer
    {
        // Private
        [DataMember(Name = "Mesh")]
        private Mesh mesh;
        [DataMember(Name = "Materials")]
        private List<Material> materials = new List<Material>{ null };  // Create with 1 slot by default

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

        // Methods
        public void SetMaterial(Material material, uint slot = 0)
        {
            // Get the bounds from the mesh, or allow 1 slot otherwise
            uint upperLimit = mesh != null
                ? mesh.SubMeshCount
                : 1;

            // Check bounds
            if (slot >= upperLimit)
                return;

            // Ensure enough capacity
            while (materials.Count < (int)upperLimit)
                materials.Add(null);

            // Set the material
            materials[(int)slot] = material;
        }

        public Material GetMaterial(uint slot = 0)
        {
            // Check bounds
            if ((int)slot < materials.Count)
                return materials[(int)slot];

            // No material assigned
            return null;
        }

        public override void Draw(GraphicsBatch graphics)
        {
            // Check for any mesh
            if (mesh == null || mesh.HasVertices == false)
                return;

            // Get the model matrix
            Matrix4F modelMatrix = GameObject.LocalToWorldMatrix;

            // Draw all sub meshes
            for(uint subMesh = 0; subMesh < mesh.SubMeshCount; subMesh++)
            {
                // Get the material
                Material material = GetMaterial(subMesh);

                // Check for none
                if (material == null)
                    continue;

                // Draw mesh
                graphics.Draw(modelMatrix, material, mesh, subMesh, 1);
            }
        }

        internal override void CloneInstantiate(GameElement element)
        {
            base.CloneInstantiate(element);

            MeshRenderer renderer = (MeshRenderer) element;

            renderer.mesh = Mesh.Instantiate(mesh);
            renderer.materials = materials != null ? materials.Select(m => Material.Instantiate(m)).ToList() : null;
        }
    }
}
