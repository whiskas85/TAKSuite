using BlazorReflection.Attributes;
using TAKSuite.Data.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using BlazorReflection.Data;
using TAKSuite.Components.Pages.Components.Data;
using TAKSuite.Components.Pages.Components;

namespace TAKSuite.Components.Pages.EventEntityComponents;

public class RadioChannelModel : BaseEntityViewModel<RadioChannel>, IFormModel<RadioChannel>
{
    public RadioChannelModel(RadioChannel model) : base(model) { }

    public static IFormModel<RadioChannel> Create(RadioChannel item) => new RadioChannelModel(item);


    public string? Name
    {
        get => Model.Name;
        set => Model.Name = value;
    }

    [DisplayName("Frequenza")]
    public string? Frequency
    {
        get => Model.Frequency;
        set => Model.Frequency = value;
    }
    
    
    [DisplayName("Tipo frequenza")]
    public string? FrequencyType
    {
        get => Model.FrequencyType;
        set => Model.FrequencyType = value;
    }

}
