using SDL3;
using System.Collections;
using System;
using System.Interop;

namespace KoraPlayer.Graphics;

public enum MeshError
{
	Unknown,
	NoSubmesh,
	NoData,
	InvalidVertexData,
	InvalidFormat,
}

enum MeshVertexElements : uint32
{
	None = 0,
	Position2 = 1 << 0,
	Position3 = 1 << 1,
	Normal = 1 << 2,
	UV = 1 << 3,
	Color = 1 << 4,
	Color32 = 1 << 5,
}

public enum MeshPrimitiveType : c_int
{
	TriangleList,
	TriangleStrip,
	LineList,
	LineStrip,
	PointList,
}

public sealed class Mesh : NativeElement
{
	// Type
	struct SubMesh
	{
		// Public
		public IndexBufferFormat IndexFormat;
		public uint32 IndexOffset;
		public uint32 IndexCount;
		public MeshVertexElements VertexElements;
		public uint32 VertexOffset;
		public uint32 VertexCount;

		// Properties
		public bool HasIndices => IndexCount > 0;
		public bool HasVertices => VertexCount > 0 && VertexElements != 0;
	}

	// Private
	private GraphicsDevice device;
	private GraphicsBuffer indexBuffer;
	private GraphicsBuffer vertexBuffer;
	private SubMesh[] submeshes;

	// Properties
	public bool HasIndices => indexBuffer != null;
	public bool HasVertices => vertexBuffer != null;
	public uint32 SubmeshCount => (uint32)submeshes.Count;

	public GraphicsBuffer IndexBuffer => indexBuffer;
	public GraphicsBuffer VertexBuffer => vertexBuffer;

	// Constructor
	public this(GraphicsDevice device, uint32 subMeshCount = 0)
	{
		this.device = device;

		uint size = subMeshCount;

		// Check bounds - should always be 1 sub mesh (the first mesh)
		if(size <= 0)
			size = 1;

		// Create array
		submeshes = new SubMesh[size];
	}

	internal ~this()
	{
		delete submeshes;
		submeshes = null;

		if(indexBuffer != null)
		{
			delete indexBuffer;
			indexBuffer = null;
		}

		if(vertexBuffer != null)
		{
			delete vertexBuffer;
			vertexBuffer = null;
		}
	}

	// Methods
	public uint32 GetElementCount(uint32 submesh = 0)
	{
		if(submesh >= (uint)submeshes.Count)
			return 0;

		return HasIndices == true
			? submeshes[submesh].IndexCount
			: submeshes[submesh].VertexCount;
	}

	public void GetElements(out uint32 indexOffset, out uint32 vertexOffset, out uint32 elementCount, uint32 submesh = 0)
	{
		// Check bounds
		if(submesh >= submeshes.Count)
		{
			indexOffset = 0;
			vertexOffset = 0;
			elementCount = 0;
			return;
		}

		// Get the sub mesh
		SubMesh mesh = submeshes[submesh];

		// Get offsets
		indexOffset = mesh.IndexOffset;
		vertexOffset = mesh.VertexOffset;

		// Get count
		elementCount = mesh.HasIndices == true
			? mesh.IndexCount
			: mesh.VertexCount;
	}

	public IndexBufferFormat GetIndexFormat(uint32 submesh = 0)
	{
		if(submesh >= (uint)submeshes.Count)
			return 0;

		// Get the format
		return submeshes[(int)submesh].IndexFormat;
	}

	public MeshVertexElements GetVertexElements(uint32 submesh = 0)
	{
		if(submesh >= (uint)submeshes.Count)
			return 0;

		// Get the elements
		return submeshes[(int)submesh].VertexElements;
	}

	public Result<void, MeshError> SetIndices(Span<uint32> indices, uint32 submesh = 0)
	{
		// Check for data provided
		if(indices.Length == 0)
			return .Err(.NoData);

		return SetIndices(indices.Ptr, (uint32)indices.Length, submesh);
	}

