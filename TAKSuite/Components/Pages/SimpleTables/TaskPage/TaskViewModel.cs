using BlazorReflection.Attributes;
using TAKSuite.Data.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using BlazorReflection.Data;
using TAKSuite.Components.Pages.Components.Data;
using TAKSuite.Components.Pages.Components;
using Microsoft.AspNetCore.Components;
using TAKSuite.Data.Services;
using BlazorBootstrap;

public class TaskViewModel : BaseEntityViewModel<TaskEntity>, IFormModel<TaskEntity>
{
    public TaskViewModel(TaskEntity model) : base(model) { }

    public static IFormModel<TaskEntity> Create(TaskEntity item) => new TaskViewModel(item);



    [Required(ErrorMessage = "L'attività è richiesta")]
    [DisplayName("Attività")]
    public string? Attivita
    {
        get => Model.Name;
        set => Model.Name = value;
    }
    
    [Required(ErrorMessage = "L'attività è richiesta")]
    [DisplayName("Descrizione")]
    [FormControl(FormControlType.MarkDown)]
    public string? Descrizione
    {
        get => Model.Description;
        set => Model.Description = value;
    }


    [DisplayName("Team assegnato")]
    [FormControlDropDownType(typeof(Team))]
    public Guid? AssignedTeam
    {
        get => Model.AssignedTeamId;
        set => Model.AssignedTeamId = value;
    }

    [DisplayName("Priorità")]
    [FormControlDropDownType(typeof(TaskPriority))]
    public Guid? Priority
    {
        get => Model.PriorityId;
        set => Model.PriorityId = value;
    }

    public static async Task<IEnumerable<KeyValuePair<Guid, string>>> GetValuesForAssignedTeam(DataServiceAbstract<Team> service)
    {
        var list = await service.GetAllAsync();

        return list.Select(t => new KeyValuePair<Guid, string>(t.Id, t.Name));
    }

    public static async Task<IEnumerable<KeyValuePair<Guid, string>>> GetValuesForPriority(DataServiceAbstract<TaskPriority> service)
    {
        var list = await service.GetAllAsync();

        return list.Select(t => new KeyValuePair<Guid, string>(t.Id, t.Name));
    }


}
