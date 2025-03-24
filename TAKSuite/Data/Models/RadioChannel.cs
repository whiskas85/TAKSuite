namespace TAKSuite.Data.Models
{
    public class RadioChannel: IGuidModel
    {
        public Guid Id { get; set; }
        public String Name { get; set; }
        public String FrequencyType { get; set; }
        public string Frequency { get; set; }

    }
}