	public Result<void, MeshError> SetIndices(uint32* indices, uint32 size, uint32 submesh = 0)
	{
		// Check bounds
		if(submesh >= (uint)submeshes.Count)
			return .Err(.NoSubmesh);
		
		// Check indices
		if(indices == null || size == 0)
			return .Err(.NoData);

		// Get index format
		IndexBufferFormat indexFormat = size >= uint16.MaxValue
			? .Int32
			: .Int16;

		// Calculate sizes
		uint32 indexSize = GetIndexSize(indexFormat);
		uint32 newDataSize = size * indexSize;

		// Calculate total buffer size needed and track offsets
		uint32 totalSize = 0;
		uint32 thisSubmeshOffset = 0;

		for(uint32 i = 0; i < submeshes.Count; i++)
		{
			if(i == submesh)
			{
				thisSubmeshOffset = totalSize;
				totalSize += newDataSize;
			}
			else if(submeshes[i].HasIndices == true)
			{
				uint32 existingIndexSize = GetIndexSize(submeshes[i].IndexFormat);
				totalSize += submeshes[i].IndexCount * existingIndexSize;
			}
		}

		// Create new buffer
		GraphicsBuffer newBuffer = new .(device, totalSize, .Index);

		// Write to buffer
		Span<uint8> data = newBuffer.BeginWrite<uint8>();
		{
			uint8* newPtr = data.Ptr;
			uint32 writeOffset = 0;

			// Copy existing sub mesh data and write new data
			if(indexBuffer != null)
			{
				Span<uint8> oldData = indexBuffer.BeginWrite<uint8>();
				{
					uint8* oldPtr = oldData.Ptr;
					uint32 readOffset = 0;

					for(uint32 i = 0; i < submeshes.Count; i++)
					{
						if(i == submesh)
						{
							// Write new data for this sub mesh
							WriteSubMeshIndexData(newPtr + writeOffset, indexFormat, Span<uint32>(indices, size));
							writeOffset += newDataSize;
						}
						else if(submeshes[i].HasIndices == true)
						{
							// Copy existing data as is
							uint32 existingIndexSize = GetIndexSize(submeshes[i].IndexFormat);
							uint32 existingDataSize = submeshes[i].IndexCount * existingIndexSize;

							// Block copy
							Internal.MemCpy(newPtr + writeOffset, oldPtr + readOffset, existingDataSize);

							readOffset += existingDataSize;
							writeOffset += existingDataSize;
						}
					}
				}
				indexBuffer.EndWrite();
			}
			else
			{
				// No existing buffer - just write new data only
				WriteSubMeshIndexData(newPtr + thisSubmeshOffset, indexFormat, Span<uint32>(indices, size));
			}
		}
		newBuffer.EndWrite();

		// Update submesh info
		submeshes[submesh].IndexFormat = indexFormat;
		submeshes[submesh].IndexCount = size;

		// Calculate and update IndexOffset for all sub meshes
		uint32 currentOffset = 0;
		for (uint32 i = 0; i < submeshes.Count; i++)
		{
		    if (submeshes[i].HasIndices == true)
		    {
		        submeshes[i].IndexOffset = currentOffset;
		        currentOffset += submeshes[i].IndexCount;
		    }
		    else
		    {
		        submeshes[i].IndexOffset = 0;
		    }
		}

		// Delete old buffer
		delete indexBuffer;

		// Replace buffer
		indexBuffer = newBuffer;
		

		static void WriteSubMeshIndexData(uint8* ptr, IndexBufferFormat format, Span<uint32> indices)
		{
			var ptr;
			for(uint32 i = 0; i < indices.Length; i++)
			{
				switch(format)
				{
				case .Int16:
					{
						*(uint16*)ptr = (uint16)indices[i];
						ptr += sizeof(uint16);
					}
				case .Int32:
					{
						*(uint32*)ptr = (uint32)indices[i];
						ptr += sizeof(uint32);
					}
				}
			}
		}	

		return .Ok;
	}

