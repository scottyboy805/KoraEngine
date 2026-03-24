
namespace KoraGame.Input
{
    public enum Key : uint // Maps to SDL key code
    {
        ExtendedMask = 536870912u,
        ScancodeMask = 1073741824u,
        Unknown = 0u,
        Return = 13u,
        Escape = 27u,
        Backspace = 8u,
        Tab = 9u,
        Space = 32u,
        Exclaim = 33u,
        DoubleApostrophe = 34u,
        Hash = 35u,
        Dollar = 36u,
        Percent = 37u,
        Ampersand = 38u,
        Apostrophe = 39u,
        LeftParen = 40u,
        RightParen = 41u,
        Asterisk = 42u,
        Plus = 43u,
        Comma = 44u,
        Minus = 45u,
        Period = 46u,
        Slash = 47u,
        Num0 = 48u,
        Num1 = 49u,
        Num2 = 50u,
        Num3 = 51u,
        Num4 = 52u,
        Num5 = 53u,
        Num6 = 54u,
        Num7 = 55u,
        Num8 = 56u,
        Num9 = 57u,
        Colon = 58u,
        Semicolon = 59u,
        Less = 60u,
        Equals = 61u,
        Greater = 62u,
        Question = 63u,
        At = 64u,
        LeftBracket = 91u,
        Backslash = 92u,
        RightBracket = 93u,
        Caret = 94u,
        Underscore = 95u,
        Grave = 96u,
        A = 97u,
        B = 98u,
        C = 99u,
        D = 100u,
        E = 101u,
        F = 102u,
        G = 103u,
        H = 104u,
        I = 105u,
        J = 106u,
        K = 107u,
        L = 108u,
        M = 109u,
        N = 110u,
        O = 111u,
        P = 112u,
        Q = 113u,
        R = 114u,
        S = 115u,
        T = 116u,
        U = 117u,
        V = 118u,
        W = 119u,
        X = 120u,
        Y = 121u,
        Z = 122u,
        LeftBrace = 123u,
        Pipe = 124u,
        RightBrace = 125u,
        Tilde = 126u,
        Delete = 127u,
        PlusMinus = 177u,
        CapsLock = 1073741881u,
        F1 = 1073741882u,
        F2 = 1073741883u,
        F3 = 1073741884u,
        F4 = 1073741885u,
        F5 = 1073741886u,
        F6 = 1073741887u,
        F7 = 1073741888u,
        F8 = 1073741889u,
        F9 = 1073741890u,
        F10 = 1073741891u,
        F11 = 1073741892u,
        F12 = 1073741893u,
        PrintScreen = 1073741894u,
        ScrollLock = 1073741895u,
        Pause = 1073741896u,
        Insert = 1073741897u,
        Home = 1073741898u,
        PageUp = 1073741899u,
        End = 1073741901u,
        PageDown = 1073741902u,
        Right = 1073741903u,
        Left = 1073741904u,
        Down = 1073741905u,
        Up = 1073741906u,
        NumLockClear = 1073741907u,
        KeypadDivide = 1073741908u,
        KeypadMultiply = 1073741909u,
        KeypadMinus = 1073741910u,
        KeypadPlus = 1073741911u,
        KeypadEnter = 1073741912u,
        Keypad1 = 1073741913u,
        Keypad2 = 1073741914u,
        Keypad3 = 1073741915u,
        Keypad4 = 1073741916u,
        Keypad5 = 1073741917u,
        Keypad6 = 1073741918u,
        Keypad7 = 1073741919u,
        Keypad8 = 1073741920u,
        Keypad9 = 1073741921u,
        Keypad0 = 1073741922u,
        KeypadPeriod = 1073741923u,
        Application = 1073741925u,
        Power = 1073741926u,
        KeypadEquals = 1073741927u,
        F13 = 1073741928u,
        F14 = 1073741929u,
        F15 = 1073741930u,
        F16 = 1073741931u,
        F17 = 1073741932u,
        F18 = 1073741933u,
        F19 = 1073741934u,
        F20 = 1073741935u,
        F21 = 1073741936u,
        F22 = 1073741937u,
        F23 = 1073741938u,
        F24 = 1073741939u,
        Execute = 1073741940u,
        Help = 1073741941u,
        Menu = 1073741942u,
        Select = 1073741943u,
        Stop = 1073741944u,
        Again = 1073741945u,
        Undo = 1073741946u,
        Cut = 1073741947u,
        Copy = 1073741948u,
        Paste = 1073741949u,
        Find = 1073741950u,
        Mute = 1073741951u,
        VolumeUp = 1073741952u,
        VolumeDown = 1073741953u,
        KeypadComma = 1073741957u,
        KeypadEqualsAs400 = 1073741958u,
        AltErase = 1073741977u,
        SysReq = 1073741978u,
        Cancel = 1073741979u,
        Clear = 1073741980u,
        Prior = 1073741981u,
        Return2 = 1073741982u,
        Separator = 1073741983u,
        Out = 1073741984u,
        Oper = 1073741985u,
        ClearAgain = 1073741986u,
        CrSel = 1073741987u,
        ExSel = 1073741988u,
        Keypad00 = 1073742000u,
        Keypad000 = 1073742001u,
        ThousandsSeparator = 1073742002u,
        DecimalSeparator = 1073742003u,
        CurrencyUnit = 1073742004u,
        CurrencySubunit = 1073742005u,
        KeypadLeftParen = 1073742006u,
        KeypadRightParen = 1073742007u,
        KeypadLeftBrace = 1073742008u,
        KeypadRightBrace = 1073742009u,
        KeypadTab = 1073742010u,
        KeypadBackspace = 1073742011u,
        KeypadA = 1073742012u,
        KeypadB = 1073742013u,
        KeypadC = 1073742014u,
        KeypadD = 1073742015u,
        KeypadE = 1073742016u,
        KeypadF = 1073742017u,
        KeypadXor = 1073742018u,
        KeypadPower = 1073742019u,
        KeypadPercent = 1073742020u,
        KeypadLess = 1073742021u,
        KeypadGreater = 1073742022u,
        KeypadAmpersand = 1073742023u,
        KeypadDoubleAmpersand = 1073742024u,
        KeypadVerticalBar = 1073742025u,
        KeypadDoubleVerticalBar = 1073742026u,
        KeypadColon = 1073742027u,
        KeypadHash = 1073742028u,
        KeypadSpace = 1073742029u,
        KeypadAt = 1073742030u,
        KeypadExclam = 1073742031u,
        KeypadMemStore = 1073742032u,
        KeypadMemRecall = 1073742033u,
        KeypadMemClear = 1073742034u,
        KeypadMemAdd = 1073742035u,
        KeypadMemSubtract = 1073742036u,
        KeypadMemMultiply = 1073742037u,
        KeypadMemDivide = 1073742038u,
        KeypadPlusMinus = 1073742039u,
        KeypadClear = 1073742040u,
        KeypadClearEntry = 1073742041u,
        KeypadBinary = 1073742042u,
        KeypadOctal = 1073742043u,
        KeypadDecimal = 1073742044u,
        KeypadHexadecimal = 1073742045u,
        LeftCtrl = 1073742048u,
        LeftShift = 1073742049u,
        LeftAlt = 1073742050u,
        LeftGui = 1073742051u,
        RightCtrl = 1073742052u,
        RightShift = 1073742053u,
        RightAlt = 1073742054u,
        RightGui = 1073742055u,
        Mode = 1073742081u,
        Sleep = 1073742082u,
        Wake = 1073742083u,
        ChannelIncrement = 1073742084u,
        ChannelDecrement = 1073742085u,
        MediaPlay = 1073742086u,
        MediaPause = 1073742087u,
        MediaRecord = 1073742088u,
        MediaFastForward = 1073742089u,
        MediaRewind = 1073742090u,
        MediaNextTrack = 1073742091u,
        MediaPreviousTrack = 1073742092u,
        MediaStop = 1073742093u,
        MediaEject = 1073742094u,
        MediaPlayPause = 1073742095u,
        MediaSelect = 1073742096u,
        AcNew = 1073742097u,
        AcOpen = 1073742098u,
        AcClose = 1073742099u,
        AcExit = 1073742100u,
        AcSave = 1073742101u,
        AcPrint = 1073742102u,
        AcProperties = 1073742103u,
        AcSearch = 1073742104u,
        AcHome = 1073742105u,
        AcBack = 1073742106u,
        AcForward = 1073742107u,
        AcStop = 1073742108u,
        AcRefresh = 1073742109u,
        AcBookmarks = 1073742110u,
        SoftLeft = 1073742111u,
        SoftRight = 1073742112u,
        Call = 1073742113u,
        EndCall = 1073742114u,
        LeftTab = 536870913u,
        Level5Shift = 536870914u,
        MultiKeyCompose = 536870915u,
        LeftMeta = 536870916u,
        RightMeta = 536870917u,
        LeftHyper = 536870918u,
        RightHyper = 536870919u
    }

