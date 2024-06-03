using HMZ.DTOs.Filters;
using HMZ.DTOs.Queries;
using HMZ.DTOs.Queries.Base;
using HMZ.DTOs.Views;
using HMZ.Service.Helpers;
using HMZ.Service.Services.IBaseService;

namespace HMZ.Service.Services.DocumentServices
{
    public interface IDocumentService: IBaseService<DocumentQuery, DocumentView, DocumentFilter>
    {
        Task<DataResult<DocumentView>> GetByClassPageList(BaseQuery<DocumentFilter> query);
        Task<DataResult<DocumentView>> GetBySubjectPageList(BaseQuery<DocumentFilter> query);
    }
}