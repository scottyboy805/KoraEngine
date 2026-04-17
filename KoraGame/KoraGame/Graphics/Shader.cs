using SDL;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Text;

namespace KoraGame.Graphics
{
    [Flags]
    public enum ShaderFormat : uint
    {
        Spirv = (1u << 1),
        DXBC = (1u << 2),
        DXIL = (1u << 3),
        MSL = (1u << 4),
        MetalLib = (1u << 5),
    }

    public enum ShaderStage : int
    {
        Vertex,
        Fragment,
    }

    public enum ShaderCullMode : int
    {
        None = 0,
        Front,
        Back,
    }

    public enum ShaderPropertyType
    {
        Color,
        Matrix4,
        Texture,
    }

#pragma warning disable 0649    // Fields are set via serialization only
    [Serializable]
    public struct ShaderProperty
    {
        // Private
        [DataMember(Name = "Name")]
        private string name;
        [DataMember(Name = "Location")]
        private uint location;
        [DataMember(Name = "Stage")]
        private ShaderStage stage;
        [DataMember(Name = "Type")]
        private ShaderPropertyType type;

        // Properties
        internal string Name => name;
        public uint Location => location;
        public ShaderStage Stage => stage;
        public ShaderPropertyType Type => type;
    }
#pragma warning restore 0649

    public unsafe sealed class Shader : GameElement, IAssetSerialize
    {
        // Type
        private struct ShaderPipelineKey
        {
            // Public
            public MeshVertexElements Elements;

            // Methods
            public override bool Equals([NotNullWhen(true)] object obj)
            {
                if (obj is ShaderPipelineKey key)
                    return key.Elements == Elements;

                return false;
            }

            public override int GetHashCode()
            {
                return Elements.GetHashCode();
            }
        }

#pragma warning disable 0649    // Fields are set via serialization only
        [Serializable]
        private struct ShaderSource
        {
            // Public
            [DataMember]
            public RawAsset Source;
            [DataMember]
            public string EntryPoint;
            [DataMember]
            public uint UniformCount;
            [DataMember]
            public uint SamplerCount;
        }

        // Private
        [DataMember(Name = "Format")]
        private ShaderFormat format = ShaderFormat.Spirv;
        [DataMember(Name = "CullMode")]
        private ShaderCullMode cullMode = ShaderCullMode.Back;
        [DataMember(Name = "Vertex")]
        private ShaderSource vertexSource;
        [DataMember(Name = "Fragment")]
        private ShaderSource fragmentSource;
        [DataMember(Name = "Properties")]
        private List<ShaderProperty> properties = new();
#pragma warning restore 0649

        private GraphicsDevice device;        
        private Dictionary<ShaderPipelineKey, IntPtr> pipelines = new();

        // Public
        public const string DefaultEntryPoint = "main";

        // Internal
        internal SDL_GPUShader* gpuVertexShader;
        internal SDL_GPUShader* gpuFragmentShader;

        // Properties
        public ShaderFormat Format => format;
        public ShaderCullMode CullMode => cullMode;
        public IList<ShaderProperty> Properties => properties;

        // Constructor
        private Shader()
        {
            // Get device
            this.device = Game.Instance?.GraphicsDevice;
        }

        public Shader(GraphicsDevice device, byte[] vertexSource, byte[] fragmentSource, ShaderFormat format, string entryPoint = "main")
        {
            // Check for null
            if(device == null)
                throw new ArgumentNullException(nameof(device));

            this.device = device;
            this.format = format;

            // Initialize shaders
            this.gpuVertexShader = InitializeShader(vertexSource, format, ShaderStage.Vertex, entryPoint, 1, 0);
            this.gpuFragmentShader = InitializeShader(fragmentSource, format, ShaderStage.Fragment, entryPoint, 0, 1);
        }

        ~Shader()
        {
            OnDestroy();
        }