    public enum MouseButton
    {
        Left = 1,
        Middle = 2,
        Right = 3,
    }

    public enum ControllerButton
    {
        None = -1,
        North = 0,
        South = 1,
        East = 2,
        West = 3,
        Back = 4,
        Guide = 5,
        Start = 6,
        LeftStick = 7,
        RightStick = 8,
        LeftShoulder = 9,
        RightShoulder = 10,
        DPadUp = 11,
        DPadDown = 12,
        DPadLeft = 13,
        DPadRight = 14,
    }

    public enum ControllerAxis
    {
        LeftStickX,
        LeftStickY,
        RightStickX,
        RightStickY,
        LeftTrigger,
        RightTrigger,
    }

    public enum ControllerAxisCombined
    {
        LeftStick,
        RightStick,
    }

    public sealed class InputProvider
    {
        // Type
        private struct InputState
        {
            // Public
            public bool Previous;
            public bool Current;
        }

        // Events
        public readonly GameEvent<MouseButton> OnMouseDown = new();
        public readonly GameEvent<MouseButton> OnMouseUp = new();
        public readonly GameEvent<Vector2F> OnMouseMove = new();

        // Private
        private static readonly MouseButton[] allMouseButtons = Enum.GetValues<MouseButton>();
        private static readonly Key[] allKeyboardButtons = Enum.GetValues<Key>();
        private static readonly ControllerButton[] allControllerButtons = Enum.GetValues<ControllerButton>();
        private static readonly ControllerAxis[] allControllerAxis = Enum.GetValues<ControllerAxis>();

