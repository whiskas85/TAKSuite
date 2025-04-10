using System.ComponentModel.DataAnnotations;

namespace TAKSuite.Data.Models
{
    public class CoordinateSystem
    {
        [Key]
        public int EpsgCode { get; set; }
        public string Name { get; set; }
        
        public double South { get; set; }
        public double West { get; set; }
        public double North { get; set; }
        public double East { get; set; }
    }
}
