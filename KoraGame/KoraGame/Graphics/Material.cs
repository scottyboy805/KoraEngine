using System.Runtime.Serialization;

namespace KoraGame.Graphics
{
    public sealed class Material : GameElement, IAssetSerialize
    {
        // Type
        private struct TextureSlot
        {
            // Public
            public string Name;
            public Texture Texture;
        }

        // Private
        [DataMember(Name = "Shader")]
        private Shader shader;
        [DataMember(Name = "Textures")]
        private List<TextureSlot> textures = new();

        // Public
        public const string MainColorName = "Color";
        public const string MainTextureName = "Texture";        

        // Properties
        public GraphicsDevice Graphics => Game?.GraphicsDevice;

        public Shader Shader
        {
            get => shader;
            set
            {
                shader = value;
                UpdateMaterialSlots();
            }
        }

        //public Color MainColor
        //{
        //    get => Get
        //}

        public Texture MainTexture
        {
            get => GetTexture(MainTextureName);
            set
            {
                try
                {
                    SetTexture(MainTextureName, value);
                }
                catch { }
            }
        }

        // Methods
        void IAssetSerialize.OnSerialize() { }
        void IAssetSerialize.OnDeserialize()
        {
            UpdateMaterialSlots();
        }

        protected override void OnDestroy()
        {
            shader = null;
            textures = null;
        }

        internal override void CloneInstantiate(GameElement element)
        {
            base.CloneInstantiate(element);

            // Get the material
            Material clone = (Material)element;

            // Copy the shader
            clone.shader = shader;

            // Copy the slots
            clone.textures = textures.ToList();
        }

        public void Bind(GraphicsCommand command, MeshVertexElements elements)
        {
            // Bind the shader
            if (shader != null)
                command.BindShader(shader, elements);

            // Process all properties
            foreach(ShaderProperty property in shader.Properties)
            {
                switch(property.Type)
                {
                    case ShaderPropertyType.Texture:
                        {
                            // Try to find the slot
                            TextureSlot slot = textures.FirstOrDefault(t => t.Name == property.Name);

                            // Get the texture
                            Texture bindTexture = slot.Texture != null
                                ? slot.Texture
                                : Graphics.WhiteTexture;

                            // Bind the texture to the property slot
                            command.BindTexture(bindTexture, property.Location);
                            break;
                        }
                }
            }
        }

        public void SetTexture(string name, Texture texture)
        {
            // Check for shader
            if (shader == null)
                throw new InvalidOperationException("Material has no shader assigned");

            // Try to find the slot
            ShaderProperty property = shader.Properties
                .FirstOrDefault(p => p.Name == name);

            // Check for found
            if (property.Name == null || property.Type != ShaderPropertyType.Texture)
                throw new InvalidOperationException("Could not find a shader texture slot named: " + name);

            // Create the slot
            TextureSlot slot = new TextureSlot
            {
                Name = name,
                Texture = texture,
            };

            // Add to textures
            int index = textures.FindIndex(t => t.Name == name);

            // Add or update the slot
            if(index != -1)
            {
                textures[index] = slot;
            }
            else
            {
                textures.Add(slot);
            }
        }

        public Texture GetTexture(string name)
        {
            // Try to find the texture or null
            return textures.FirstOrDefault(t => t.Name == name)
                .Texture;
        }

        private void UpdateMaterialSlots()
        {
            // Check for shader
            if (shader == null)
                return;

            // Process all properties
            foreach(ShaderProperty property in shader.Properties)
            {
                switch(property.Type)
                {
                    case ShaderPropertyType.Texture:
                        {
                            // Add an empty slot
                            if(textures.Any(t => t.Name == property.Name) == false)
                                textures.Add(new TextureSlot { Name = property.Name });
                            break;
                        }
                }
            }

            // Remove unused
            textures.RemoveAll(t => shader.Properties.Any(p => p.Name == t.Name) == false);
        }
    }
}
