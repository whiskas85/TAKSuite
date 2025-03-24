using System.ComponentModel.DataAnnotations;
using TAKSuite.Data.Models;

namespace TAKSuite.Components.Pages.TeamSettingsPage
{
    public class RadioChannelInput
    {
        public RadioChannelInput()
        {
                
        }
        public RadioChannelInput(TeamRadioChannel trc)
        {
            ChannelName = trc.Name;
            Position = trc.Position;
            RadioChannel = trc.RadioChannelId;
            BackupRadioChannel = trc.BackupRadioChannelId;
            StartDate = trc.BeginValidityPeriod;
            EndDate = trc.EndValidityPeriod;
        }
        public Guid? Id { get; set; }
        public Guid TeamID { get; set; }

        public String ChannelName { get; set; }

        public ChannelType Position { get; set; }

        [Required]
        public Guid? RadioChannel { get; set; }
        public Guid? BackupRadioChannel { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

    }
}
