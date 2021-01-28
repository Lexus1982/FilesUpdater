using System.Threading;

namespace FilesUpdater.DTO
{
    internal class TaskParams
    { 
        public CancellationToken Token { get; set; }
        public SynchronizationContext Context { get; set; }
        public string[] DestinationHostNames { get; set; }
        public string DestinationPath { get; set; }
        public string SourcePath { get; set; }
        public string SearchPattern { get; set; }
        public bool ExecuteScript { get; set; }
        public string ScriptParams { get; set; }
        public int ScriptTimeout { get; set; }
        public bool ExecuteInternalDirectories { get; set; }
    }
}
