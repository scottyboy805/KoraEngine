using SDL;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace KoraEditor.UI
{
    internal unsafe class FileDialog
    {
        // Private
        private static Dictionary<IntPtr, Action<string>> singlePathCallback = new();
        private static Dictionary<IntPtr, Action<string[]>> multiPathCallbacks = new();

        // Methods
        public static void ShowSaveFileDialog(Action<string> callback, string filter, string directory)
        {
            // Check for filter and directory
            bool hasFilter = string.IsNullOrEmpty(filter) == false;
            bool hasDirectory = string.IsNullOrEmpty(directory) == false;

            SDL_DialogFileFilter fileFilter = default;
            byte* filterNamePtr = null;
            byte* filterStringPtr = null;
            byte* defaultLocationPtr = null;

            // Check for filter
            if (hasFilter == true)
            {
                // Allocate strings
                filterNamePtr = Utf8StringMarshaller.ConvertToUnmanaged(filter.TrimStart('.'));
                filterStringPtr = Utf8StringMarshaller.ConvertToUnmanaged(filter);

                fileFilter = new SDL_DialogFileFilter
                {
                    name = filterNamePtr,
                    pattern = filterStringPtr,
                };

            }

            // Check for directory
            if (hasDirectory == true)
            {
                defaultLocationPtr = Utf8StringMarshaller.ConvertToUnmanaged(directory);
            }

            IntPtr callbackPtr = IntPtr.Zero;
            if (callback != null)
            {
                // Register callback
                lock (singlePathCallback)
                {
                    Random rand = new();
                    IntPtr id = IntPtr.Zero;

                    // Generate random id for callback
                    while (id == IntPtr.Zero || singlePathCallback.ContainsKey(id) == true)
                        id = rand.Next();

                    // Register callback
                    singlePathCallback.Add(id, callback);
                    callbackPtr = id;
                }
            }

            // Show the dialog
            SDL3.SDL_ShowSaveFileDialog(&SinglePathDialogCallback, callbackPtr, null, hasFilter ? &fileFilter : null, hasFilter ? 1 : 0, defaultLocationPtr);

            // Free pointers
            if (hasFilter == true)
            {
                Utf8StringMarshaller.Free(filterNamePtr);
                Utf8StringMarshaller.Free(filterStringPtr);
            }
            if (hasDirectory == true)
            {
                Utf8StringMarshaller.Free(defaultLocationPtr);
            }
        }

        public static void ShowOpenFileDialog(Action<string[]> callback, string filter, string directory, bool multiple)
        {
            // Check for filter and directory
            bool hasFilter = string.IsNullOrEmpty(filter) == false;
            bool hasDirectory = string.IsNullOrEmpty(directory) == false;

            SDL_DialogFileFilter fileFilter = default;
            byte* filterNamePtr = null;
            byte* filterStringPtr = null;
            byte* defaultLocationPtr = null;

            // Check for filter
            if (hasFilter == true)
            {
                // Allocate strings
                filterNamePtr = Utf8StringMarshaller.ConvertToUnmanaged(filter.TrimStart('.'));
                filterStringPtr = Utf8StringMarshaller.ConvertToUnmanaged(filter);

                fileFilter = new SDL_DialogFileFilter
                {
                    name = filterNamePtr,
                    pattern = filterStringPtr,
                };

            }

            // Check for directory
            if (hasDirectory == true)
            {
                defaultLocationPtr = Utf8StringMarshaller.ConvertToUnmanaged(directory);
            }

            IntPtr callbackPtr = IntPtr.Zero;
            if (callback != null)
            {
                // Register callback
                lock (multiPathCallbacks)
                {
                    Random rand = new();
                    IntPtr id = IntPtr.Zero;

                    // Generate random id for callback
                    while (id == IntPtr.Zero || multiPathCallbacks.ContainsKey(id) == true)
                        id = rand.Next();

                    // Register callback
                    multiPathCallbacks.Add(id, callback);
                    callbackPtr = id;
                }
            }

            // Show the dialog
            SDL3.SDL_ShowOpenFileDialog(&MultiPathDialogCallback, callbackPtr, null, hasFilter ? &fileFilter : null, hasFilter ? 1 : 0, defaultLocationPtr, multiple);

            // Free pointers
            if (hasFilter == true)
            {
                Utf8StringMarshaller.Free(filterNamePtr);
                Utf8StringMarshaller.Free(filterStringPtr);
            }
            if (hasDirectory == true)
            {
                Utf8StringMarshaller.Free(defaultLocationPtr);
            }
        }

        public static void ShowOpenFolderDialog(Action<string[]> callback, string directory, bool multiple)
        {
            // Check for directory
            bool hasDirectory = string.IsNullOrEmpty(directory) == false;

            byte* defaultLocationPtr = null;

            // Check for directory
            if (hasDirectory == true)
            {
                defaultLocationPtr = Utf8StringMarshaller.ConvertToUnmanaged(directory);
            }

            IntPtr callbackPtr = IntPtr.Zero;
            if (callback != null)
            {
                // Register callback
                lock (multiPathCallbacks)
                {
                    Random rand = new();
                    IntPtr id = IntPtr.Zero;

                    // Generate random id for callback
                    while (id == IntPtr.Zero || multiPathCallbacks.ContainsKey(id) == true)
                        id = rand.Next();

                    // Register callback
                    multiPathCallbacks.Add(id, callback);
                    callbackPtr = id;
                }
            }

            // Show the dialog
            SDL3.SDL_ShowOpenFolderDialog(&MultiPathDialogCallback, callbackPtr, null, defaultLocationPtr, multiple);

            // Free pointers
            if (hasDirectory == true)
            {
                Utf8StringMarshaller.Free(defaultLocationPtr);
            }
        }

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static void SinglePathDialogCallback(IntPtr userData, byte** files, int fileterIndex)
        {
            // Check id
            if(userData == IntPtr.Zero) 
                return;

            // Get managed string
            string path = files != null
                ? Utf8StringMarshaller.ConvertToManaged(files[0])
                : null;

            // Get the callback
            Action<string> callback = null;

            lock(singlePathCallback)
            {
                if(singlePathCallback.TryGetValue(userData, out callback) == true)
                    singlePathCallback.Remove(userData);
            }

            // Invoke callback
            if(callback != null)
                callback(path);
        }

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static void MultiPathDialogCallback(IntPtr userData, byte** files, int filterIndex)
        {
            // Check id
            if (userData == IntPtr.Zero)
                return;

            // Get length
            int length = 0;

            byte** fileCount = files;
            while(fileCount != null && fileCount[length] != null)
                length++;

            // Create array
            string[] fileNames = files != null
                ? new string[length]
                : null;

            for (int i = 0; i < length; i++)
            {
                // Get managed string
                fileNames[i] = files != null
                    ? Utf8StringMarshaller.ConvertToManaged(files[i])
                    : null;
            }

            // Get the callback
            Action<string[]> callback = null;

            lock (multiPathCallbacks)
            {
                if (multiPathCallbacks.TryGetValue(userData, out callback) == true)
                    multiPathCallbacks.Remove(userData);
            }

            // Invoke callback
            if (callback != null)
                callback(fileNames);
        }
    }
}
