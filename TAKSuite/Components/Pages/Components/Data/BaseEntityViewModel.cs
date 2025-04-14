using BlazorBootstrap;
using BlazorReflection.Attributes;
using TAKSuite.Components.Pages.EventEntityComponents;
using TAKSuite.Data.Models;

namespace TAKSuite.Components.Pages.Components.Data
{
    public class BaseEntityViewModel<T> where T : class, IGuidModel, new()
    {
        [Show(false)]
        public T Model { get; set; }


        public BaseEntityViewModel()
        {
            Model = new();
        }
        public BaseEntityViewModel(T model)
        {
            Model = model;
        }
    }
}