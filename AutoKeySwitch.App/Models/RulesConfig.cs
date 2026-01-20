
namespace AutoKeySwitch.App.Models
{
    public class RulesConfig
    {
        public string DefaultLayout { get; set; } = "fr-FR";
        public List<GameRule> Rules { get; set; } = [];
    }
}