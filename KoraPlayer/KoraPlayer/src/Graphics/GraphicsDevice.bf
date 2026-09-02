using SDL3;
using System;
using internal KoraPlayer;
using internal KoraPlayer.Graphics;

namespace KoraPlayer.Graphics;

public class GraphicsDevice
{
	// Private
	//private Screen renderTarget;
	private TextureFormat preferredFormat = .B8G8R8A8_UNORM;

	// Internal
	internal SDL_GPUDevice* gpuDevice;

	// Properties
	public TextureFormat PreferredFormat => preferredFormat;

	// Constructor
	internal this()//Screen renderTarget = null)
	{
		//this.renderTarget = renderTarget;

		// Create the device
		gpuDevice = SDL_CreateGPUDevice(.SDL_GPU_SHADERFORMAT_SPIRV | .SDL_GPU_SHADERFORMAT_MSL, true, null);

		// Attach the device
		//if(renderTarget != null)
		{
			// Claim the window
			//SDL_ClaimWindowForGPUDevice(gpuDevice, renderTarget.window);
			
			// Get the preferred format
			//this.preferredFormat = (TextureFormat)SDL_GetGPUSwapchainTextureFormat(gpuDevice, renderTarget.window);
		}
	}

	internal ~this()
	{
		// Destroy the device
		SDL_DestroyGPUDevice(gpuDevice);
		gpuDevice = null;
	}

	// Methods
	public GraphicsCommand AcquireCommandBuffer()
	{
		// Create the command buffer
		SDL_GPUCommandBuffer* gpuCommandBuffer = SDL_AcquireGPUCommandBuffer(gpuDevice);
		
		// Create the command buffer
		return GraphicsCommand(gpuDevice, gpuCommandBuffer);
	}

	public void GetDeviceDriverName(String outName)
	{
		// Get the name
		char8* cstr = SDL_GetGPUDeviceDriver(gpuDevice);

		// Write to output string
		outName.Set(scope .(cstr));

		// Free name
		SDL_free(cstr);
	}
}