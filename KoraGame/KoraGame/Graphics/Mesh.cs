
namespace KoraGame.Graphics
{
    [Flags]
    public enum MeshVertexElements : uint
    {
        None = 0,
        Position = 1 << 0,
        Normal = 1 << 1,
        UV = 1 << 2,
        Color = 1 << 3,
    }

    public sealed class Mesh : GameElement
    {
        // Type
        private struct SubMesh
        {
            // Public
            public IndexBufferFormat IndexFormat;
            public uint IndexOffset;
            public uint IndexCount;
            public MeshVertexElements VertexElements;
            public uint VertexOffset;
            public uint VertexCount;

            // Properties
            public bool HasIndices => IndexCount > 0;
            public bool HasVertices => VertexCount > 0 && VertexElements != 0;
        }

        // Private
        private GraphicsDevice device;
        private GraphicsBuffer indexBuffer;
        private GraphicsBuffer vertexBuffer;
        private SubMesh[] subMeshes;

        // Properties
        public bool HasIndices => indexBuffer != null;
        public bool HasVertices => vertexBuffer != null;
        public uint SubMeshCount => (uint)subMeshes.Length;

        public GraphicsBuffer IndexBuffer => indexBuffer;
        public GraphicsBuffer VertexBuffer => vertexBuffer;

        // Constructor
        private Mesh()
        {
            device = Game.Instance?.Graphics;
        }

        public Mesh(GraphicsDevice device, uint subMeshCount = 1)
        {
            // Check for null
            if (device == null)
                throw new ArgumentNullException(nameof(device));

            // Require 1 sub mesh minimum
            if (subMeshCount < 1)
                subMeshCount = 1;

            this.device = device;

            // Create sub meshes
            this.subMeshes = new SubMesh[subMeshCount];
        }

        // Methods
        protected override void OnDestroy()
        {
            if(device != null)
            {
                device = null;

                // GC will run the finalizer to cleanup these
                indexBuffer = null;
                vertexBuffer = null;
                subMeshes = null;
            }
        }

        internal override void CloneInstantiate(GameElement element)
        {
            base.CloneInstantiate(element);

            // Get mesh
            Mesh clone = (Mesh)element;

            // Copy buffers and sub meshes
            clone.indexBuffer = indexBuffer?.Copy();
            clone.vertexBuffer = vertexBuffer?.Copy();
            clone.subMeshes = subMeshes.ToArray();
        }

        public void GetElements(out uint indexOffset, out uint vertexOffset, out uint elementCount, uint subMesh = 0)
        {
            // Check bounds
            if (subMesh >= (uint)subMeshes.Length)
            {
                indexOffset = 0;
                vertexOffset = 0;
                elementCount = 0;
                return;
            }

            // Get the sub mesh
            SubMesh mesh = subMeshes[subMesh];

            // Get offsets
            indexOffset = mesh.IndexOffset;
            vertexOffset = mesh.VertexOffset;

            // Get count
            elementCount = mesh.HasIndices == true
                ? subMeshes[subMesh].IndexCount
                : subMeshes[subMesh].VertexCount;
        }

        public IndexBufferFormat GetIndexFormat(uint subMesh = 0)
        {
            // Check bounds
            if (subMesh >= (uint)subMeshes.Length)
                return 0;

            // Get the format
            return subMeshes[subMesh].IndexFormat;
        }

        public MeshVertexElements GetVertexElements(uint subMesh = 0)
        {
            // Check bounds
            if (subMesh >= (uint)subMeshes.Length)
                return 0;

            // Get the elements
            return subMeshes[subMesh].VertexElements;
        }

