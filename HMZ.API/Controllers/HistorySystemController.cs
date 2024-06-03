
using HMZ.API.Controllers.Base;
using HMZ.DTOs.Filters;
using HMZ.DTOs.Queries;
using HMZ.DTOs.Views;
using HMZ.Service.Services.HistorySystemServices;

namespace HMZ.API.Controllers
{
    public class HistorySystemController : CRUDController<IHistorySystemService, HistorySystemQuery, HistorySystemView, HistorySystemFilter>
    {
        public HistorySystemController(IHistorySystemService service) : base(service)
        {
            
        }
    }
}