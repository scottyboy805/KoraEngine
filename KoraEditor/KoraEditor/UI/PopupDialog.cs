using SDL;
using System.Runtime.InteropServices.Marshalling;
using System.Text;

namespace KoraEditor.UI
{
    internal class PopupDialog
    {
        public struct DialogButton
        {
            // Public
            public string Text;
            public SDL_MessageBoxButtonFlags Flags;
        }

        // Methods
        public static unsafe int ShowPopupDialog(string title, string text, SDL_MessageBoxFlags flags, DialogButton[] buttons)
        {
            // Check for invalid
            if (buttons.Length == 0)
                throw new ArgumentException("Must be at least 1 button");

            // Create the buttons
            SDL_MessageBoxButtonData[] buttonData = new SDL_MessageBoxButtonData[buttons.Length];

            for (int i = 0; i < buttons.Length; i++)
            {
                // Create the string
                byte* buttonPtr = Utf8StringMarshaller.ConvertToUnmanaged(buttons[i].Text);

                // Create the button info
                buttonData[i] = new SDL_MessageBoxButtonData
                {
                    buttonID = i,
                    flags = buttons[i].Flags,
                    text = buttonPtr,
                };
            }

            // Get strings
            byte* titlePtr = Utf8StringMarshaller.ConvertToUnmanaged(title);
            byte* textPtr = Utf8StringMarshaller.ConvertToUnmanaged(text);

            // The button that was selected
            int selectedButton = 0;

            // Pin the array
            fixed (SDL_MessageBoxButtonData* buttonDataPtr = buttonData)
            {                
                // Create the message info
                SDL_MessageBoxData messageData = new SDL_MessageBoxData
                {
                    title = titlePtr,
                    message = textPtr,
                    flags = flags,
                    buttons = buttonDataPtr,
                    numbuttons = buttons.Length,
                };

                // Show the dialog                
                SDL3.SDL_ShowMessageBox(&messageData, &selectedButton);
            }

            // Free strings
            Utf8StringMarshaller.Free(titlePtr);
            Utf8StringMarshaller.Free(textPtr);

            for (int i = 0; i < buttonData.Length; i++)
                Utf8StringMarshaller.Free(buttonData[i].text);

            return selectedButton;
        }
    }
}
