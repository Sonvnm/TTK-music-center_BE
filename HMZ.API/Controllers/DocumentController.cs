
using HMZ.API.Controllers.Base;
using HMZ.DTOs.Filters;
using HMZ.DTOs.Queries;
using HMZ.DTOs.Queries.Base;
using HMZ.DTOs.Views;
using HMZ.Service.Extensions;
using HMZ.Service.Services.DocumentServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HMZ.API.Controllers
{
    public class DocumentController : CRUDController<IDocumentService, DocumentQuery, DocumentView, DocumentFilter>
    {
        public DocumentController(IDocumentService service) : base(service)
        {

        }

        [Authorize(Roles = "Teacher,Admin,Member")]
        [HttpPost]
        public async Task<IActionResult> GetBySubject(BaseQuery<DocumentFilter> query)
        {
            query.PageNumber = query.PageNumber > 0 ? query.PageNumber : 1;
            query.PageSize = query.PageSize > 0 ? query.PageSize : 10;
            var items = await _service.GetBySubjectPageList(query);
            return Ok(items);
        }

        [Authorize(Roles = "Teacher,Admin,Member")]
        [HttpPost]
        public async Task<IActionResult> GetByClass(BaseQuery<DocumentFilter> query)
        {
            query.PageNumber = query.PageNumber > 0 ? query.PageNumber : 1;
            query.PageSize = query.PageSize > 0 ? query.PageSize : 10;
            var items = await _service.GetByClassPageList(query);
            return Ok(items);
        }

        [Authorize]
        [HttpPost,DisableRequestSizeLimit]
        public async Task<IActionResult> UploadDocument(string classCode)
        {
            // get file from request 
            var file = Request.Form.Files[0];
            var query = new DocumentQuery
            {
                File = file,
                ClassCode = classCode
            };
            var result = await _service.CreateAsync(query);
            return Ok(result);
        }

        [Authorize]
        [HttpPost, DisableRequestSizeLimit]
        public async Task<IActionResult> UploadDocumentForSubject(string subjectCode)
        {
            // get file from request 
            var file = Request.Form.Files[0];
            var query = new DocumentQuery
            {
                File = file,
                SubjectCode = subjectCode
            };
            var result = await _service.CreateAsync(query);
            return Ok(result);
        }
    }
}