        #region SetData
        public unsafe void SetIndices(IList<int> indices, uint subMesh = 0)
        {
            // Check bounds
            if (subMesh >= (uint)subMeshes.Length)
                return;

            // Check for no indices - clear this sub mesh
            if (indices == null || indices.Count == 0)
            {
                subMeshes[subMesh].IndexFormat = IndexBufferFormat.Int16;
                subMeshes[subMesh].IndexOffset = 0;
                subMeshes[subMesh].IndexCount = 0;
                return;
            }

            // Determine index format for this sub mesh
            IndexBufferFormat indexFormat = indices.Count >= ushort.MaxValue
                ? IndexBufferFormat.Int32
                : IndexBufferFormat.Int16;

            // Calculate sizes
            uint indexSize = GetIndexSize(indexFormat);
            uint newDataSize = (uint)indices.Count * indexSize;

            // Calculate total buffer size needed and track offsets
            uint totalSize = 0;
            uint thisSubMeshOffset = 0;

            for (uint i = 0; i < subMeshes.Length; i++)
            {
                if (i == subMesh)
                {
                    thisSubMeshOffset = totalSize;
                    totalSize += newDataSize;
                }
                else if (subMeshes[i].HasIndices)
                {
                    uint existingIndexSize = GetIndexSize(subMeshes[i].IndexFormat);
                    totalSize += subMeshes[i].IndexCount * existingIndexSize;
                }
            }

            // Create new buffer
            GraphicsBuffer newBuffer = new GraphicsBuffer(device, GraphicsBufferUsage.Index, totalSize);

            // Copy existing data and write new data
            newBuffer.MapMemory(newBufferPtr =>
            {
                byte* newPtr = (byte*)newBufferPtr;
                uint writeOffset = 0;

                // Copy existing sub mesh data and write new data
                if (indexBuffer != null)
                {
                    indexBuffer.MapMemory(oldBufferPtr =>
                    {
                        byte* oldPtr = (byte*)oldBufferPtr;
                        uint readOffset = 0;

                        for (uint i = 0; i < subMeshes.Length; i++)
                        {
                            if (i == subMesh)
                            {
                                // Write new data for this sub mesh
                                WriteSubMeshIndexData(newPtr + writeOffset, indexFormat, indices);
                                writeOffset += newDataSize;
                            }
                            else if (subMeshes[i].HasIndices)
                            {
                                // Copy existing data as-is
                                uint existingIndexSize = GetIndexSize(subMeshes[i].IndexFormat);
                                uint existingDataSize = subMeshes[i].IndexCount * existingIndexSize;

                                // Direct block copy - no re-layout
                                Buffer.MemoryCopy(oldPtr + readOffset, newPtr + writeOffset,
                                                existingDataSize, existingDataSize);

                                readOffset += existingDataSize;
                                writeOffset += existingDataSize;
                            }
                        }
                    });
                }
                else
                {
                    // No existing buffer, just write new data
                    WriteSubMeshIndexData(newPtr + thisSubMeshOffset, indexFormat, indices);
                }
            });

            // Update sub mesh info
            subMeshes[subMesh].IndexFormat = indexFormat;
            subMeshes[subMesh].IndexCount = (uint)indices.Count;

            // Calculate and update IndexOffset for all sub meshes
            uint currentOffset = 0;
            for (uint i = 0; i < subMeshes.Length; i++)
            {
                if (subMeshes[i].HasIndices)
                {
                    subMeshes[i].IndexOffset = currentOffset;
                    currentOffset += subMeshes[i].IndexCount;
                }
                else
                {
                    subMeshes[i].IndexOffset = 0;
                }
            }

            // Replace buffer
            indexBuffer = newBuffer;

            // Write sub mesh into the buffer mapped ptr
            static void WriteSubMeshIndexData(byte* ptr, IndexBufferFormat format, IList<int> indices)
            {
                for (int i = 0; i < indices.Count; i++)
                {
                    switch (format)
                    {
                        case IndexBufferFormat.Int16:
                            *(ushort*)ptr = (ushort)indices[i];
                            ptr += sizeof(ushort);
                            break;
                        case IndexBufferFormat.Int32:
                            *(uint*)ptr = (uint)indices[i];
                            ptr += sizeof(uint);
                            break;
                    }
                }
            }
        }

