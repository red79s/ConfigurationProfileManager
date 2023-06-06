using ConfigManagerLib;

namespace ConfigManagerLibTests
{
    [TestClass]
    public class TestProfileStoreManager
    {
        [TestMethod]
        public void TestCreateConfig()
        {
            var folder = GetTmpDirectory();
            var cfgFile = Path.Combine(folder, "confgi.json");
            var profileFolder = Path.Combine(folder, "Profiles");
            var psm = new ProfileStoreManager(cfgFile, profileFolder);
            Assert.AreEqual(profileFolder, psm.Configuration.ProfileFolder);
        }

        [TestMethod]
        public void TestAddProfile()
        {
            var folder = GetTmpDirectory();
            var cfgFile = Path.Combine(folder, "confgi.json");
            var profileFolder = Path.Combine(folder, "Profiles");
            var psm = new ProfileStoreManager(cfgFile, profileFolder);
            var p = psm.AddProfile("test");
            
            Assert.AreEqual(psm.Configuration.Profiles.Count, 1);
            Assert.AreEqual(profileFolder, psm.Configuration.ProfileFolder);
        }


        [TestMethod]
        public void TestSave()
        {
            var folder = GetTmpDirectory();
            var cfgFile = Path.Combine(folder, "confgi.json");
            var profileFolder = Path.Combine(folder, "Profiles");
            var psm = new ProfileStoreManager(cfgFile, profileFolder);
            var p = psm.AddProfile("test");
            psm.Save();

            psm = new ProfileStoreManager(cfgFile, profileFolder);
            Assert.AreEqual("test", psm.Configuration.Profiles[0].Name);
            Assert.AreEqual(profileFolder, psm.Configuration.ProfileFolder);
        }

        public string GetTmpDirectory()
        {
            string tmpDirectory;

            do
            {
                tmpDirectory = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
            }
            while (Directory.Exists(tmpDirectory));

            Directory.CreateDirectory(tmpDirectory);
            return tmpDirectory;
        }
    }
}