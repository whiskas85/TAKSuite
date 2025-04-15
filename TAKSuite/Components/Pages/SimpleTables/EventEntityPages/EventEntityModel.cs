using BlazorReflection.Attributes;
using TAKSuite.Data.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using BlazorReflection.Data;
using TAKSuite.Components.Pages.Components.Data;
using TAKSuite.Components.Pages.Components;

namespace TAKSuite.Components.Pages.SimpleTables.EventEntityPages;

public class EventEntityModel : BaseEntityViewModel<EventEntity>, IFormModel<EventEntity>
{
    public EventEntityModel(EventEntity model) : base(model) { }

    public static IFormModel<EventEntity> Create(EventEntity item) => new EventEntityModel(item);



    [Required(ErrorMessage = "Timestamp is required.")]
    public DateTime? Timestamp
    {
        get => Model.Timestamp;
        set => Model.Timestamp = value;
    }

    [Required(ErrorMessage = "Title is required.")]
    [FormControl(FormControlType.Text)]
    public string? Title
    {
        get => Model.Title;
        set => Model.Title = value;
    }

    [FormControl(FormControlType.Textarea)]
    [DisplayName("Note")]
    public string? Note
    {
        get => Model.Note;
        set => Model.Note = value;
    }

}
