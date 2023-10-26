using ConfigManagerLib.Model;

namespace ConfigManagerLib
{
    public interface IProfileStoreManager
    {
        ConfigurationInfo Configuration { get; }

        void Save();

        public ProfileInfo AddProfile(string profileName);
        public ProfileInfo GetProfile(string profileName);
        public void UseProfile(string profileName);
        public void DeleteProfile(string profileName);
        public void RenameProfile(string oldProfileName, string newProfileName);
        public ProfileInfo CloneProfile(string profileName, string newProfileName);
        public ConfigFileInfo AddFileToProfile(string profileName, string fileName);
    }
}