using BlazorBootstrap;
using TAKSuite.Data.Models;

namespace TAKSuite.Components.Pages.EventEntityComponents
{
    public abstract class BaseEntityViewModel<T> where T : class, IGuidModel, new()
    {
        internal readonly T _model;
        private readonly DataServiceAbstract<T> _service;

        public BaseEntityViewModel(DataServiceAbstract<T> service)
        {
            _model = new();
            _service = service;
        }
        public BaseEntityViewModel(DataServiceAbstract<T> service, T model)
        {
            _model = model;
            _service = service;
        }


        public async Task Save()
        {
            await _service.AddOrUpdateAsync(_model);

        }
    }
}