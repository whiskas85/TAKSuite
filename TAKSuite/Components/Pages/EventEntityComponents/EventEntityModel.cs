using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Org.BouncyCastle.Security;
using System.ComponentModel.DataAnnotations;
using TAKSuite.Data.Models;

namespace TAKSuite.Components.Pages.EventEntityComponents
{
    public class EventEntityModel: BaseEntityViewModel<EventEntity>
    {
        public EventEntityModel(DataServiceAbstract<EventEntity> service) : base(service)
        {
        }
        public EventEntityModel(DataServiceAbstract<EventEntity> service, EventEntity model) : base(service, model)
        {
        }

        [Required(ErrorMessage = "Timestamp is required.")]
        public DateTime? Timestamp
        {
            get
            {
                return _model.Timestamp;
            }
            set
            {
                _model.Timestamp = value;
            }
        }

        [Required(ErrorMessage = "Title is required.")]
        public string? Title
        {
            get
            {
                return _model.Title;
            }
            set
            {
                _model.Title = value;
            }
        }

        public string? Note
        {
            get
            {
                return _model.Note;
            }
            set
            {
                _model.Note = value;
            }
        }



        

    }
}
