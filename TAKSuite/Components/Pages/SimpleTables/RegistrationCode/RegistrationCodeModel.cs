
using System.ComponentModel.DataAnnotations;

namespace TAKSuite.Components.Pages.SimpleTables.RegistrationCode
{
    public class RegistrationCodeModel
    {
        public RegistrationCodeModel()
        {
                
        }
        public RegistrationCodeModel(Data.Models.RegistrationCode code)
        {
            Code = code.Code;
            Id = code.Id;
            TeamId = code.TeamId;
            
            if (code.ExpirationDate.HasValue)
                ExpirationDate = code.ExpirationDate.Value;
        }
        public Guid? Id { get; set; }  
        
        [Required]
        [StringLength(10, ErrorMessage = "Id is too long.")]
        public string Code { get; set; }

        public Guid TeamId { get; set; }
        public DateTime ExpirationDate { get; set; }
    }
}