        private Vector2F mousePreviousPosition = default;
        private Vector2F mousePosition = default;
        private Vector2F mousePositionDelta = default;
        private Vector2F mouseScrollDelta = default;

        private readonly Dictionary<MouseButton, InputState> mouseButtonStates = new();
        private readonly Dictionary<Key, InputState> keyboardButtonStates = new();
        private readonly Dictionary<(int, ControllerButton), InputState> controllerButtonStates = new();
        private readonly Dictionary<(int, ControllerAxis), float> controllerAxisStates = new();
        private readonly HashSet<int> controllerConnected = new();

        // Public
        public const int MaxControllers = 4;
        public const float ControllerAxisRange = 32768.0f;

        // Properties
        public Vector2F MousePosition => mousePosition;
        public Vector2F MousePositionDelta => mousePositionDelta;
        public Vector2F MouseScrollDelta => mouseScrollDelta;

        // Constructor
        internal InputProvider()
        {
            // Create mouse states
            foreach (MouseButton mouseButton in allMouseButtons)
                mouseButtonStates[mouseButton] = default;

            // Create key states
            foreach (Key key in allKeyboardButtons)
                keyboardButtonStates[key] = default;

            // Create all buttons
            foreach(ControllerButton controllerButton in allControllerButtons)
                for(int controllerId = 0; controllerId < MaxControllers; controllerId++)
                    controllerButtonStates[(controllerId, controllerButton)] = default;

            // Create all axis
            foreach (ControllerAxis controllerAxis in allControllerAxis)
                for (int controllerId = 0; controllerId < MaxControllers; controllerId++)
                    controllerAxisStates[(controllerId, controllerAxis)] = default;
        }

