
using System.Runtime.InteropServices;

namespace KoraGame.Graphics
{
    public sealed class GraphicsBatch
    {
        // Type
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

        [StructLayout(LayoutKind.Sequential)]
        private struct Transform
        {
            public Matrix4F ViewMatrix;
            public Matrix4F ProjectionMatrix;
            public Matrix4F ModelMatrix;
        }

        // Private
        private static readonly DrawCommandComparer keyComparer = new();

        private uint batchSize = 0;
        private List<DrawCommand> batchDraw = null;

        private GraphicsCommand renderCommand;
        private Matrix4F viewMatrix = Matrix4F.Identity;
        private Matrix4F projectionMatrix = Matrix4F.Identity;

        // Properties
        public GraphicsCommand Command => renderCommand;
        public Matrix4F ViewMatrix => viewMatrix;
        public Matrix4F ProjectionMatrix => projectionMatrix;

        // Constructor
        public GraphicsBatch(uint batchSize)
        {
            this.batchSize = batchSize;
            this.batchDraw = new((int)batchSize);
        }

        // Methods
        public void Begin(GraphicsCommand renderCommand, Matrix4F viewMatrix, Matrix4F projectionMatrix)
        {
            this.renderCommand = renderCommand;
            this.viewMatrix = viewMatrix;
            this.projectionMatrix = projectionMatrix;

            // Clear draw calls
            batchDraw.Clear();
        }

        public void SubmitDrawCall(Matrix4F matrix, Material material, GraphicsBuffer vertexBuffer, MeshVertexElements elements, uint offset, uint size)
        {
            // Check null
            if (material == null)
                throw new ArgumentNullException(nameof(material));

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
                VertexOffset = offset,
                Count = size,
            });

            // Check for flush
            if (batchDraw.Count >= batchSize)
                Execute();
        }

        public void SubmitIndexedDrawCall(Matrix4F matrix, Material material, GraphicsBuffer vertexBuffer, MeshVertexElements elements, GraphicsBuffer indexBuffer, IndexBufferFormat indexFormat, uint indexOffset, uint vertexOffset, uint size)
        {
            // Check null
            if (material == null)
                throw new ArgumentNullException(nameof(material));

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

            // Check for flush
            if (batchDraw.Count >= batchSize)
                Execute();
        }

        public void End()
        {
            // Flush remaining calls
            Execute();

            // Reset matrix
            this.viewMatrix = Matrix4F.Identity;
        }

        private void Execute()
        {
            // Check for any
            if (batchDraw.Count == 0)
                return;

            // Sort by key
            batchDraw.Sort(keyComparer);

            // Get the key
            DrawKey currentKey = default;

            // Create the transform
            Transform transform = new Transform
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
                renderCommand.BindUniform(transform, 0, ShaderStage.Vertex);

                // Change the batch if required
                if(draw.Key.Equals(currentKey) == false)
                {
                    // Update current render key
                    currentKey = draw.Key;

                    // Bind material
                    draw.Material.Bind(renderCommand, draw.Key.VertexElements);
                }                

                // Bind vertex buffers
                renderCommand.BindVertexBuffer(draw.VertexBuffer);

                // Bind index buffers
                if (draw.IndexBuffer != null)
                    renderCommand.BindIndexBuffer(draw.IndexBuffer, draw.IndexFormat);                


                // Draw the primitives
                if (draw.IndexBuffer != null)
                {
                    // Draw indexed
                    renderCommand.DrawIndexedPrimitives(draw.Count, 1, draw.IndexOffset, 0, draw.VertexOffset);
                }
                else
                {
                    // Draw vertex
                    renderCommand.DrawPrimitives(draw.Count, 1, draw.VertexOffset);
                }
            }

            // Clear all draw calls
            batchDraw.Clear();
        }
    }
}
