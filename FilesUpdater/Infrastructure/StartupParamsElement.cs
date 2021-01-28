using System.Configuration;

namespace FilesUpdater.Infrastructure
{
    public class StartupParamsElement : ConfigurationElement
    {
        [ConfigurationProperty("paramName", DefaultValue = "", IsKey = true, IsRequired = true)]
        public string ParamName
        {
            get => ((string)(base["paramName"]));
            set => base["paramName"] = value;
        }

        [ConfigurationProperty("value", DefaultValue = "", IsKey = false, IsRequired = false)]
        public string Value
        {
            get => ((string)(base["value"]));
            set => base["value"] = value;
        }
    }
}
