using System;
using System.Collections;

namespace KoraPlayer.Graphics;

public sealed class GraphicsBatch
{
	// Type
	struct DrawKey
	{
		// Public
		public int32 MaterialHash;
		public int32 ShaderHash;
		public int32 RenderQueue;
		public MeshVertexElements VertexElements;

		// Methods
		public int CompareTo(DrawKey other)
		{
			if(MaterialHash != other.MaterialHash)
				return MaterialHash < other.MaterialHash ? -1 : 1;

			if(ShaderHash != other.ShaderHash)
				return ShaderHash < other.ShaderHash ? -1 : 1;

			if(RenderQueue != other.RenderQueue)
				return RenderQueue < other.RenderQueue ? -1 : 1;

			if(VertexElements != other.VertexElements)
				return VertexElements < other.VertexElements ? -1 : 1;

			return 0;
		}
	}

	struct DrawCommand
	{
		// Public
		public DrawKey Key;
		public matrix4 Matrix;
		public Material Material;
		public IndexBufferFormat IndexFormat;
		public GraphicsBuffer IndexBuffer;
		public GraphicsBuffer VertexBuffer;
		public uint32 IndexOffset;
		public uint32 VertexOffset;
		public uint32 Count;
	}

	[CRepr]
	struct Transform
	{
	    public matrix4 ViewMatrix;
	    public matrix4 ProjectionMatrix;
	    public matrix4 ModelMatrix;
	}

	// Private
	private uint32 batchSize = 0;
	private List<DrawCommand> drawCommands;

	private GraphicsCommand renderCommand;
	private matrix4 viewMatrix;
	private matrix4 projectionMatrix;

	// Constructor
	public this(uint32 batchSize)
	{
		this.batchSize = batchSize;
		this.drawCommands = new .(batchSize);
	}

	public ~this()
	{
		delete drawCommands;
		drawCommands = null;
	}

	// Methods
	public Result<void> Begin(GraphicsCommand renderCommand, matrix4? viewMatrix = null, matrix4? projectionMatrix = null)
	{
		if(renderCommand.IsRenderPass == false)
		{
			Debug.LogError("Must be in an active render pass");
			return .Err;
		}

		this.renderCommand = renderCommand;
		this.viewMatrix = viewMatrix != null ? viewMatrix.Value : matrix4.Identity;
		this.projectionMatrix = projectionMatrix != null ? projectionMatrix.Value : matrix4.Identity;

		// Clear draw calls
		drawCommands.Clear();
		return .Ok;
	}

	public Result<void> Draw(matrix4 matrix, Material material, Mesh mesh, uint32 submeshStart = 0, uint32 submeshCount = 1)
	{
	    // Check null
	    if (material == null || mesh == null)
			return .Err;

	    // Get submeshes
	    uint32 start = submeshCount >= mesh.SubmeshCount ? 0 : submeshStart;
	    uint32 count = (submeshStart + submeshCount > mesh.SubmeshCount) ? mesh.SubmeshCount : submeshCount;
	    uint32 end = start + count;

	    // Draw submesh
	    for (uint32 i = start; i < end; i++)
	    {
	        // Get mesh elements
			uint32 indexOffset, vertexOffset, elementCount;
	        mesh.GetElements(out indexOffset, out vertexOffset, out elementCount, i);

	        // Get the mesh vertex elements
	        MeshVertexElements vertexElements = mesh.GetVertexElements(i);

	        // Check for indexed
	        if(mesh.HasIndices == true)
	        {
	            // Get the index format
	            IndexBufferFormat indexFormat = mesh.GetIndexFormat(i);

	            // Draw indexed
	            DrawIndexed(matrix, material, mesh.VertexBuffer, vertexElements, mesh.IndexBuffer, indexFormat, indexOffset, vertexOffset, elementCount);
	        }
	        else
	        {
	            // Draw vertices
	            Draw(matrix, material, mesh.VertexBuffer, vertexElements, vertexOffset, elementCount);
	        }
	    }
		return .Ok;
	}

	public Result<void> Draw(matrix4 matrix, Material material, GraphicsBuffer vertexBuffer, MeshVertexElements elements, uint32 offset, uint32 size)
	{
	    // Check null
	    if (material == null)
			return .Err;

	    // Add the draw call
	    drawCommands.Add(DrawCommand
	    {
	        Key = DrawKey
	        {
	            MaterialHash = (int32)HashCode.Generate(material),
	            ShaderHash = (int32)HashCode.Generate(material.Shader),
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
	    if (drawCommands.Count >= batchSize)
	        Execute();

		return .Ok;
	}

	public Result<void> DrawIndexed(matrix4 matrix, Material material, GraphicsBuffer vertexBuffer, MeshVertexElements elements, GraphicsBuffer indexBuffer, IndexBufferFormat indexFormat, uint32 indexOffset, uint32 vertexOffset, uint32 size)
	{
	    // Check null
	    if (material == null)
			return .Err;

	    // Add the draw call
	    drawCommands.Add(DrawCommand
	    {
	        Key = DrawKey
	        {
	            MaterialHash = (int32)HashCode.Generate(material),
				ShaderHash = (int32)HashCode.Generate(material.Shader),
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
	    if (drawCommands.Count >= batchSize)
	        Execute();

		return .Ok;
	}

	public void End()
	{
		// Flush remaining calls
		Execute();
	}

	private void Execute()
	{
		// Check for any
		if(drawCommands.Count == 0)
			return;

		// Sort draw calls
		drawCommands.Sort((a, b) => a.Key.CompareTo(b.Key));

		// Get the key
		DrawKey currentKey = default;

		// Create the transform
		Transform transform = .
		{
			ViewMatrix = viewMatrix,
			ProjectionMatrix = projectionMatrix,
			ModelMatrix = default,
		};

		// Draw in order
		for(int32 i = 0; i < drawCommands.Count; i++)
		{
			// Get the draw command
			DrawCommand draw = drawCommands[i];

			// Bind transform
			transform.ModelMatrix = draw.Matrix;
			renderCommand.BindUniform(0, .Vertex, transform);

			// Check for batch change
			if(draw.Key.CompareTo(currentKey) != 0)
			{
				// Update the current render key
				currentKey = draw.Key;

				// Bind material
				draw.Material.Bind(renderCommand, .TriangleList, draw.Key.VertexElements);
			}

			// Bind vertex buffers
			renderCommand.BindVertexBuffer(draw.VertexBuffer);

			// Bind index buffer
			if(draw.IndexBuffer != null)
				renderCommand.BindIndexBuffer(draw.IndexBuffer, draw.IndexFormat);

			// Draw primitives
			if(draw.IndexBuffer != null)
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
		drawCommands.Clear();
	}
}