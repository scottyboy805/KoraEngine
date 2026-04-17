using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace KoraGame.Graphics
{
    public enum LightKind
    {
        Directional,
        Point,
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct LightData
    {
        public Vector4F PosisionType;   // XYZ position, W type (0 = directional, 1 = point)
        public Color ColorIntensity;    // RGB color, A intensity
    }

    public sealed class Light : Component
    {
        // Properties
        [DataMember]
        public LightKind Kind { get; set; } = LightKind.Directional;
        [DataMember]
        public Color Color { get; set; } = Color.White;
        [DataMember]
        public float Intensity { get; set; } = 1f;

        // Methods
        internal override void RegisterSubSystems()
        {
            if (Scene != null)
            {
                Scene.activeLights.Add(this);
                Scene.lightBuffer = null;
            }
        }

        internal override void UnregisterSubSystems()
        {
            if (Scene != null)
            {
                Scene.activeLights.Remove(this);
                Scene.lightBuffer = null;
            }
        }

        internal static unsafe GraphicsBuffer RebuildLightBuffer(Scene scene)
        {
            // Get light count
            List<Light> activeLights = scene.activeLights;
            uint lightCount = (uint)activeLights.Count;

            // Recreate buffer
            GraphicsBuffer lightBuffer = new GraphicsBuffer(scene.Game.GraphicsDevice, GraphicsBufferUsage.GraphicsRead, (uint)sizeof(LightData) * lightCount);

            // Fill light data
            lightBuffer.MapMemory((bufferPtr) =>
            {
                LightData* lightDataArray = (LightData*)bufferPtr;
                for (int i = 0; i < activeLights.Count; i++)
                {
                    Light light = activeLights[i];

                    // Get light position
                    Vector4F lightPosition = (Vector4F)light.GameObject.WorldPosition;
                    lightPosition.W = light.Kind == LightKind.Directional ? 0f : 1f; // Set W to indicate light type

                    // Get light color
                    Color lightColor = light.Color;
                    lightColor.A = light.Intensity; // Use alpha channel to store intensity

                    lightDataArray[i] = new LightData
                    {
                        PosisionType = lightPosition,
                        ColorIntensity = lightColor
                    };
                }
            });

            // Get buffer
            return lightBuffer;
        }
    }
}
