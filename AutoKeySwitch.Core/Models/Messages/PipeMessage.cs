
namespace AutoKeySwitch.Core.Models.Messages
{
    public abstract class PipeMessage
    {
        public abstract string Type { get; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}