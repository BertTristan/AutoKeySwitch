
namespace AutoKeySwitch.Core.Models.Messages
{
    public class SwitchLayoutMessage : PipeMessage
    {
        public override string Type => "SwitchLayout";
        public string AppName { get; set; } = "";
        public string AppPath { get; set; } = "";
        public string Layout { get; set; } = "";
    }
}