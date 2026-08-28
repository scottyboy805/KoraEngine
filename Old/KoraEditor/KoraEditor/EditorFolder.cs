using KoraPipeline;
using System.Runtime.Serialization;

namespace KoraEditor
{
    [Serializable]
    public sealed class EditorFolder
    {
        // Private
        private string folderRelativePath = "";

        // Properties
        /// <summary>
        /// The folder path relative to the project folder.
        /// </summary>
        [DataMember]
        public string FolderPath
        {
            get => folderRelativePath;
            internal set => folderRelativePath = value;
        }

        // Constructor
        public EditorFolder(string folderRelativePath)
        {
            this.folderRelativePath = folderRelativePath;

            // Check for valid
            AssetDatabase.CheckAssetPathValid(folderRelativePath);
        }
    }
}
