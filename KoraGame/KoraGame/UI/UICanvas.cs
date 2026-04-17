using KoraGame.Graphics;
using KoraGame.Input;
using System.Runtime.Serialization;

namespace KoraGame.UI
{
    public sealed class UICanvas : Renderer
    {
        // Internal
        internal readonly List<UIComponent> activeUIComponents = new();

        // Private
        [DataMember(Name = "Camera")]
        private Camera camera = null;
        [DataMember(Name = "AspectRatio")]
        private Vector2F aspectRatio = new Vector2F(16, 9);
        [DataMember(Name = "PixelsPerUnit")]
        private float pixelsPerUnit = 100f;
        
        private readonly List<UIComponent> overUIComponents = new();
        private UIComponent pressedTarget = null;
        private Matrix4F? canvasToScreenMatrix;
        private Matrix4F? screenToCanvasMatrix;

        // Properties
        public InputProvider Input => Game?.Input;

        public Camera Camera
        {
            get => camera;
            set
            {
                camera = value;
            }
        }

        public Vector2F AspectRatio
        {
            get => aspectRatio;
            set
            {
                aspectRatio = value;
            }
        }

        public float PixelsPerUnit
        {
            get => pixelsPerUnit;
            set
            {
                pixelsPerUnit = value;
            }
        }

        public Matrix4F CanvasToScreenMatrix
        {
            get
            {
                if (canvasToScreenMatrix == null)
                    canvasToScreenMatrix = Matrix4F.Scale(new Vector3F(aspectRatio.X, aspectRatio.Y, 1f));

                return canvasToScreenMatrix.Value;
            }
        }

        public Matrix4F ScreenToCanvasMatrix
        {
            get
            {
                if (screenToCanvasMatrix == null)
                    screenToCanvasMatrix = Matrix4F.Inverse(CanvasToScreenMatrix);

                return screenToCanvasMatrix.Value;
            }
        }

        // Methods
        internal override void RegisterSubSystems()
        {
            // Register mouse input
            Input.OnMouseMove.AddListener(DoPointerMoveEvent);
            Input.OnMouseDown.AddListener(OnMouseDownEvent);
            Input.OnMouseUp.AddListener(OnMouseUpEvent);
        }

        internal override void UnregisterSubSystems()
        {
            InputProvider input = Game.Input;

            // Unregister mouse input
            input.OnMouseMove.RemoveListener(DoPointerMoveEvent);
            input.OnMouseDown.RemoveListener(OnMouseDownEvent);
            input.OnMouseUp.RemoveListener(OnMouseUpEvent);
        }

        public override void Draw(GraphicsBatch graphics)
        {
            foreach(UIComponent component in activeUIComponents)
                component.Draw(graphics);
        }

        public void DoPointerMoveEvent(Vector2F pointerPosition)
        {
            bool didHitTarget = false;

            // Process all targets
            foreach (UIComponent target in activeUIComponents)
            {
                // Check for raycast
                bool hit = target.Raycast(pointerPosition);

                // Check for enter
                if (hit == true && didHitTarget == false && overUIComponents.Contains(target) == false)
                {
                    // Set hit flag - only send to the first hit target
                    didHitTarget = true;

                    // Add the target
                    overUIComponents.Add(target);

                    // Trigger event
                    target.DoPointerEnterEvent();
                }
                // Check for exit
                else if (hit == false && overUIComponents.Contains(target) == true)
                {
                    // Remove the target
                    overUIComponents.Remove(target);

                    // Trigger event
                    target.DoPointerExitEvent();
                }
            }
        }

        public void DoPointerDownEvent()
        {
            // Only apply to active targets
            foreach (UIComponent target in activeUIComponents)
            {
                // Check for raycast
                bool hit = target.Raycast(Input.MousePosition);

                // Check for hit
                if (hit == true)
                {
                    // Add the target
                    pressedTarget = target;

                    // Trigger event
                    target.DoPointerDownEvent();
                    break;
                }
            }
        }

        public void DoPointerUpEvent()
        {
            if(pressedTarget != null)
            {
                // Trigger event
                pressedTarget.DoPointerUpEvent();
                pressedTarget = null;
            }
        }

        private void OnMouseDownEvent(MouseButton button)
        {
            if (button == MouseButton.Left)
                DoPointerDownEvent();
        }

        private void OnMouseUpEvent(MouseButton button)
        {
            if (button == MouseButton.Left)
                DoPointerUpEvent();
        }
    }
}
