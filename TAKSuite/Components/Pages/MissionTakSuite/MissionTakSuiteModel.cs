using BlazorReflection.Attributes;
using BlazorReflection.Data;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TAKSuite.Components.Pages.Components;
using TAKSuite.Components.Pages.Components.Data;
using TAKSuite.Data.Models;

namespace TAKSuite.Components.Pages.MissionTakSuite
{
    public class MissionTakSuiteModel : BaseEntityViewModel<MissionSuite>, IFormModel<MissionSuite>
    {
        public MissionTakSuiteModel(MissionSuite model) : base(model) { }

        public static IFormModel<MissionSuite> Create(MissionSuite item) => new MissionTakSuiteModel(item);



        [Required(ErrorMessage = "Name is required.")]
        [DisplayName("Nome")]
        public string? Name
        {
            get => Model.Name;
            set => Model.Name = value;
        }

        [FormControl(FormControlType.Textarea)]
        [DisplayName("Descrizione")]
        public string? Descrizione
        {
            get => Model.Descrizione;
            set => Model.Descrizione = value;
        }

    }

}
