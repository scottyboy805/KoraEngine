using System.Runtime.Serialization;

namespace KoraGame
{
    [Serializable]
    public sealed class GameSettings
    {
        // Private
        [DataMember(Name = "GameName")]
        private string gameName = "Default Game";
        [DataMember(Name = "GameVersion")]
        private Version gameVersion = new Version(1, 0, 0);
        [DataMember(Name = "CompanyName")]
        private string companyName = "Default Company";
        [DataMember(Name = "PreferredScreenWidth")]
        private uint preferredScreenWidth = 720;
        [DataMember(Name = "PreferredScreenHeight")]
        private uint preferredScreenHeight = 480;
        [DataMember(Name = "FullScreen")]
        private bool fullScreen = false;

        // Properties
        public string GameName => gameName;
        public Version GameVersion => gameVersion;
        public string CompanyName => companyName;
        public uint PreferredScreenWidth => preferredScreenWidth;
        public uint PreferredScreenHeight => preferredScreenHeight;
        public bool Fullscreen => fullScreen;
    }
}
