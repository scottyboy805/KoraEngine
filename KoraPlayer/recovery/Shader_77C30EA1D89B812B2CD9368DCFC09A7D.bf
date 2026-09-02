using SDL3;
using System;
using System.Collections;
using System.Interop;
using internal KoraPlayer;
using internal KoraPlayer.Graphics;

namespace KoraPlayer.Graphics;

public enum ShaderFormat : uint32
{
	Spirv = (1u << 1),
	DXBC = (1u << 2),
	DXIL  = (1u << 3),
	MSL = (1u << 4),
	MetalLib = (1u << 5),
}

public enum ShaderStage : c_int
{
	Vertex,
	Fragment,
}

public enum ShaderCullMode : c_int
{
	None = 0,
	Front,
	Back,
}

public enum ShaderPropertyType : c_int
{
	Color,
	Matrix4,
	Texture,
}

public sealed class ShaderProperty
{
	// Private
	private String name = new .() ~delete _;
	private uint32 location;
	private ShaderStage stage;
	private ShaderPropertyType type;

	// Properties
	public StringView Name => name;
	public uint32 Location => location;
	public ShaderStage Stage => stage;
	public ShaderPropertyType Type => type;

	// Constructor
	private this() {}

	internal this(StringView name, uint32 location, ShaderStage stage, ShaderPropertyType type)
	{
		this.name.Set(name);
		this.location = location;
		this.stage = stage;
		this.type = type;
	}
}

public sealed class ShaderSource
{
	// Private
	private RawAsset source;
	private String entryPoint = new .() ~delete _;
	private uint32 uniformCount;
	private uint32 samplerCount;

	// Properties
	public RawAsset Source => source;
	public StringView EntryPoint => entryPoint;
	public uint32 UniformCount => uniformCount;
	public uint32 SamplerCount => samplerCount;

	// Constructor
	private this() {}

	internal this(RawAsset source, StringView entryPoint, uint32 uniformCount, uint32 samplerCount)
	{
		this.source = source;
		this.entryPoint.Set(entryPoint);
		this.uniformCount = uniformCount;
		this.samplerCount = samplerCount;
	}
}

public sealed class Shader : NativeElement
{
	// Type
	struct ShaderPipelineKey : IHashable
	{
		// Public
		public MeshPrimitiveType Type;
		public MeshVertexElements Elements;

		// Methods
		public int GetHashCode()
		{
			let a = HashCode.Generate(Type);
			let b = HashCode.Generate(Elements);
			
			return HashCode.Mix(a, b);
		}
	}


	// Private
	private readonly ShaderFormat format;
	private readonly Dictionary<ShaderPipelineKey, SDL_GPUGraphicsPipeline*> pipelines = new .();
	//private readonly List<UniformAttribute> uniforms = new .() ~delete _;

	// Private
	private MeshPrimitiveType primitiveType = .TriangleList;
	private ShaderCullMode cullMode = .Back;
	private ShaderSource vertexSource;
	private ShaderSource fragmentSource;
	private List<ShaderProperty> properties = new .();

	// Internal
	internal SDL_GPUDevice* gpuDevice;
	internal SDL_GPUShader* gpuVertexShader;
	internal SDL_GPUShader* gpuFragmentShader;

	// Properties
	public ShaderFormat Format => format;
	public MeshPrimitiveType PrimitiveType => primitiveType;
	public ShaderCullMode CullMode => cullMode;
	public List<ShaderProperty> Properties => properties;

	// Constructor
	public this(GraphicsDevice device, ShaderSource vertexSource, ShaderSource fragmentSource, ShaderFormat format)
	{
		this.gpuDevice = device.gpuDevice;
		this.format = format;
		this.vertexSource = vertexSource;
		this.fragmentSource = fragmentSource;

		// Setup shader stages
		this.gpuVertexShader = InitializeShader(vertexSource, format, .Vertex);
		this.gpuFragmentShader = InitializeShader(fragmentSource, format, .Fragment);
	}