        // Methods
        public bool GetMouseDown(MouseButton mouse)
        {
            // Get the state
            InputState state = mouseButtonStates[mouse];

            // Check for down this frame
            return state.Current == true && state.Previous == false;
        }

        public bool GetMouseUp(MouseButton mouse)
        {
            // Get the state
            InputState state = mouseButtonStates[mouse];

            // Check for up this frame
            return state.Current == false && state.Previous == true;
        }

        public bool GetMouse(MouseButton mouse)
        {
            // Get the state
            InputState state = mouseButtonStates[mouse];

            // Check for held this frame
            return state.Current;
        }

        public bool GetKeyDown(Key key)
        {
            // Get the state
            InputState state = keyboardButtonStates[key];

            // Check for down this frame
            return state.Current == true && state.Previous == false;
        }

        public bool GetKeyUp(Key key)
        {
            // Get the state
            InputState state = keyboardButtonStates[key];

            // Check for up this frame
            return state.Current == false && state.Previous == true;
        }

        public bool GetKey(Key key)
        {
            // Get the state
            InputState state = keyboardButtonStates[key];

            // Check for held this frame
            return state.Current;
        }

        public bool IsControllerConnected(int controllerId = 0)
        {
            return controllerConnected.Contains(controllerId);
        }

        public bool GetControllerButtonDown(ControllerButton button, int controllerId = 0)
        {
            // Check id
            if (controllerId < 0 || controllerId >= MaxControllers)
                return false;

            // Get the state
            InputState state = controllerButtonStates[(controllerId, button)];

            // Check for down this frame
            return state.Current == true && state.Previous == false;
        }

        public bool GetControllerButtonUp(ControllerButton button, int controllerId = 0)
        {
            // Check id
            if (controllerId < 0 || controllerId >= MaxControllers)
                return false;

            // Get the state
            InputState state = controllerButtonStates[(controllerId, button)];

            // Check for up this frame
            return state.Current == false && state.Previous == true;
        }

        public bool GetControllerButton(ControllerButton button, int controllerId = 0)
        {
            // Check id
            if (controllerId < 0 || controllerId >= MaxControllers)
                return false;

            // Get the state
            InputState state = controllerButtonStates[(controllerId, button)];
            
            // Check for held this frame
            return state.Current;
        }

        public float GetControllerAxis(ControllerAxis axis, int controllerId = 0)
        {
            // Check id
            if (controllerId < 0 || controllerId >= MaxControllers)
                return 0f;

            // Get the value
            return controllerAxisStates[(controllerId, axis)];
        }

        public Vector2F GetControllerAxis(ControllerAxisCombined combinedAxis, int controllerId = 0)
        {
            // Check id
            if (controllerId < 0 || controllerId >= MaxControllers)
                return Vector2F.Zero;

            float x = 0f;
            float y = 0f;

            // Check axis
            switch(combinedAxis)
            {
                case ControllerAxisCombined.LeftStick:
                    {
                        // Read values
                        x = controllerAxisStates[(controllerId, ControllerAxis.LeftStickX)];
                        y = controllerAxisStates[(controllerId, ControllerAxis.LeftStickY)];
                        break;
                    }
                case ControllerAxisCombined.RightStick:
                    {
                        // Read values
                        x = controllerAxisStates[(controllerId, ControllerAxis.RightStickX)];
                        y = controllerAxisStates[(controllerId, ControllerAxis.RightStickY)];
                        break;
                    }
            }

            // Create result
            Vector2F result = new Vector2F(x, y);

            // Normalize
            result.Normalize();

            return result;
        }