        // Methods
        protected override void OnDestroy()
        {
            if(device != null)
            {
                // Destroy pipelines
                foreach (IntPtr pipeline in pipelines.Values)
                    SDL3.SDL_ReleaseGPUGraphicsPipeline(device.gpuDevice, (SDL_GPUGraphicsPipeline*)pipeline);

                // Destroy shaders
                SDL3.SDL_ReleaseGPUShader(device.gpuDevice, gpuVertexShader);
                SDL3.SDL_ReleaseGPUShader(device.gpuDevice, gpuFragmentShader);

                device = null;
                gpuVertexShader = null;
                gpuFragmentShader = null;

                format = 0;
                cullMode = 0;
                vertexSource = default;
                fragmentSource = default;
                properties = null;
            }
        }

        void IAssetSerialize.OnSerialize() { }
        void IAssetSerialize.OnDeserialize()
        {
            gpuVertexShader = InitializeShader(vertexSource, ShaderStage.Vertex);
            gpuFragmentShader = InitializeShader(fragmentSource, ShaderStage.Fragment);
        }

        internal SDL_GPUGraphicsPipeline* GetOrCreatePipeline(MeshVertexElements elements)
        {
            // Create the key
            ShaderPipelineKey key = new ShaderPipelineKey
            {
                Elements = elements
            };

            // Try to lookup
            if (pipelines.TryGetValue(key, out IntPtr pipeline) == false)
            {
                // Create and cache the variant
                pipeline = (IntPtr)InitializePipeline(key);

                // Add to cache
                pipelines[key] = pipeline;
            }

            // Get the gpu pipeline
            return (SDL_GPUGraphicsPipeline*)pipeline;
        }

        private SDL_GPUShader* InitializeShader(byte[] source, ShaderFormat format, ShaderStage stage, string entryPoint, uint uniformBuffers, uint samplers)
        {
            // Get the array for entry point
            byte[] entryPointBytes = Encoding.UTF8.GetBytes(entryPoint);

            // Pin the source
            fixed (byte* sourcePtr = source)
            {
                fixed (byte* entryPointPtr = entryPointBytes)
                {
                    // Setup vertex shader
                    SDL_GPUShaderCreateInfo vertexInfo = new SDL_GPUShaderCreateInfo
                    {
                        code = sourcePtr,
                        code_size = (uint)source.Length,
                        entrypoint = entryPointPtr,
                        format = (SDL_GPUShaderFormat)format,
                        stage = (SDL_GPUShaderStage)stage,
                        num_uniform_buffers = uniformBuffers,
                        num_samplers = samplers,
                    };

                    // Create the shader
                    SDL_GPUShader* shader = SDL3.SDL_CreateGPUShader(device.gpuDevice, &vertexInfo);

                    // Check for error
                    if (shader == null)
                        throw new Exception("Error initializing shader: " + SDL3.SDL_GetError());

                    return shader;
                }
            }
        }

        private SDL_GPUShader* InitializeShader(ShaderSource shaderSource, ShaderStage stage)
        {
            // Check for device
            if (device == null)
                return null;

            // Get entry point
            string entryPoint = string.IsNullOrEmpty(shaderSource.EntryPoint) == false
                ? shaderSource.EntryPoint
                : DefaultEntryPoint;

            // Get the array for entry point
            byte[] entryPointBytes = Encoding.UTF8.GetBytes(entryPoint);

            // Pin the source
            fixed (byte* sourcePtr = shaderSource.Source.GetBytes())
            {
                fixed (byte* entryPointPtr = entryPointBytes)
                {
                    // Setup vertex shader
                    SDL_GPUShaderCreateInfo vertexInfo = new SDL_GPUShaderCreateInfo
                    {
                        code = sourcePtr,
                        code_size = shaderSource.Source.Length,
                        entrypoint = entryPointPtr,
                        format = (SDL_GPUShaderFormat)format,
                        stage = (SDL_GPUShaderStage)stage,
                        num_uniform_buffers = shaderSource.UniformCount,
                        num_samplers = shaderSource.SamplerCount,
                    };

                    // Create the shader
                    SDL_GPUShader* shader = SDL3.SDL_CreateGPUShader(device.gpuDevice, &vertexInfo);

                    // Check for error
                    if (shader == null)
                        throw new Exception("Error initializing shader: " + SDL3.SDL_GetError());

                    return shader;
                }
            }
        }