	public ~this()
	{
		// Release all variants
		for(let pipeline in pipelines.Values)
			SDL_ReleaseGPUGraphicsPipeline(gpuDevice, pipeline);

		delete pipelines;

		// Release all properties
		for(let property in properties)
			delete property;

		delete properties;

		// Release the shaders
		SDL_ReleaseGPUShader(gpuDevice, gpuVertexShader);
		SDL_ReleaseGPUShader(gpuDevice, gpuFragmentShader);

		gpuDevice = null;
		gpuVertexShader = null;
		gpuFragmentShader = null;
	}

	// Methods
//	public bool GetUniform(StringView name, out uint location, out UniformElementFormat format)
//	{
//		location = 0;
//		format = 0;
//
//		for(let uniform in uniforms)
//		{
//			// Check for matching name
//			if(name.CompareTo(uniform.Name) == 0)
//			{
//				location = uniform.Location;
//				format = uniform.Format;
//				return true;
//			}
//		}
//		return false;
//	}

	internal SDL_GPUGraphicsPipeline* GetOrCreatePipeline(MeshPrimitiveType primitiveType, MeshVertexElements elements)
	{
		// Create the key
		ShaderPipelineKey key = ShaderPipelineKey
		{
		  	Type = primitiveType,
			Elements = elements,
		};

		// Try to lookup
		SDL_GPUGraphicsPipeline* variant;
		if(pipelines.TryGetValue(key, out variant) == true)
			return variant;

		// Create and cache the variant
		variant = InitializePipeline(&key);
		pipelines[key] = variant;

		// Finally get the pipeline
		return variant;
	}

	private SDL_GPUShader* InitializeShader(ShaderSource shaderSource, ShaderFormat format, ShaderStage stage)
	{
		// Get the raw asset
		RawAsset sourceAsset = shaderSource.Source;

		// Get the bytes
		Span<uint8> sourceBytes = sourceAsset.GetBytes();

		// Get the entry point string
		char8* entryPoint = shaderSource.EntryPoint.ToScopeCStr!();

		// Create shader info
		SDL_GPUShaderCreateInfo shaderInfo = SDL_GPUShaderCreateInfo
		{
			code = sourceBytes.Ptr,
			code_size = (uint)sourceBytes.Length,
			entrypoint = entryPoint,
			format = (SDL_GPUShaderFormat)format,
			stage = (SDL_GPUShaderStage)stage,
			num_uniform_buffers = shaderSource.UniformCount,
			num_samplers = shaderSource.SamplerCount,
			num_storage_buffers = 0,
			num_storage_textures = 0,
			props = 0,
		};

		// Create the shader
		SDL_GPUShader* shader = SDL3.SDL_CreateGPUShader(gpuDevice, &shaderInfo);

		// Check for error

		return shader;
	}

