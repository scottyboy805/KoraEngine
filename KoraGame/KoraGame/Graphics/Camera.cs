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

        private readonly GraphicsBatch graphicsBatch = new(256);

        // Properties
        public GraphicsProvider Graphics => Game?.Graphics;

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
        protected override void OnEnable()
        {
            Scene?.activeCameras.Add(this);
        }

        protected override void OnDisable()
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
            // Get graphics
            GraphicsProvider graphics = Graphics;

            // Begin rendering
            graphics.BeginRenderPass(clearColor, renderTexture);
            {
                // Render the camera perspective
                Render(graphics, viewMatrix, projectionMatrix);
            }
            // End rendering
            graphics.EndRenderPass();

            // Submit the command buffer
            graphics.Submit();
        }

        public void Render(GraphicsProvider graphics, Matrix4F? viewMatrix = null, Matrix4F? projectionMatrix = null)
        {
            // Get the aspect
            float aspect = graphics.RenderWidth / (float)graphics.RenderHeight;

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
            graphicsBatch.Begin(graphics, view, projection);
            {
                // Render the scene
                Scene?.Draw(graphicsBatch);
            }
            graphicsBatch.End();
        }
    }
}