	public Result<void, MeshError> SetVertices(Span<float3> positions, Span<float3> normals = null, Span<float2> uvs = null, Span<Color> colors = null, uint32 submesh = 0)
	{
		// Check for positions
		if(positions.Length == 0)
			return .Err(.NoData);

		// Get expected count
		uint32 vertexCount = (uint32)positions.Length;

		MeshVertexElements elements = .Position3;

		if(normals.Length > 0) elements |= .Normal;
		if(uvs.Length > 0) elements |= .UV;
		if(colors.Length > 0) elements |= .Color;

		// Validate that provided data is of the correct size
		if((elements & .Normal) != 0 && normals.Length != vertexCount ||
			(elements & .UV) != 0 && uvs.Length != vertexCount ||
			(elements & .Color) != 0 && colors.Length != vertexCount)
		{
			// All sizes must match
			return .Err(.InvalidVertexData);
		}

		// Check bounds
		if(submesh >= (uint)submeshes.Count)
			return .Err(.NoSubmesh);

		// Get the vertex size
		uint32 vertexSize = GetVertexSize(elements);
		uint32 newDataSize = (uint32)positions.Length * vertexSize;

		// Calculate total buffer size needed
		uint32 totalSize = 0;
		uint32 thisSubMeshOffset = 0;

		for(uint32 i = 0; i < submeshes.Count; i++)
		{
			if (i == submesh)
			{
			    thisSubMeshOffset = totalSize;
			    totalSize += newDataSize;
			}
			else if (submeshes[i].HasVertices)
			{
			    uint32 existingVertexSize = GetVertexSize(submeshes[i].VertexElements);
			    totalSize += submeshes[i].VertexCount * existingVertexSize;
			}
		}

		// Create new buffer
		GraphicsBuffer newBuffer = new .(device, totalSize, .Vertex);

		// Copy existing data and write new data
		Span<uint8> data = newBuffer.BeginWrite<uint8>();
		{
			uint8* newPtr = data.Ptr;
			uint32 writeOffset = 0;

			// Copy existing sub mesh data and write new data
			if(vertexBuffer != null)
			{
				Span<uint8> oldData = vertexBuffer.BeginWrite<uint8>();
				{
					uint8* oldPtr = oldData.Ptr;
					uint32 readOffset = 0;

					for(uint32 i = 0; i < submeshes.Count; i++)
					{
						if(i == submesh)
						{
							// Write new data for this sub mesh
							WriteSubMeshVertexData(newPtr + writeOffset, vertexSize, elements, positions, normals, uvs, colors);
							writeOffset += newDataSize;
						}
						else if(submeshes[i].HasVertices == true)
						{
							// Copy existing data as is
							uint32 existingVertexSize = GetVertexSize(submeshes[i].VertexElements);
							uint32 existingDataSize = submeshes[i].VertexCount * existingVertexSize;

							// Direct block copy
							Internal.MemCpy(newPtr + writeOffset, oldPtr + readOffset, existingDataSize);

							readOffset += existingDataSize;
							writeOffset += existingDataSize;
						}
					}
				}
				vertexBuffer.EndWrite();
			}
			else
			{
				// No existing buffer, just write new data
				WriteSubMeshVertexData(newPtr + thisSubMeshOffset, vertexSize, elements, positions, normals, uvs, colors);
			}
		}
		newBuffer.EndWrite();

		// Update submesh info
		submeshes[submesh].VertexElements = elements;
		submeshes[submesh].VertexCount = (uint32)positions.Length;

		// Calculate and update VertexOffset for all sub meshes
		uint32 currentOffset = 0;
		for (uint32 i = 0; i < submeshes.Count; i++)
		{
		    if (submeshes[i].HasVertices == true)
		    {
		        submeshes[i].VertexOffset = currentOffset;
		        currentOffset += submeshes[i].VertexCount;
		    }
		    else
		    {
		        submeshes[i].VertexOffset = 0;
		    }
		}

		// Delete old buffer
		delete vertexBuffer;

		// Replace buffer
		vertexBuffer = newBuffer;

		static void WriteSubMeshVertexData(uint8* ptr, uint vertexSize, MeshVertexElements elements, Span<float3> positions, Span<float3> normals, Span<float2> uvs, Span<Color> colors)
		{
		    for (uint32 i = 0; i < positions.Length; i++)
		    {
		        uint8* vertexPtr = ptr + (vertexSize * i);

		        // Write position
		        if ((elements & MeshVertexElements.Position3) != 0)
		        {
		            *(float3*)vertexPtr = positions[i];
		            vertexPtr += sizeof(float3);
		        }

		        // Write normal
		        if ((elements & MeshVertexElements.Normal) != 0)
		        {
		            *(float3*)vertexPtr = normals[i];
		            vertexPtr += sizeof(float3);
		        }

		        // Write uv
		        if ((elements & MeshVertexElements.UV) != 0)
		        {
		            *(float2*)vertexPtr = uvs[i];
		            vertexPtr += sizeof(float2);
		        }

		        // Write color
		        if ((elements & MeshVertexElements.Color) != 0)
		        {
		            *(Color*)vertexPtr = colors[i];
		            vertexPtr += sizeof(Color);
		        }
		    }
		}

		return .Ok;
	}

	public Result<void, MeshError> GetIndices(List<int32> outIndices, uint32 subMesh = 0)
	{
		// Check bounds
		if(subMesh >= (uint)submeshes.Count)
			return .Err(.NoSubmesh);

		// Get the sub mesh
		SubMesh submesh = submeshes[(int)subMesh];

		// Check for any
		if(submesh.HasIndices == false || indexBuffer == null)
			return .Err(.NoData);

		// Get the index size
		uint indexSize = GetIndexSize(submesh.IndexFormat);

		// Check compatible size
		if(indexSize != sizeof(int32))
			return .Err(.InvalidFormat);

		// Get offset into index buffer
		uint offset = 0;
		for(uint i = 0; i < subMesh; i++)
			offset += submeshes[(int)i].IndexCount * indexSize;

		// Read from the index buffer
		for(uint i = offset; i < offset + submesh.IndexCount; i += indexSize)
		{
			// Get the pointer
		}

		return .Ok;
	}

	public static uint32 GetIndexSize(IndexBufferFormat format)
	{
		switch(format)
		{
		case .Int16: return sizeof(int16);
		case .Int32: return sizeof(int32);
		}	
	}

