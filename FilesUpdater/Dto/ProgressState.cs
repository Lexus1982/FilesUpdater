using System.Windows.Forms;

namespace FilesUpdater.DTO
{
    internal class ProgressState
    {
        public int ProgressValue { get; set; }
        public string HostName { get; set; }
        public string Message { get; set; }
        public /*OperationState*/ EventType State { get; set; }

        public ProgressState() : this(EventType.Info, 0, string.Empty, string.Empty) { }
        public ProgressState(EventType state, string hostName, string message) : this(state, 0, hostName, message) { }
        
        public ProgressState(EventType state, int progressValue, string hostName, string message)
        {
            State = state;
            ProgressValue = progressValue;
            HostName = hostName;
            Message = message;
        }
    }

    internal enum EventType
    {
        Info,
        Error,
        Complete,
        Cancellation
    }
}