        private SDL_GPUGraphicsPipeline* InitializePipeline(in ShaderPipelineKey key)
        {
            // Create color target
            SDL_GPUColorTargetDescription colorTargetInfo = new SDL_GPUColorTargetDescription
            {
                blend_state = new SDL_GPUColorTargetBlendState
                {
                    enable_blend = true,
                    color_blend_op = SDL_GPUBlendOp.SDL_GPU_BLENDOP_ADD,
                    alpha_blend_op = SDL_GPUBlendOp.SDL_GPU_BLENDOP_ADD,
                    src_color_blendfactor = SDL_GPUBlendFactor.SDL_GPU_BLENDFACTOR_SRC_ALPHA,
                    dst_color_blendfactor = SDL_GPUBlendFactor.SDL_GPU_BLENDFACTOR_ONE_MINUS_SRC_ALPHA,
                    src_alpha_blendfactor = SDL_GPUBlendFactor.SDL_GPU_BLENDFACTOR_SRC_ALPHA,
                    dst_alpha_blendfactor = SDL_GPUBlendFactor.SDL_GPU_BLENDFACTOR_ONE_MINUS_SRC_ALPHA,
                },
                format = (SDL_GPUTextureFormat.SDL_GPU_TEXTUREFORMAT_B8G8R8A8_UNORM),
            };

            // Create raster state
            SDL_GPURasterizerState rasterState = new SDL_GPURasterizerState
            {
                // Disable face culling by default to avoid mismatches with imported vertex winding (UI/ImGui expects no culling)
                cull_mode = SDL_GPUCullMode.SDL_GPU_CULLMODE_BACK,
                fill_mode = SDL_GPUFillMode.SDL_GPU_FILLMODE_FILL,
                front_face = SDL_GPUFrontFace.SDL_GPU_FRONTFACE_CLOCKWISE,
            };

            // Create vertex buffer layout
            SDL_GPUVertexBufferDescription vertexBufferInfo = new SDL_GPUVertexBufferDescription
            {
                slot = 0,
                input_rate = SDL_GPUVertexInputRate.SDL_GPU_VERTEXINPUTRATE_VERTEX,
                pitch = Mesh.GetVertexSize(key.Elements),
            };


            // Create auto vertex attributes
            SDL_GPUVertexAttribute[] vertexAttributes = CreateAutoVertexAttributes(key.Elements);


            // Pin the attributes
            fixed (SDL_GPUVertexAttribute* attributes = vertexAttributes)
            {
                // Create the pipeline
                SDL_GPUGraphicsPipelineCreateInfo pipelineInfo = new SDL_GPUGraphicsPipelineCreateInfo
                {
                    vertex_shader = gpuVertexShader,
                    fragment_shader = gpuFragmentShader,
                    primitive_type = SDL_GPUPrimitiveType.SDL_GPU_PRIMITIVETYPE_TRIANGLELIST,

                    // Attach color targets
                    target_info = new SDL_GPUGraphicsPipelineTargetInfo
                    {
                        num_color_targets = 1,
                        color_target_descriptions = &colorTargetInfo,
                        depth_stencil_format = SDL_GPUTextureFormat.SDL_GPU_TEXTUREFORMAT_D32_FLOAT,
                        has_depth_stencil_target = true,
                    },

                    // Attach depth target - default to disabled for compatibility with 2D/UI shaders
                    depth_stencil_state = new SDL_GPUDepthStencilState
                    {
                        enable_depth_test = false,
                        enable_depth_write = false,
                        compare_op = SDL_GPUCompareOp.SDL_GPU_COMPAREOP_LESS,
                        enable_stencil_test = false,
                    },

                    // Attach raster state
                    rasterizer_state = rasterState,

                    // Attach vertex input state
                    vertex_input_state = new SDL_GPUVertexInputState
                    {
                        num_vertex_buffers = 1,
                        vertex_buffer_descriptions = &vertexBufferInfo,

                        num_vertex_attributes = (uint)vertexAttributes.Length,
                        vertex_attributes = attributes,
                    },
                };

                // Create the pipeline
                SDL_GPUGraphicsPipeline* pipeline = SDL3.SDL_CreateGPUGraphicsPipeline(device.gpuDevice, &pipelineInfo);

                // Check for error
                if(pipeline == null)
                    throw new Exception("Error initializing pipeline: " + SDL3.SDL_GetError());

                return pipeline;
            }
        }

