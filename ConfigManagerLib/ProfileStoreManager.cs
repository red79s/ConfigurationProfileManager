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
                _config.Profiles = _config.Profiles.OrderBy(x => x.Name).ToList();
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
            var txt = System.Text.Json.JsonSerializer.Serialize<ConfigurationInfo>(_config, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
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

        public void RenameProfile(string oldProfileName, string newProfileName)
        {
            if (oldProfileName == newProfileName)
            {
                return;
            }

            if (_config.Profiles.Any(p => p.Name == newProfileName))
            {
                throw new Exception($"Profile already exists: {newProfileName}");
            }

            var profile = GetProfile(oldProfileName);
            if (profile == null)
            {
                throw new Exception($"Invalid profile: {oldProfileName}");
            }

            var newProfileDirectory = Path.Combine(_defaultProfileStorageDirectory, newProfileName);
            Directory.Move(profile.ProfileDirectory, newProfileDirectory);
            profile.Name = newProfileName;
            profile.ProfileDirectory = newProfileDirectory;

            foreach (var file in profile.Files)
            {
                var fi = new FileInfo(file.FileName);
                file.FileName = Path.Combine(newProfileDirectory, fi.Name);
            }

            Save();
        }

        public ProfileInfo CloneProfile(string profileName, string newProfileName)
        {
            if (_config.Profiles.Any(p => p.Name == newProfileName))
            {
                throw new Exception($"Profile already exists: {newProfileName}");
            }

            var profile = GetProfile(profileName);
            if (profile == null)
            {
                throw new Exception($"Invalid profile: {profileName}");
            }

            var newProfile = AddProfile(newProfileName);
            foreach (var file in profile.Files)
            {
                var fi = new FileInfo(file.FileName);
                var newFileName = Path.Combine(newProfile.ProfileDirectory, $"{Guid.NewGuid()} - {fi.Name}");
                File.Copy(file.FileName, newFileName, true);
                newProfile.Files.Add(new ConfigFileInfo
                {
                    Description = file.Description,
                    OriginalFileName = file.OriginalFileName,
                    FileName = newFileName,
                    Created = DateTime.Now,
                    Updated = DateTime.Now
                });
            }

            Save();

            return profile;
        }

        public ConfigFileInfo AddFileToProfile(string profileName, string fileName)
        {
            var profile = GetProfile(profileName);
            if (profile == null)
            {
                throw new Exception($"Invalid profile: {profileName}");
            }

            var fi = new FileInfo(fileName);
            var newFileName = Path.Combine(profile.ProfileDirectory, $"{Guid.NewGuid()} - {fi.Name}");
            var fileInfo = new ConfigFileInfo
            {
                Description = fi.Name,
                OriginalFileName = fileName,
                FileName = newFileName,
                Created = DateTime.Now,
                Updated = DateTime.Now
            };

            File.Copy(fileName, newFileName, true);
            profile.Files.Add(fileInfo);

            Save();

            return fileInfo;
        }
    }
}
