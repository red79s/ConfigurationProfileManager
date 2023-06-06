using ConfigManagerLib.Model;

namespace ConfigManagerLib
{
    public class ProfileStoreManager : IProfileStoreManager
    {
        private readonly string _configFile;
        private readonly string _defaultProfileStorageDirectory;
        private ConfigurationInfo _config;

        public ProfileStoreManager(string configFile, string defaultProfileStorageDirectory)
        {
            _configFile = configFile;
            _defaultProfileStorageDirectory = defaultProfileStorageDirectory;
            Load();
        }

        public ConfigurationInfo Configuration
        {
            get
            {
                return _config;
            }
        }

        private void Load()
        {
            if (File.Exists(_configFile))
            {
                var txt = File.ReadAllText(_configFile);
                var profiles = System.Text.Json.JsonSerializer.Deserialize<ConfigurationInfo>(txt);
                _config = profiles ?? CreateNewConfig();
            }
            else
            {
                _config = CreateNewConfig();
            }
        }

        private ConfigurationInfo CreateNewConfig()
        {
            if (!Directory.Exists(_defaultProfileStorageDirectory))
            {
                Directory.CreateDirectory(_defaultProfileStorageDirectory);
            }

            return new ConfigurationInfo { ProfileFolder = _defaultProfileStorageDirectory, Profiles = new List<ProfileInfo>() };
        }

        public void Save()
        {
            var txt = System.Text.Json.JsonSerializer.Serialize<ConfigurationInfo>(_config);
            File.WriteAllText(_configFile, txt);
        }

        public ProfileInfo AddProfile(string profileName)
        {
            var profile = _config.Profiles.FirstOrDefault(p => p.Name == profileName);
            if (profile != null)
            {
                return profile;
            }

            var profileDirectory = Path.Combine(_defaultProfileStorageDirectory, profileName);
            Directory.CreateDirectory(profileDirectory);

            profile = new ProfileInfo
            {
                Name = profileName,
                Created = DateTime.Now,
                Updated = DateTime.Now,
                ProfileDirectory = profileDirectory,
                Files = new List<ConfigFileInfo>()
            };

            _config.Profiles.Add(profile);

            

            return profile;
        }

        public ProfileInfo GetProfile(string profileName)
        {
            return _config.Profiles.FirstOrDefault(p => p.Name.Equals(profileName, StringComparison.CurrentCultureIgnoreCase));
        }

        public void UseProfile(string profileName)
        {
            var profile = GetProfile(profileName);
            if (profile == null)
            {
                throw new Exception($"Invalid profile: {profileName}");
            }

            foreach (var file in profile.Files)
            {
                File.Copy(file.FileName, file.OriginalFileName, true);
            }
        }

        public void DeleteProfile(string profileName)
        {
            var profile = GetProfile(profileName);
            if (profile == null)
            {
                throw new Exception($"Invalid profile: {profileName}");
            }

            foreach (var file in profile.Files)
            {
                File.Delete(file.FileName);
            }

            Directory.Delete(profile.ProfileDirectory, true);

            _config.Profiles.Remove(profile);

            Save();
        }
    }
}
