using System.Configuration;

namespace FilesUpdater.Infrastructure
{
    internal class StartupSettings : ConfigurationSection
    {
        private static readonly StartupSettings _settings = ConfigurationManager.GetSection("StartupSettings") as StartupSettings;

        public static StartupSettings Settings => _settings;

        [ConfigurationProperty("handleInternalDirectories", IsRequired = true)]
        public bool HandleInternalDirectories
        {
            get => (bool)this["handleInternalDirectories"];
            set => this["handleInternalDirectories"] = value;
        }

        [ConfigurationProperty("executeScript", IsRequired = true)]
        public bool ExecuteScript
        {
            get => (bool)this["executeScript"];
            set => this["executeScript"] = value;
        }

        [ConfigurationProperty("executeScriptParams", IsRequired = true)]
        public string ExecuteScriptParams
        {
            get => (string)this["executeScriptParams"];
            set => this["executeScriptParams"] = value;
        }

        [ConfigurationProperty("executeScriptTimeout", IsRequired = true)]
        public int ExecuteScriptTimeout
        {
            get => (int)this["executeScriptTimeout"];
            set => this["executeScriptTimeout"] = value;
        }

        [ConfigurationProperty("fileSearchMask", IsRequired = true)]
        [StringValidator(InvalidCharacters = "", MinLength = 0, MaxLength = 50)]
        public string FileSearchMask
        {
            get => (string)this["fileSearchMask"];
            set => this["fileSearchMask"] = value;
        }

        [ConfigurationProperty("sourcePathName", IsRequired = true)]
        [StringValidator(InvalidCharacters = "", MinLength = 0, MaxLength = 255)]
        public string SourcePathName
        {
            get => (string)this["sourcePathName"];
            set => this["sourcePathName"] = value;
        }

        [ConfigurationProperty("remouteHostDestinationPath", IsRequired = true)]
        [StringValidator(InvalidCharacters = "", MinLength = 0, MaxLength = 255)]
        public string RemouteHostDestinationPath
        {
            get => (string)this["remouteHostDestinationPath"];
            set => this["remouteHostDestinationPath"] = value;
        }

        [ConfigurationProperty("remouteHostSourceFileName", IsRequired = true)]
        [StringValidator(InvalidCharacters = "  /\"|", MinLength = 0, MaxLength = 100)]
        public string RemouteHostSourceFileName
        {
            get => (string)this["remouteHostSourceFileName"];
            set => this["remouteHostSourceFileName"] = value;
        }

        [ConfigurationProperty("hostErrorsFileName", IsRequired = true)]
        [StringValidator(InvalidCharacters = "  /\"|", MinLength = 0, MaxLength = 100)]
        public string HostErrorsFileName
        {
            get => (string)this["hostErrorsFileName"];
            set => this["hostErrorsFileName"] = value;
        }

        [ConfigurationProperty("logFileName", IsRequired = true)]
        [StringValidator(InvalidCharacters = "  /\"|", MinLength = 0, MaxLength = 100)]
        public string LogFileName
        {
            get => (string)this["logFileName"];
            set => this["logFileName"] = value;
        }
    }
}

