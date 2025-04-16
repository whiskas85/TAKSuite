using BlazorReflection.Attributes;
using TAKSuite.Data.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using BlazorReflection.Data;
using TAKSuite.Components.Pages.Components.Data;
using TAKSuite.Components.Pages.Components;


public class TeamViewModel : BaseEntityViewModel<Team>, IFormModel<Team>
{
    public TeamViewModel(Team model) : base(model) { }

    public static IFormModel<Team> Create(Team item) => new TeamViewModel(item);



    
    public String? Name
    {
        get => Model.Name;
        set => Model.Name = value;
    }

    [Required(ErrorMessage = "Title is required.")]
    [FormControl(FormControlType.Color)]
    public int? Color
    {
        get => Model.Color;
        set => Model.Color = value.Value;
    }

}
