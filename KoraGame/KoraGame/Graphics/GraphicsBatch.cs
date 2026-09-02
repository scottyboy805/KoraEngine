using System.Runtime.InteropServices;

namespace KoraGame.Graphics
{
    public sealed class GraphicsBatch
    {
        // Type
        #region Type
        private sealed class DrawCommandComparer : IComparer<DrawCommand>
        {
            public int Compare(DrawCommand x, DrawCommand y)
            {
                // 1. Sort by render queue (e.g., opaque first, transparent later)
                int result = x.Key.RenderQueue.CompareTo(y.Key.RenderQueue);
                if (result != 0)
                    return result;

                // 2. Sort by shader (group by shader to minimize program switches)
                result = x.Key.ShaderHash.CompareTo(y.Key.ShaderHash);
                if (result != 0)
                    return result;

                // 3. Sort by vertex elements
                result = x.Key.VertexElements.CompareTo(y.Key.VertexElements);
                if (result != 0)
                    return result;

                // 4. Sort by material (group by texture/state)
                result = x.Key.MaterialHash.CompareTo(y.Key.MaterialHash);
                if (result != 0)
                    return result;

                // 5. Equal keys — keep stable order
                return 0;
            }
        }

        private struct DrawKey
        {
            // Public
            public int MaterialHash;
            public int ShaderHash;
            public int RenderQueue;
            public MeshVertexElements VertexElements;

            // Methods
            public bool Equals(in DrawKey other)
            {
                return MaterialHash == other.MaterialHash
                    && ShaderHash == other.ShaderHash
                    && RenderQueue == other.RenderQueue
                    && VertexElements == other.VertexElements;
            }
        }

        private struct DrawCommand
        {
            // Public
            public DrawKey Key;
            public Matrix4F Matrix;
            public Material Material;
            public IndexBufferFormat IndexFormat;
            public GraphicsBuffer IndexBuffer;
            public GraphicsBuffer VertexBuffer;
            public uint IndexOffset;
            public uint VertexOffset;
            public uint Count;
        }
        #endregion

        // Private
        private static readonly DrawCommandComparer keyComparer = new();

        private readonly uint batchSize = 0;
        private readonly List<DrawCommand> batchDraw = null;

        // Properties
        public bool IsEmpty => batchDraw.Count == 0;
        public bool IsFull => batchDraw.Count >= batchSize;

        // Constructor
        public GraphicsBatch(uint batchSize)
        {
            this.batchSize = batchSize;
            this.batchDraw = new((int)batchSize);
        }

        // Methods
        public void PushDraw(Matrix4F modelMatrix, Material material, GraphicsBuffer vertexBuffer, MeshVertexElements elements, uint offset, uint size)
        {
            // Check null
            if (material == null)
                throw new ArgumentNullException(nameof(material));

            if (vertexBuffer == null)
                throw new ArgumentNullException(nameof(vertexBuffer));

            // Check for too many
            if (batchDraw.Count >= batchSize)
            {
                Debug.LogWarning("Draw call would exceed max batch count. Execute the batch first");
                return;
            }

            // Add the draw call
            batchDraw.Add(new DrawCommand
            {
                Key = new DrawKey
                {
                    MaterialHash = material.GetHashCode(),
                    ShaderHash = material.Shader.GetHashCode(),
                    RenderQueue = 0,
                    VertexElements = elements,
                },
                Matrix = modelMatrix,
                Material = material,
                VertexBuffer = vertexBuffer,
                VertexOffset = offset,
                Count = size,
            });
        }

        public void PushDrawIndexed(Matrix4F matrix, Material material, GraphicsBuffer vertexBuffer, MeshVertexElements elements, GraphicsBuffer indexBuffer, IndexBufferFormat indexFormat, uint indexOffset, uint vertexOffset, uint size)
        {
            // Check null
            if (material == null)
                throw new ArgumentNullException(nameof(material));

            // Check for too many
            if (batchDraw.Count >= batchSize)
            {
                Debug.LogWarning("Draw call would exceed max batch count. Execute the batch first");
                return;
            }

            // Add the draw call
            batchDraw.Add(new DrawCommand
            {
                Key = new DrawKey
                {
                    MaterialHash = material.GetHashCode(),
                    ShaderHash = material.Shader.GetHashCode(),
                    RenderQueue = 0,
                    VertexElements = elements,
                },
                Matrix = matrix,
                Material = material,
                VertexBuffer = vertexBuffer,
                IndexBuffer = indexBuffer,
                IndexFormat = indexFormat,
                IndexOffset = indexOffset,
                VertexOffset = vertexOffset,
                Count = size,
            });
        }

        public void Execute(GraphicsCommand graphics, Matrix4F viewMatrix, Matrix4F projectionMatrix)
        {
            // Check for any
            if (batchDraw.Count == 0)
                return;

            // Sort by key
            batchDraw.Sort(keyComparer);

            // Get the key
            DrawKey currentKey = default;

            // Create the transform
            GraphicsCommand.TransformUniform transform = new ()
            {
                ViewMatrix = viewMatrix,
                ProjectionMatrix = projectionMatrix,
            };

            // Draw ll commands
            for(int i = 0; i < batchDraw.Count; i++)
            {
                // Get the draw command
                DrawCommand draw = batchDraw[i];

                // Bind transform
                transform.ModelMatrix = draw.Matrix;
                graphics.BindUniform(transform, 0, ShaderStage.Vertex);

                // Change the batch if required
                if(draw.Key.Equals(currentKey) == false)
                {
                    // Update current render key
                    currentKey = draw.Key;

                    // Bind material
                    draw.Material.Bind(graphics, draw.Key.VertexElements);
                }                

                // Bind vertex buffers
                graphics.BindVertexBuffer(draw.VertexBuffer);

                // Bind index buffers
                if (draw.IndexBuffer != null)
                    graphics.BindIndexBuffer(draw.IndexBuffer, draw.IndexFormat);                


                // Draw the primitives
                if (draw.IndexBuffer != null)
                {
                    // Draw indexed
                    graphics.DrawIndexedPrimitives(draw.Count, 1, draw.IndexOffset, 0, draw.VertexOffset);
                }
                else
                {
                    // Draw vertex
                    graphics.DrawPrimitives(draw.Count, 1, draw.VertexOffset);
                }
            }

            // Clear all draw calls
            batchDraw.Clear();
        }
    }
}
