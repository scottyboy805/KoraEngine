using System;
using System.Collections;
using KoraPlayer.Graphics;

namespace KoraPlayer.Assets;

internal enum AssetLoadError
{
	FileNotFound,
	IOError,
	NotEnoughData,
	InvalidFormat,
	InvalidHeader,
	InvalidVersion,
	InvalidData,
}

internal struct AssetLoadContext
{
	// Private
	private readonly GraphicsDevice graphics;
	private GraphicsCommand graphicsCmd;

	private readonly Type assetType;
	private readonly StringView assetName;
	private readonly StringView assetExtension;
	private readonly int32 dependencyDepth;

	// Properties
	public GraphicsDevice Graphics => graphics;
	public GraphicsCommand GraphicsCmd => graphicsCmd;

	public Type AssetType => assetType;
	public StringView AssetName => assetName;
	public StringView AssetExtension => assetExtension;
	public int32 DependencyDepth => dependencyDepth;
	public bool IsDependency => dependencyDepth > 0;

	// Constructor
	internal this(GraphicsDevice graphics, Type assetType, StringView assetName, StringView assetExtension, AssetLoadContext? parent = null)
	{
		this.graphics = graphics;
		this.assetType = assetType;
		this.assetName = assetName;
		this.assetExtension = assetExtension;
		this.dependencyDepth = parent != null ? parent.Value.dependencyDepth + 1 : 0;

		if(parent != null)
		{
			this.graphicsCmd = parent.Value.graphicsCmd;
		}
		else
		{
			this.graphicsCmd = graphics.AcquireCommandBuffer();
			this.graphicsCmd.BeginCopyPass();
		}
	}

	// Methods
	internal void SubmitCmd() mut
	{
		// Only root context can submit the command buffer
		if(IsDependency == true)
			return;

		// End phase
		graphicsCmd.EndCopyPass();

		// Submit buffer
		graphicsCmd.Submit();
	}
}