        public unsafe void SetVertices(IList<Vector3F> positions, IList<Vector3F> normals = null, IList<Vector2F> uvs = null, IList<Color> colors = null, uint subMesh = 0)
        {
            // Check bounds
            if (subMesh >= (uint)subMeshes.Length)
                return;

            // Check for no positions
            if (positions == null || positions.Count == 0)
            {
                // Clear this sub mesh data
                subMeshes[subMesh].VertexElements = MeshVertexElements.None;
                subMeshes[subMesh].VertexOffset = 0;
                subMeshes[subMesh].VertexCount = 0;
                return;
            }

            // Get vertex elements for this sub mesh only
            MeshVertexElements elements = MeshVertexElements.Position;
            if (normals != null && normals.Count > 0) elements |= MeshVertexElements.Normal;
            if (uvs != null && uvs.Count > 0) elements |= MeshVertexElements.UV;
            if (colors != null && colors.Count > 0) elements |= MeshVertexElements.Color;

            // Validate input data
            if ((elements & MeshVertexElements.Normal) != 0 && normals.Count != positions.Count)
                throw new ArgumentException("Normals must be same length as positions");
            if ((elements & MeshVertexElements.UV) != 0 && uvs.Count != positions.Count)
                throw new ArgumentException("UVs must be same length as positions");
            if ((elements & MeshVertexElements.Color) != 0 && colors.Count != positions.Count)
                throw new ArgumentException("Colors must be same length as positions");

            // Calculate sizes
            uint vertexSize = GetVertexSize(elements);
            uint newDataSize = (uint)positions.Count * vertexSize;

            // Calculate total buffer size needed
            uint totalSize = 0;
            uint thisSubMeshOffset = 0;

            for (uint i = 0; i < subMeshes.Length; i++)
            {
                if (i == subMesh)
                {
                    thisSubMeshOffset = totalSize;
                    totalSize += newDataSize;
                }
                else if (subMeshes[i].HasVertices)
                {
                    uint existingVertexSize = GetVertexSize(subMeshes[i].VertexElements);
                    totalSize += subMeshes[i].VertexCount * existingVertexSize;
                }
            }

            // Create new buffer
            GraphicsBuffer newBuffer = new GraphicsBuffer(device, GraphicsBufferUsage.Vertex, totalSize);

            // Copy existing data and write new data
            newBuffer.MapMemory(newBufferPtr =>
            {
                byte* newPtr = (byte*)newBufferPtr;
                uint writeOffset = 0;

                // Copy existing sub mesh data and write new data
                if (vertexBuffer != null)
                {
                    vertexBuffer.MapMemory(oldBufferPtr =>
                    {
                        byte* oldPtr = (byte*)oldBufferPtr;
                        uint readOffset = 0;

                        for (uint i = 0; i < subMeshes.Length; i++)
                        {
                            if (i == subMesh)
                            {
                                // Write new data for this sub mesh
                                WriteSubMeshVertexData(newPtr + writeOffset, vertexSize, elements,
                                                     positions, normals, uvs, colors);
                                writeOffset += newDataSize;
                            }
                            else if (subMeshes[i].HasVertices)
                            {
                                // Copy existing data as-is
                                uint existingVertexSize = GetVertexSize(subMeshes[i].VertexElements);
                                uint existingDataSize = subMeshes[i].VertexCount * existingVertexSize;

                                // Direct block copy - no re-layout
                                Buffer.MemoryCopy(oldPtr + readOffset, newPtr + writeOffset,
                                                existingDataSize, existingDataSize);

                                readOffset += existingDataSize;
                                writeOffset += existingDataSize;
                            }
                        }
                    });
                }
                else
                {
                    // No existing buffer, just write new data
                    WriteSubMeshVertexData(newPtr + thisSubMeshOffset, vertexSize, elements,
                                         positions, normals, uvs, colors);
                }
            });

            // Update sub mesh info
            subMeshes[subMesh].VertexElements = elements;
            subMeshes[subMesh].VertexCount = (uint)positions.Count;

            // Calculate and update VertexOffset for all sub meshes
            uint currentOffset = 0;
            for (uint i = 0; i < subMeshes.Length; i++)
            {
                if (subMeshes[i].HasVertices)
                {
                    subMeshes[i].VertexOffset = currentOffset;
                    currentOffset += subMeshes[i].VertexCount;
                }
                else
                {
                    subMeshes[i].VertexOffset = 0;
                }
            }

            // Replace buffer
            vertexBuffer = newBuffer;

            // Write method
            static void WriteSubMeshVertexData(byte* ptr, uint vertexSize, MeshVertexElements elements, IList<Vector3F> positions, IList<Vector3F> normals, IList<Vector2F> uvs, IList<Color> colors)
            {
                for (int i = 0; i < positions.Count; i++)
                {
                    byte* vertexPtr = ptr + (vertexSize * i);

                    // Write position
                    if ((elements & MeshVertexElements.Position) != 0)
                    {
                        *(Vector3F*)vertexPtr = positions[i];
                        vertexPtr += sizeof(Vector3F);
                    }

                    // Write normal
                    if ((elements & MeshVertexElements.Normal) != 0)
                    {
                        *(Vector3F*)vertexPtr = normals[i];
                        vertexPtr += sizeof(Vector3F);
                    }

                    // Write uv
                    if ((elements & MeshVertexElements.UV) != 0)
                    {
                        *(Vector2F*)vertexPtr = uvs[i];
                        vertexPtr += sizeof(Vector2F);
                    }

                    // Write color
                    if ((elements & MeshVertexElements.Color) != 0)
                    {
                        *(Color*)vertexPtr = colors[i];
                        vertexPtr += sizeof(Color);
                    }
                }
            }
        }
        #endregion

