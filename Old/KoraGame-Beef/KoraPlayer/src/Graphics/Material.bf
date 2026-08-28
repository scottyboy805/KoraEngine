using SDL3;
using System;
using internal KoraPlayer.Graphics;

namespace KoraPlayer.Graphics;

public sealed class Material : GameElement
{
	// Private
	private Shader shader;
	private int renderQueue;

	public Texture texture;

	// Properties
	public Shader Shader
	{
		get => shader;
		set
		{
			shader = value;
		}
	}

	public int RenderQueue
	{
		get => renderQueue;
		set
		{
			renderQueue = Math.Clamp(value, -5000, 5000);
		}
	}

	// Methods
	protected override GameElement OnInstantiate()
	{
		
		return default;
	}

	public void Bind(GraphicsCommand cmd, MeshPrimitiveType primitiveType, MeshVertexElements elements)
	{
		// Bind the shader
		if(shader != null)
			cmd.BindShader(shader, primitiveType, elements);

		// Process all properties
		for(let property in shader.Properties)
		{
			switch(property.Type)
			{
			case .Texture:
				{
					cmd.BindTexture(texture, property.Location);
				}
			default:
			}
		}
	}

	public void BindTexture(GraphicsCommand cmd, Texture texture, uint location)
	{
		// Create the binding
		SDL_GPUTextureSamplerBinding binding = SDL_GPUTextureSamplerBinding();
		binding.texture = texture.gpuTexture;
		binding.sampler = texture.gpuSampler;

		// Attach to shader
		SDL_BindGPUFragmentSamplers(cmd.gpuRenderPass, (uint32)location, &binding, 1);
	}
}