	public static uint32 GetVertexSize(MeshVertexElements elements)
	{
		uint32 size = 0;

		// Calculate required size for one vertex element
		if((elements & .Position2) != 0) size += sizeof(float2);
		if((elements & .Position3) != 0) size += sizeof(float3);
		if((elements & .Normal) != 0) size += sizeof(float3);
		if((elements & .UV) != 0) size += sizeof(float2);
		if((elements & .Color) != 0) size += sizeof(Color);
		if((elements & .Color32) != 0) size += sizeof(Color32);

		return size;
	}

	public static Mesh CreatePrimitiveCube(GraphicsDevice device, float3 extents)
	{
		// Check for null
		if(device == null)
			return null;

		// Create tbe mesh
		Mesh mesh = new .(device);

		// Calculate half extents to center the cube
		float3 halfExtents = extents * 0.5f;

		// Define the 8 vertices of a cube centered at origin
		let positions = scope float3[] // 6 faces * 4 vertices per face
		(
		    // Front face (positive z)
		    float3(-halfExtents.x, -halfExtents.y,  halfExtents.z),
		    float3( halfExtents.x, -halfExtents.y,  halfExtents.z),
		    float3( halfExtents.x,  halfExtents.y,  halfExtents.z),
		    float3(-halfExtents.x,  halfExtents.y,  halfExtents.z),

		    // Back face (negative z)
		    float3( halfExtents.x, -halfExtents.y, -halfExtents.z),
		    float3(-halfExtents.x, -halfExtents.y, -halfExtents.z),
		    float3(-halfExtents.x,  halfExtents.y, -halfExtents.z),
		    float3( halfExtents.x,  halfExtents.y, -halfExtents.z),

		    // Left face (negative x)
		    float3(-halfExtents.x, -halfExtents.y, -halfExtents.z),
		    float3(-halfExtents.x, -halfExtents.y,  halfExtents.z),
		    float3(-halfExtents.x,  halfExtents.y,  halfExtents.z),
		    float3(-halfExtents.x,  halfExtents.y, -halfExtents.z),

		    // Right face (positive x)
		    float3( halfExtents.x, -halfExtents.y,  halfExtents.z),
		    float3( halfExtents.x, -halfExtents.y, -halfExtents.z),
		    float3( halfExtents.x,  halfExtents.y, -halfExtents.z),
		    float3( halfExtents.x,  halfExtents.y,  halfExtents.z),

		    // Bottom face (negative y)
		    float3(-halfExtents.x, -halfExtents.y, -halfExtents.z),
		    float3( halfExtents.x, -halfExtents.y, -halfExtents.z),
		    float3( halfExtents.x, -halfExtents.y,  halfExtents.z),
		    float3(-halfExtents.x, -halfExtents.y,  halfExtents.z),

		    // Top face (positive y)
		     float3(-halfExtents.x,  halfExtents.y,  halfExtents.z),
		     float3( halfExtents.x,  halfExtents.y,  halfExtents.z),
		     float3( halfExtents.x,  halfExtents.y, -halfExtents.z),
		     float3(-halfExtents.x,  halfExtents.y, -halfExtents.z),
		);

		// Define normals for each face
		let normals = scope float3[]
		(
		    // Front face
		    float3.Forward, float3.Forward, float3.Forward, float3.Forward,
		    // Back face  
		    float3.Backward, float3.Backward, float3.Backward, float3.Backward,
		    // Left face
		    float3.Left, float3.Left, float3.Left, float3.Left,
		    // Right face
		    float3.Right, float3.Right, float3.Right, float3.Right,
		    // Bottom face
		    float3.Down, float3.Down, float3.Down, float3.Down,
		    // Top face
		    float3.Up, float3.Up, float3.Up, float3.Up,
		);

		// Define UV coordinates for each face
		let uvs = scope float2[]
		(
		    // Front face
		    float2(0, 1), float2(1, 1), float2(1, 0), float2(0, 0),
		    // Back face
		    float2(0, 1), float2(1, 1), float2(1, 0), float2(0, 0),
		    // Left face
		    float2(0, 1), float2(1, 1), float2(1, 0), float2(0, 0),
		    // Right face
		    float2(0, 1), float2(1, 1), float2(1, 0), float2(0, 0),
		    // Bottom face
		    float2(0, 1), float2(1, 1), float2(1, 0), float2(0, 0),
		    // Top face
		    float2(0, 1), float2(1, 1), float2(1, 0), float2(0, 0),
		);

		// Define colors for each vertex (all white)
		let colors = scope Color[24];
		for (int i = 0; i < colors.Count; i++)
		{
		    colors[i] = Color.White;
		}

		// Define indices for triangles (2 triangles per face)
		let indices = scope uint32[]// 6 faces * 2 triangles * 3 vertices
		(
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
		);

		// Set the mesh data
		mesh.SetVertices(positions, normals, uvs, default);
		mesh.SetIndices(indices);

		return mesh;
	}
}