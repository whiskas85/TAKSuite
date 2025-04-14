using TAKSuite.Components.Pages.Components.Data;
using TAKSuite.Data.Models;

namespace TAKSuite.Components.Pages.Components
{
    public interface IFormModel<TItem> where TItem : IGuidModel, new()
    {

        TItem Model { get; set; }

        static abstract IFormModel<TItem> Create(TItem item);
    }
}