	private SDL_GPUGraphicsPipeline* InitializePipeline(ShaderPipelineKey* key)
	{
		// Create color target
		SDL_GPUColorTargetDescription colorTargetDescription = SDL_GPUColorTargetDescription
		{
			blend_state = SDL_GPUColorTargetBlendState
			{
				enable_blend = true,
				color_blend_op = .SDL_GPU_BLENDOP_ADD,
				alpha_blend_op = .SDL_GPU_BLENDOP_ADD,
				src_color_blendfactor = .SDL_GPU_BLENDFACTOR_SRC_ALPHA,
				dst_color_blendfactor = .SDL_GPU_BLENDFACTOR_ONE_MINUS_SRC_ALPHA,
				src_alpha_blendfactor = .SDL_GPU_BLENDFACTOR_SRC_ALPHA,
				dst_alpha_blendfactor = .SDL_GPU_BLENDFACTOR_ONE_MINUS_SRC_ALPHA,
			},
#warning fix this
			format =  0//(SDL_GPUTextureFormat)game.Graphics.PreferredFormat,
		};

		// Create raster state
		SDL_GPURasterizerState rasterState = SDL_GPURasterizerState
		{
			cull_mode = (SDL_GPUCullMode)cullMode,
			fill_mode = .SDL_GPU_FILLMODE_FILL,
			front_face = .SDL_GPU_FRONTFACE_COUNTER_CLOCKWISE,
		};

		// Create vertex buffer layout
		SDL_GPUVertexBufferDescription vertexBufferDescription = SDL_GPUVertexBufferDescription
		{
			slot = 0,
			input_rate = .SDL_GPU_VERTEXINPUTRATE_VERTEX,
			pitch = (uint32)Mesh.GetVertexSize(key.Elements),
		};


		// Create auto vertex attributes
		List<SDL_GPUVertexAttribute> attributes = scope .();
		CreateAutoVertexAttributes(key.Elements, attributes);

		// Create the pipeline
		SDL_GPUGraphicsPipelineCreateInfo pipelineInfo = SDL_GPUGraphicsPipelineCreateInfo
		{
			vertex_shader = gpuVertexShader,
			fragment_shader = gpuFragmentShader,
			primitive_type = (SDL_GPUPrimitiveType)primitiveType,
	
			// Attach color targets
			target_info = .
			{
				num_color_targets = 1,
				color_target_descriptions = &colorTargetDescription,
			},
			
			// Attach raster state
			rasterizer_state = rasterState,
			multisample_state = default,
			depth_stencil_state = default,
	
			// Attach vertex input state
			vertex_input_state = .
			{
				num_vertex_buffers = 1,
				vertex_buffer_descriptions = &vertexBufferDescription,

				// Attach vertex attributes
				num_vertex_attributes = (uint32)attributes.Count,
				vertex_attributes = attributes.Ptr,
			},

			props = 0,
		};
		
		// Finally we can create the pipeline
		return SDL_CreateGPUGraphicsPipeline(gpuDevice, &pipelineInfo);
	}

	private static void CreateAutoVertexAttributes(MeshVertexElements elements, List<SDL_GPUVertexAttribute> outAttributes)
	{
		uint32 location = 0;
		uint32 offset = 0;

		// Create position2 attribute
		if((elements & .Position2) != 0)
		{
			// Add position attribute
			outAttributes.Add(SDL_GPUVertexAttribute
			{
			   	location = location,
				format = .SDL_GPU_VERTEXELEMENTFORMAT_FLOAT2,
				offset = offset,
			});

			// Update location and offset
			location++;
			offset += sizeof(float2);
		}

		// Create position3 attribute
		if((elements & .Position3) != 0)
		{
			// Add position attribute
			outAttributes.Add(SDL_GPUVertexAttribute
			{
			   	location = location,
				format = .SDL_GPU_VERTEXELEMENTFORMAT_FLOAT3,
				offset = offset,
			});

			// Update location and offset
			location++;
			offset += sizeof(float3);
		}

		// Create normal attribute
		if((elements & .Normal) != 0)
		{
			// Add normal attribute
			outAttributes.Add(SDL_GPUVertexAttribute
			{
			   	location = location,
				format = .SDL_GPU_VERTEXELEMENTFORMAT_FLOAT3,
				offset = offset,
			});

			// Update location and offset
			location++;
			offset += sizeof(float3);
		}

		// Create uv attribute
		if((elements & .UV) != 0)
		{
			// Add position attribute
			outAttributes.Add(SDL_GPUVertexAttribute
			{
			   	location = location,
				format = .SDL_GPU_VERTEXELEMENTFORMAT_FLOAT2,
				offset = offset,
			});

			// Update location and offset
			location++;
			offset += sizeof(float2);
		}

		// Create color attribute
		if((elements & .Color) != 0)
		{
			// Add position attribute
			outAttributes.Add(SDL_GPUVertexAttribute
			{
			   	location = location,
				format = .SDL_GPU_VERTEXELEMENTFORMAT_FLOAT4,
				offset = offset,
			});

			// Update location and offset
			location++;
			offset += sizeof(Color);
		}

		// Create color32 attribute
		if((elements & .Color32) != 0)
		{
			// Add position attribute
			outAttributes.Add(SDL_GPUVertexAttribute
			{
			   	location = location,
				format = .SDL_GPU_VERTEXELEMENTFORMAT_UBYTE4_NORM,
				offset = offset,
			});

			// Update location and offset
			location++;
			offset += sizeof(Color32);
		}
	}
}