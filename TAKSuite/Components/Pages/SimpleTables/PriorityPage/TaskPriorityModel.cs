using BlazorBootstrap;
using BlazorReflection.Attributes;
using TAKSuite.Data.Models;
using System.ComponentModel;
using BlazorReflection.Data;
using TAKSuite.Components.Pages.Components.Data;
using TAKSuite.Components.Pages.Components;

public class TaskPriorityModel : BaseEntityViewModel<TaskPriority>, IFormModel<TaskPriority>
{
    public TaskPriorityModel(TaskPriority model) : base(model) { }

    public static IFormModel<TaskPriority> Create(TaskPriority item) => new TaskPriorityModel(item);

    [DisplayName("Nome")]
    public string Name
    {
        get => Model.Name;
        set => Model.Name = value;
    }

    [DisplayName("Livello")]
    public int Level
    {
        get => Model.Level;
        set => Model.Level = value;
    }

    [DisplayName("Colore")]
    public CardColor CardColor
    {
        get => Model.CardColor;
        set => Model.CardColor = value;
    }

    [DisplayName("Predefinita")]
    public bool IsDefault
    {
        get => Model.IsDefault;
        set => Model.IsDefault = value;
    }
}
