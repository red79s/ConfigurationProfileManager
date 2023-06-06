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
    }
}