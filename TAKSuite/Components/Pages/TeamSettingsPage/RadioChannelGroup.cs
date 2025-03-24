using TAKSuite.Data.Models;

namespace TAKSuite.Components.Pages.TeamSettingsPage
{
    public class RadioChannelGroup
    {
        public String Name { get; set; }
        public ChannelType ChannelType { get; set; }
        public List<TeamRadioChannel> RadioChannelList { get; set; }

    }
}
