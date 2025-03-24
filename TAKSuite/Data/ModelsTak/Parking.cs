using System.Text.Json;

namespace TAKSuite.Data.ModelsTak
{
    public class Parking : BaseCoT<Parking>, IColoredCoT
    {
        public int? Color { get; set; }

        public Parking(JsonElement uid) : base(uid)
        {
            Color = int.Parse(uid.GetProperty("details").GetProperty("color").GetString());
        }
    }
}
