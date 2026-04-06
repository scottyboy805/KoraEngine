
using System.Runtime.Serialization;

namespace KoraGame.Graphics
{
    [EditorIcon("Icon/Camera.png")]
    public sealed class Camera : Component
    {
        // Private
        [DataMember(Name = "Clear Color")]
        private Color clearColor = Color.CornflowerBlue;
        [DataMember(Name = "Field Of View")]
        [EditorRange(1f, 180f)]
        private float fieldOfView = 60f;
        [DataMember(Name = "Near Plane")]
        [EditorMin(0.01f)]
        private float nearPlane = 0.01f;
        [DataMember(Name = "Far Plane")]
        private float farPlane = 1000f;

        private readonly GraphicsBatch renderBatch = new(256);

        // Properties
        public GraphicsDevice Graphics => Game?.Graphics;

        public Color ClearColor
        {
            get => clearColor;
            set
            {
                clearColor = value;
            }
        }

        public float FieldOfView
        {
            get => fieldOfView;
            set
            {
                fieldOfView = value;
            }
        }

        public float NearPlane
        {
            get => nearPlane;
            set
            {
                nearPlane = value;
            }
        }

        public float FarPlane
        {
            get => farPlane;
            set
            {
                farPlane = value;
            }
        }

        // Methods
        internal override void RegisterSubSystems()
        {
            Scene?.activeCameras.Add(this);
        }

        internal override void UnregisterSubSystems()
        {
            Scene?.activeCameras.Remove(this);
        }

        public Matrix4F GetProjectionMatrix(float aspect)
        {
            // Get projection
            return Matrix4F.Perspective(fieldOfView, aspect, nearPlane, farPlane);
        }

        public void Render(Texture renderTexture = null, Matrix4F? viewMatrix = null, Matrix4F? projectionMatrix = null)
        {
            // Get command buffer
            GraphicsCommand cmd = Graphics.AcquireCommandBuffer();

            // Begin rendering
            cmd.BeginRenderPass(clearColor, renderTexture);
            {
                // Render the camera perspective
                Render(cmd, viewMatrix, projectionMatrix);
            }
            // End rendering
            cmd.EndRenderPass();

            // Submit the command buffer
            cmd.Submit();
        }

        public void Render(GraphicsCommand renderPass, Matrix4F? viewMatrix = null, Matrix4F? projectionMatrix = null)
        {
            // Get the aspect
            float aspect = renderPass.RenderWidth / (float)renderPass.RenderHeight;

            // Create view matrix
            // IMPORTANT - Use WorldToLocal as the inverse for camera
            Matrix4F view = viewMatrix == null
                ? GameObject != null ? GameObject.WorldToLocalMatrix : Matrix4F.Identity
                : projectionMatrix.Value;

            // Create projection matrix
            Matrix4F projection = projectionMatrix == null
                ? GetProjectionMatrix(aspect)
                : projectionMatrix.Value;

            // Begin batch
            renderBatch.Begin(renderPass, view, projection);
            {
                // Render the scene
                Scene?.Draw(renderBatch);
            }
            renderBatch.End();
        }
    }
}
