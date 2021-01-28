using  System.Configuration;

namespace FilesUpdater.Infrastructure
{
    [ConfigurationCollection(typeof(StartupParamsElement))]
    public class StartupParamsCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new StartupParamsElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((StartupParamsElement)(element)).ParamName;
        }
        public StartupParamsElement this[int idx] => (StartupParamsElement)BaseGet(idx);
    }
}