        public static uint GetIndexSize(IndexBufferFormat format)
        {
            return format switch
            {
                IndexBufferFormat.Int32 => sizeof(uint),
                _ => sizeof(ushort),
            };
        }

        public static unsafe uint GetVertexSize(MeshVertexElements elements)
        {
            uint size = 0;

            // Calculate required size for one vertex element
            if ((elements & MeshVertexElements.Position) != 0) size += (uint)sizeof(Vector3F);
            if ((elements & MeshVertexElements.Normal) != 0) size += (uint)sizeof(Vector3F);
            if ((elements & MeshVertexElements.UV) != 0) size += (uint)sizeof(Vector2F);
            if ((elements & MeshVertexElements.Color) != 0) size += (uint)sizeof(Color);

            return size;
        }

        public static uint GetVertexElementCount(MeshVertexElements elements)
        {
            uint count = 0;

            // Calculate required size for one vertex element
            if ((elements & MeshVertexElements.Position) != 0) count++;
            if ((elements & MeshVertexElements.Normal) != 0) count++;
            if ((elements & MeshVertexElements.UV) != 0) count++;
            if ((elements & MeshVertexElements.Color) != 0) count++;

            return count;
        }

        #region Primitives
        public static Mesh PrimitiveQuad(GraphicsDevice device, Vector2F bounds)
        {
            // Check for null device
            if (device == null)
                throw new ArgumentNullException(nameof(device));

            // Create the mesh
            Mesh mesh = new Mesh(device);

            // Calculate half extents to center the quad
            Vector2F halfExtents = bounds * 0.5f;

            // Define the 4 vertices of a quad centered at origin in XY plane
            Vector3F[] positions =
            {
                // Bottom-left
                new Vector3F(-halfExtents.X, -halfExtents.Y, 0f),
                // Bottom-right
                new Vector3F( halfExtents.X, -halfExtents.Y, 0f),
                // Top-right
                new Vector3F( halfExtents.X,  halfExtents.Y, 0f),
                // Top-left
                new Vector3F(-halfExtents.X,  halfExtents.Y, 0f),
            };

            // Define normals (all pointing forward along positive Z axis)
            Vector3F[] normals =
            {
                Vector3F.Forward, Vector3F.Forward, Vector3F.Forward, Vector3F.Forward,
            };

            // Define UV coordinates (standard quad mapping)
            Vector2F[] uvs =
            {
                new Vector2F(0, 1), // Bottom-left
                new Vector2F(1, 1), // Bottom-right
                new Vector2F(1, 0), // Top-right
                new Vector2F(0, 0), // Top-left
            };

            // Define colors for each vertex (all white)
            Color[] colors =
            {
                Color.White, Color.White, Color.White, Color.White,
            };

            // Define indices for triangles (2 triangles to form the quad)
            int[] indices =
            {
                // First triangle (bottom-left, bottom-right, top-right)
                0, 1, 2,
                // Second triangle (bottom-left, top-right, top-left)
                0, 2, 3,
            };

            // Set the mesh data
            mesh.SetVertices(positions, normals, uvs, default);
            mesh.SetIndices(indices);

            return mesh;
        }
        
