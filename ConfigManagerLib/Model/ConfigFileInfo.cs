namespace ConfigManagerLib.Model
{
    public class ConfigFileInfo
    {
        public string Description { get; set; }
        public string FileName { get; set; }
        public string OriginalFileName { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}