        private static unsafe SDL_GPUVertexAttribute[] CreateAutoVertexAttributes(MeshVertexElements elements)
        {
            // Get element count
            uint elementCount = Mesh.GetVertexElementCount(elements);

            // Allocate array
            SDL_GPUVertexAttribute[] attributes = new SDL_GPUVertexAttribute[elementCount];

            uint location = 0;
            uint offset = 0;

            // Create position2 attribute
            if ((elements & MeshVertexElements.Position2) != 0)
            {
                // Add position
                attributes[location] = new SDL_GPUVertexAttribute
                {
                    location = location,
                    format = SDL_GPUVertexElementFormat.SDL_GPU_VERTEXELEMENTFORMAT_FLOAT2,
                    offset = offset,
                };

                // Update location and offset and index
                location++;
                offset += (uint)sizeof(Vector2F);
            }

            // Create position3 attribute
            if((elements & MeshVertexElements.Position3) != 0)
            {
                // Add position
                attributes[location] = new SDL_GPUVertexAttribute
                {
                    location = location,
                    format = SDL_GPUVertexElementFormat.SDL_GPU_VERTEXELEMENTFORMAT_FLOAT3,
                    offset = offset,
                };

                // Update location and offset and index
                location++;
                offset += (uint)sizeof(Vector3F);
            }

            // Create normal attribute
            if ((elements & MeshVertexElements.Normal) != 0)
            {
                // Add normal attribute
                attributes[location] = new SDL_GPUVertexAttribute    
                {
                    location = location,
				    format = SDL_GPUVertexElementFormat.SDL_GPU_VERTEXELEMENTFORMAT_FLOAT3,
				    offset = offset,
			    };

                // Update location and offset
                location++;
                offset += (uint)sizeof(Vector3F);
            }

            // Create uv attribute
            if ((elements & MeshVertexElements.UV) != 0)
            {
                // Add uv attribute
                attributes[location] = new SDL_GPUVertexAttribute
                {
                    location = location,
				    format = SDL_GPUVertexElementFormat.SDL_GPU_VERTEXELEMENTFORMAT_FLOAT2,
				    offset = offset,
			    };

                // Update location and offset
                location++;
                offset += (uint)sizeof(Vector2F);
            }

            // Create color attribute
            if ((elements & MeshVertexElements.Color) != 0)
            {
                // Add color attribute
                attributes[location] = new SDL_GPUVertexAttribute
                {
                    location = location,
				    format = SDL_GPUVertexElementFormat.SDL_GPU_VERTEXELEMENTFORMAT_FLOAT4,
				    offset = offset,
			    };

                // Update location and offset
                location++;
                offset += (uint)sizeof(Color);
            }

            // Create color32 attribute
            if ((elements & MeshVertexElements.Color32) != 0)
            {
                // Add color attribute
                attributes[location] = new SDL_GPUVertexAttribute
                {
                    location = location,
                    format = SDL_GPUVertexElementFormat.SDL_GPU_VERTEXELEMENTFORMAT_UBYTE4_NORM,
                    offset = offset,
                };

                // Update location and offset
                location++;
                offset += (uint)sizeof(Color32);
            }

            return attributes;
        }
    }
}