        public static Mesh PrimitiveCube(GraphicsDevice device, Vector3F extents)
        {
            // Check for null device
            if (device == null)
                throw new ArgumentNullException(nameof(device));

            // Create the mesh
            Mesh mesh = new Mesh(device);

            // Calculate half extents to center the cube
            Vector3F halfExtents = extents * 0.5f;

            // Define the 8 vertices of a cube centered at origin
            Vector3F[] positions = // 6 faces * 4 vertices per face
            {
                // Front face (positive Z)
                new Vector3F(-halfExtents.X, -halfExtents.Y,  halfExtents.Z),
                new Vector3F( halfExtents.X, -halfExtents.Y,  halfExtents.Z),
                new Vector3F( halfExtents.X,  halfExtents.Y,  halfExtents.Z),
                new Vector3F(-halfExtents.X,  halfExtents.Y,  halfExtents.Z),

                // Back face (negative Z)
                new Vector3F( halfExtents.X, -halfExtents.Y, -halfExtents.Z),
                new Vector3F(-halfExtents.X, -halfExtents.Y, -halfExtents.Z),
                new Vector3F(-halfExtents.X,  halfExtents.Y, -halfExtents.Z),
                new Vector3F( halfExtents.X,  halfExtents.Y, -halfExtents.Z),

                // Left face (negative X)
                new Vector3F(-halfExtents.X, -halfExtents.Y, -halfExtents.Z),
                new Vector3F(-halfExtents.X, -halfExtents.Y,  halfExtents.Z),
                new Vector3F(-halfExtents.X,  halfExtents.Y,  halfExtents.Z),
                new Vector3F(-halfExtents.X,  halfExtents.Y, -halfExtents.Z),

                // Right face (positive X)
                new Vector3F( halfExtents.X, -halfExtents.Y,  halfExtents.Z),
                new Vector3F( halfExtents.X, -halfExtents.Y, -halfExtents.Z),
                new Vector3F( halfExtents.X,  halfExtents.Y, -halfExtents.Z),
                new Vector3F( halfExtents.X,  halfExtents.Y,  halfExtents.Z),

                // Bottom face (negative Y)
                new Vector3F(-halfExtents.X, -halfExtents.Y, -halfExtents.Z),
                new Vector3F( halfExtents.X, -halfExtents.Y, -halfExtents.Z),
                new Vector3F( halfExtents.X, -halfExtents.Y,  halfExtents.Z),
                new Vector3F(-halfExtents.X, -halfExtents.Y,  halfExtents.Z),

                // Top face (positive Y)
                new Vector3F(-halfExtents.X,  halfExtents.Y,  halfExtents.Z),
                new Vector3F( halfExtents.X,  halfExtents.Y,  halfExtents.Z),
                new Vector3F( halfExtents.X,  halfExtents.Y, -halfExtents.Z),
                new Vector3F(-halfExtents.X,  halfExtents.Y, -halfExtents.Z),
            };

            // Define normals for each face
            Vector3F[] normals =
            {
                // Front face
                Vector3F.Forward, Vector3F.Forward, Vector3F.Forward, Vector3F.Forward,
                // Back face  
                Vector3F.Backward, Vector3F.Backward, Vector3F.Backward, Vector3F.Backward,
                // Left face
                Vector3F.Left, Vector3F.Left, Vector3F.Left, Vector3F.Left,
                // Right face
                Vector3F.Right, Vector3F.Right, Vector3F.Right, Vector3F.Right,
                // Bottom face
                Vector3F.Down, Vector3F.Down, Vector3F.Down, Vector3F.Down,
                // Top face
                Vector3F.Up, Vector3F.Up, Vector3F.Up, Vector3F.Up,
            };

            // Define UV coordinates for each face
            Vector2F[] uvs =
            {
                // Front face
                new Vector2F(0, 1), new Vector2F(1, 1), new Vector2F(1, 0), new Vector2F(0, 0),
                // Back face
                new Vector2F(0, 1), new Vector2F(1, 1), new Vector2F(1, 0), new Vector2F(0, 0),
                // Left face
                new Vector2F(0, 1), new Vector2F(1, 1), new Vector2F(1, 0), new Vector2F(0, 0),
                // Right face
                new Vector2F(0, 1), new Vector2F(1, 1), new Vector2F(1, 0), new Vector2F(0, 0),
                // Bottom face
                new Vector2F(0, 1), new Vector2F(1, 1), new Vector2F(1, 0), new Vector2F(0, 0),
                // Top face
                new Vector2F(0, 1), new Vector2F(1, 1), new Vector2F(1, 0), new Vector2F(0, 0),
            };

            // Define colors for each vertex (all white)
            Color[] colors = new Color[24];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = Color.White;
            }

