using System.Runtime.Serialization;

namespace KoraGame.UI
{
    public class UIButton : UIImage
    {
        // Events
        [DataMember]
        public GameEvent OnClicked = new GameEvent();

        // Private
        [DataMember(Name = "HighlightColor")]
        private Color highlightColor = new Color(0.95f, 0.95f, 0.95f, 1f);
        [DataMember(Name = "PressedColor")]
        private Color pressedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        [DataMember(Name = "InactiveColor")]
        private Color inactiveColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        [DataMember(Name = "Interactable")]
        private bool interactable = true;

        private bool isPointerOver = false;
        private bool isPressed = false;

        // Properties
        public Color HighlightColor
        {
            get => highlightColor;
            set { highlightColor = value; }
        }

        public Color PressedColor
        {
            get => pressedColor;
            set { pressedColor = value; }
        }

        public Color InactiveColor
        {
            get => inactiveColor;
            set { inactiveColor = value; }
        }

        public bool Interactable
        {
            get => interactable;
            set { interactable = value; }
        }

        // Methods
        protected override void OnPointerEnter()
        {
            isPointerOver = true;
        }

        protected override void OnPointerExit()
        {
            isPointerOver = false;
        }

        protected override void OnPointerDown()
        {
            isPressed = true;
        }

        protected override void OnPointerUp()
        {
            Perform();
            isPressed = false;            
        }

        public virtual void Perform()
        {
            // Trigger event
            if (interactable == true)
                OnClicked?.Raise();
        }
    }
}
