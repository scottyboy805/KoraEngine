using Assimp;
using KoraGame.Graphics;
using Material = KoraGame.Graphics.Material;
using Mesh = KoraGame.Graphics.Mesh;

namespace KoraGame.Assets
{
    [AssetImporter(".fbx")]
    internal sealed class ModelReader : IAssetImporter
    {
        // Private
        private Material defaultMaterial = null;

        // Methods
        public async Task<GameElement> ImportAsync(AssetReadContext context, Stream stream, CancellationToken cancellationToken)
        {
            // Load default material
            if(defaultMaterial == null)
                defaultMaterial = await context.LoadDependencyAsync<Material>("DefaultAssets/PbrMaterial.json");

            // Create import context
            AssimpContext importContext = new AssimpContext();

            // Configure
            importContext.SetConfig(new Assimp.Configs.FBXPreservePivotsConfig(true));

            // Post process flags
            PostProcessSteps importFlags = PostProcessSteps.Triangulate
                | PostProcessSteps.MakeLeftHanded
                | PostProcessSteps.FlipWindingOrder
                ;//| PostProcessSteps.GenerateSmoothNormals;

            // Import the asset
            Assimp.Scene scene = importContext.ImportFileFromStream(stream, importFlags, context.AssetExtension);

            // Get result
            GameElement result = null;

            // Check for mesh specified
            if (context.AssetType == typeof(Mesh))
            {
                result = ReadAsMesh(context, scene, scene.RootNode.Children.First());
            }
            // Check for material specified
            else if(context.AssetType == typeof(Material))
            {
                // Load the materials
                IList<Material> material = await ReadAsMaterial(context, scene, scene.RootNode.Children.First(), true);

                // Get the first material
                result = material.First();
            }
            // Import as game object - default
            else
            {
                // Load the object
                result = await ReadAsGameObject(context, scene, scene.RootNode.Children.First());
            }

            // Get result
            return result;
        }

        private static unsafe Mesh ReadAsMesh(AssetReadContext context, Assimp.Scene scene, Node meshNode)
        {
            // Get number of sub meshes
            uint subMeshCount = (uint)meshNode.MeshCount;

            // Create mesh
            Mesh mesh = new Mesh(context.Graphics, subMeshCount);
            mesh.Name = meshNode.Name;

            // Process all child
            uint subMeshIndex = 0;
            foreach(int meshIndex in meshNode.MeshIndices)
            {
                // Get imp mesh
                Assimp.Mesh impMesh = scene.Meshes[meshIndex];
                
                // Apply indices
                mesh.SetIndices(impMesh.GetIndices(), subMeshIndex);

                // Vertices
                IList<Vector3F> vertices = null;
                IList<Vector3F> normals = null;
                IList<Vector2F> uvs = null;
                IList<Color> colors = null;

                // Positions
                vertices = impMesh.Vertices
                    .Select(v => new Vector3F(v.X, v.Y, v.Z))
                    .ToArray();

                // Normals
                if (impMesh.HasNormals == true)
                    normals = impMesh.Normals
                        .Select(n => new Vector3F(n.X, n.Y, n.Z))
                        .ToArray();

                // UV0
                if (impMesh.TextureCoordinateChannelCount > 0)
                    uvs = impMesh.TextureCoordinateChannels[0]
                        .Select(uv => new Vector2F(uv.X, uv.Y))
                        .ToArray();

                // Color
                if (impMesh.VertexColorChannelCount > 0)
                {
                    colors = impMesh.VertexColorChannels[0]
                        .Select(c => new Color(c.R, c.G, c.B, c.A))
                        .ToArray();
                }
                else
                {
                    // Provide fallback colors for testing
                    colors = Enumerable.Repeat(Color.White, vertices.Count)
                        .ToArray();
                }

                // Update mesh
                mesh.SetVertices(vertices, normals, uvs, default, subMeshIndex);

                // Increment sub mesh
                subMeshIndex++;
            }

            // Upload the mesh
            context.GraphicsCmd.UploadMesh(mesh);

            // Get the mesh result
            return mesh;
        }

        private async Task<IList<Material>> ReadAsMaterial(AssetReadContext context, Assimp.Scene scene, Node meshNode, bool firstOnly = false)
        {
            List<Material> materials = new();

            // Process all meshes
            foreach (int meshIndex in meshNode.MeshIndices)
            {
                // Get imp mesh
                Assimp.Mesh impMesh = scene.Meshes[meshIndex];

                // Get the material
                Assimp.Material impMaterial = scene.Materials[impMesh.MaterialIndex];
                
                // Copy the default material
                Material material = GameElement.Instantiate(defaultMaterial);
                material.Name = impMaterial.Name;

                // Check for texture
                if(impMaterial.HasTextureDiffuse == true)
                {
                    // Try to load texture
                    Texture texture = await context.LoadDependencyAsync<Texture>(impMaterial.TextureDiffuse.FilePath);

                    // Check for found
                    if (texture != null)
                        material.MainTexture = texture;
                }

                // Get the material
                materials.Add(material);

                // Check for first
                if (firstOnly == true)
                    break;
            }
            return materials;
        }

        private async Task<GameObject> ReadAsGameObject(AssetReadContext context, Assimp.Scene scene, Node objectNode)
        {
            // Create game object
            GameObject go = new GameObject(objectNode.Name);

            // Check for mesh
            if(objectNode.HasMeshes == true)
            {
                // Add mesh renderer
                MeshRenderer meshRenderer = new();

                // Read the mesh
                meshRenderer.Mesh = ReadAsMesh(context, scene, objectNode);

                // Read the materials
                uint materialSlot = 0;
                foreach(Material mat in await ReadAsMaterial(context, scene, objectNode))
                {
                    meshRenderer.SetMaterial(mat, materialSlot);
                    materialSlot++;
                }
                
                // Add component
                go.AddComponent(meshRenderer);
            }

            // Check for children
            if(objectNode.HasChildren == true)
            {
                // Read all children
                for(int i = 0; i < objectNode.ChildCount; i++)
                {
                    // Get the child
                    Node childNode = objectNode.Children[i];

                    // Read the child object
                    GameObject childObject = await ReadAsGameObject(context, scene, childNode);

                    // Add child
#warning Fix this
                    //childObject.Parent = go;
                }
            }

            return go;
        }
    }
}
