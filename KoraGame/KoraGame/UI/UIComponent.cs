using KoraGame.Graphics;
using KoraGame.Input;
using System.Runtime.Serialization;

namespace KoraGame.UI
{
    public enum UIAnchor
    {
        Center,
        TopLeft,
        TopRight,
        TopCenter,
        BottomLeft,
        BottomRight,
        BottomCenter,
        CenterLeft,
        CenterRight,
    };


    public abstract class UIComponent : Component
    {
        // Private
        [DataMember(Name = "PointerTarget")]
        private bool pointerTarget = true;
        [DataMember(Name = "Size")]
        private Vector2F size = new Vector2F(0.5f, 0.5f);
        [DataMember(Name = "Pivot")]
        private Vector2F pivot = new Vector2F(0.5f, 0.5f);
        [DataMember(Name = "Anchor")]
        private UIAnchor anchor = UIAnchor.Center;

        private UICanvas canvas = null;

        // Properties
        public GraphicsDevice Graphics => Game?.Graphics;
        public InputProvider Input => Game?.Input;

        public bool PointerTarget
        {
            get => pointerTarget;
            set => pointerTarget = value;
        }

        public Vector2F Size
        {
            get { return size; }
            set { size = value; }
        }

        public Vector2F Pivot
        {
            get { return pivot; }
            set
            {
                pivot = value;
                pivot.X = Math.Clamp(pivot.X, 0f, 1f);
                pivot.Y = Math.Clamp(pivot.Y, 0f, 1f);
            }
        }

        public UIAnchor Anchor
        {
            get { return anchor; }
            set { anchor = value; }
        }

        public Vector2F Min
        {
            get
            {
                // Get world position
                Vector2F worldPos = GameObject.WorldPosition.XY;

                // Get scaled pivot
                Vector2F scaledPivot = size * pivot;

                // Get min position taking pivot into account
                return worldPos - scaledPivot;
            }
        }

        public Vector2F Max
        {
            get
            {
                // Get world position
                Vector2F worldPos = GameObject.WorldPosition.XY;

                // Get scaled pivot
                Vector2F scaledPivot = size * pivot;

                // Get min position taking pivot into account
                return worldPos + scaledPivot;
            }
        }

        // Methods
        protected virtual void OnPointerEnter() { }
        protected virtual void OnPointerExit() { }
        protected virtual void OnPointerDown() { }
        protected virtual void OnPointerUp() { }

        public virtual void Draw(GraphicsBatch renderBatch) { }

        internal override void RegisterSubSystems()
        {
            // Try to get the canvas
            canvas = GameObject.GetComponentInParent<UICanvas>();

            // Register with canvas
            if(canvas != null)
                canvas.activeUIComponents.Add(this);
        }

        internal override void UnregisterSubSystems()
        {
            // Unregister from canvas
            if(canvas != null)
                canvas.activeUIComponents.Remove(this);

            canvas = null;
        }

        public void GetBounds(out Vector2F min, out Vector2F max)
        {
            // Get world position
            Vector2F worldPos = GameObject.WorldPosition.XY;

            // Get scaled pivot
            Vector2F scaledPivot = size * pivot;

            // Get min and max
            min = worldPos - scaledPivot;
            max = worldPos + scaledPivot;
        }

        public bool Raycast(Vector2F canvasPoint)
        {
            // Check for raycast enabled
            if (pointerTarget == false)
                return false;

            // Get min and max bounds
            GetBounds(out Vector2F min, out Vector2F max);

            // Check bounds
            if(canvasPoint.X >= min.X && canvasPoint.X <= max.X
                && canvasPoint.Y >= min.Y && canvasPoint.Y <= max.Y)
            {
                // Point is inside the bounds
                return true;
            }
            return false;
        }

        internal void DoPointerEnterEvent()
        {
            try
            {
                OnPointerEnter();
            }
            catch(Exception e)
            {
                Debug.LogException(e);
            }
        }

        internal void DoPointerExitEvent()
        {
            try
            {
                OnPointerExit();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        internal void DoPointerDownEvent()
        {
            try
            {
                OnPointerDown();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        internal void DoPointerUpEvent()
        {
            try
            {
                OnPointerUp();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