        internal void UpdateInputStates()
        {
            // Update mouse
            foreach(MouseButton mouseButton in allMouseButtons)
            {
                InputState state = mouseButtonStates[mouseButton];
                mouseButtonStates[mouseButton] = new InputState
                {
                    Previous = state.Current,
                    Current = state.Current,
                };
            }

            // Update keyboard
            foreach(Key key in allKeyboardButtons)
            {
                InputState state = keyboardButtonStates[key];
                keyboardButtonStates[key] = new InputState
                {
                    Previous = state.Current,
                    Current = state.Current,
                };
            }

            // Update controller
            foreach(ControllerButton controllerButton in allControllerButtons)
            {
                for (int controllerId = 0; controllerId < MaxControllers; controllerId++)
                {
                    InputState state = controllerButtonStates[(controllerId, controllerButton)];
                    controllerButtonStates[(controllerId, controllerButton)] = new InputState
                    {
                        Previous = state.Current,
                        Current = state.Current,
                    };
                }
            }

            // Update mouse position
            mousePositionDelta = mousePosition - mousePreviousPosition;
            mousePreviousPosition = mousePosition;
        }

        internal void DoMouseMove(float x, float y)
        {
            // Update position
            mousePosition.X = x;
            mousePosition.Y = y;

            // Trigger event
            OnMouseMove.Raise(new Vector2F(x, y));
        }

        internal void DoMouseWheelMove(float xDelta, float yDelta)
        {
            // Update delta
            mouseScrollDelta.X = xDelta;
            mouseScrollDelta.Y = yDelta;
        }

        internal void DoMouseButtonEvent(MouseButton button, bool value)
        {
            // Get current state
            InputState state = mouseButtonStates[button];

            // Update state
            state.Current = value;

            // Apply state
            mouseButtonStates[button] = state;

            // Trigger event
            switch(value)
            {
                case true: OnMouseDown.Raise(button); break;
                case false: OnMouseUp.Raise(button); break;
            }
        }

        internal void DoKeyboardButtonEvent(Key key, bool value)
        {
            // Get current state
            InputState state = keyboardButtonStates[key];

            // Update state
            state.Current = value;

            // Apply state
            keyboardButtonStates[key] = state;
        }

        internal void DoControllerAvailabilityEvent(int controllerId, bool available)
        {
            // Check for out of bounds
            if (controllerId >= MaxControllers)
                return;

            // Check for added
            if(available == true)
            { 
                // Add device
                if(controllerConnected.Contains(controllerId) == false)
                    controllerConnected.Add(controllerId);

                // Report connected
                Debug.Log($"Controller connected: {controllerId}", LogFilter.Input);
            }
            else
            {
                // Remove device
                if (controllerConnected.Contains(controllerId) == false)
                    controllerConnected.Add(controllerId);

                // Report disconnected
                Debug.Log($"Controller disconnected: {controllerId}", LogFilter.Input);
            }
        }

        internal void DoControllerButtonEvent(int controllerId, ControllerButton button, bool value)
        {
            // Check for out of bounds
            if (controllerId >= MaxControllers)
                return;

            // Get current state
            InputState state = controllerButtonStates[(controllerId, button)];

            // Update state
            state.Current = value;

            // Apply state
            controllerButtonStates[(controllerId, button)] = state;
        }

        internal void DoControllerAxisEvent(int controllerId, ControllerAxis axis, float value)
        {
            // Check for out of bounds
            if (controllerId >= MaxControllers)
                return;

            // Apply state
            controllerAxisStates[(controllerId, axis)] = value;
        }
    }
}