            // Define indices for triangles (2 triangles per face)
            int[] indices = // 6 faces * 2 triangles * 3 vertices
            {
                // Front face
                0, 1, 2,    0, 2, 3,
                // Back face
                4, 5, 6,    4, 6, 7,
                // Left face
                8, 9, 10,   8, 10, 11,
                // Right face
                12, 13, 14, 12, 14, 15,
                // Bottom face
                16, 17, 18, 16, 18, 19,
                // Top face
                20, 21, 22, 20, 22, 23,
            };

            // Set the mesh data
            mesh.SetVertices(positions, normals, uvs, default);
            mesh.SetIndices(indices);

            return mesh;
        }

        public static Mesh PrimitiveSphere(GraphicsDevice device, float radius, float segments)
        {
            // Check for null device
            if (device == null)
                throw new ArgumentNullException(nameof(device));

            // Ensure minimum segments
            if (segments < 3)
                segments = 3;

            // Create the mesh
            Mesh mesh = new Mesh(device);

            // Calculate vertex and index counts
            int latitudeSegments = (int)segments;
            int longitudeSegments = (int)segments * 2; // Double for longitude to get better aspect ratio
            int vertexCount = (latitudeSegments + 1) * (longitudeSegments + 1);
            int indexCount = latitudeSegments * longitudeSegments * 6;

            // Create arrays for vertex data
            Vector3F[] positions = new Vector3F[vertexCount];
            Vector3F[] normals = new Vector3F[vertexCount];
            Vector2F[] uvs = new Vector2F[vertexCount];
            Color[] colors = new Color[vertexCount];

            // Generate vertices
            int vertexIndex = 0;
            for (int lat = 0; lat <= latitudeSegments; lat++)
            {
                float theta = lat * MathF.PI / latitudeSegments; // 0 to PI
                float sinTheta = MathF.Sin(theta);
                float cosTheta = MathF.Cos(theta);

                for (int lon = 0; lon <= longitudeSegments; lon++)
                {
                    float phi = lon * 2.0f * MathF.PI / longitudeSegments; // 0 to 2*PI
                    float sinPhi = MathF.Sin(phi);
                    float cosPhi = MathF.Cos(phi);

                    // Convert spherical coordinates to Cartesian
                    float x = sinTheta * cosPhi;
                    float y = cosTheta;
                    float z = sinTheta * sinPhi;

                    // Position (scaled by radius)
                    positions[vertexIndex] = new Vector3F(x * radius, y * radius, z * radius);

                    // Normal (same as normalized position vector for a sphere centered at origin)
                    normals[vertexIndex] = new Vector3F(x, y, z);

                    // UV coordinates
                    uvs[vertexIndex] = new Vector2F((float)lon / longitudeSegments, (float)lat / latitudeSegments);

                    // Color (all white)
                    colors[vertexIndex] = Color.White;

                    vertexIndex++;
                }
            }

            // Generate indices
            List<int> indices = new List<int>();

            for (int lat = 0; lat < latitudeSegments; lat++)
            {
                for (int lon = 0; lon < longitudeSegments; lon++)
                {
                    int first = lat * (longitudeSegments + 1) + lon;
                    int second = first + longitudeSegments + 1;

                    // First triangle
                    indices.Add(first);
                    indices.Add(second);
                    indices.Add(first + 1);

                    // Second triangle
                    indices.Add(second);
                    indices.Add(second + 1);
                    indices.Add(first + 1);
                }
            }

            // Set the mesh data
            mesh.SetVertices(positions, normals, uvs, default);
            mesh.SetIndices(indices.ToArray());

            return mesh;
        }
        #endregion
    }
}
