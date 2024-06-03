using HMZ.DTOs.Queries.Base;
using Microsoft.AspNetCore.Mvc;
using HMZ.Service.Services.IBaseService;
using Microsoft.AspNetCore.Authorization;
using HMZ.Service.Helpers;

namespace HMZ.API.Controllers.Base
{
    public abstract class CRUDController<T,TQuery,TView, TFilter>  : BaseController<T>
    where T : IBaseService<TQuery,TView ,TFilter>
    {
        public CRUDController(T service) : base(service)
        {
        }

        [Authorize]
        [HttpPost]
        public virtual async Task<IActionResult> GetAll(BaseQuery<TFilter> query)
        {
            query.PageNumber = query.PageNumber > 0 ? query.PageNumber : 1;
            query.PageSize = query.PageSize > 0 ? query.PageSize : 10;
            var items = await _service.GetPageList(query);
            return Ok(items);
        }
        [Authorize]
        [HttpGet("{id}")]
        public virtual async Task<IActionResult> GetById(string id)
        {
            var user = await _service.GetByIdAsync(id);
            return Ok(user);
        }
        [Authorize]
        [HttpGet("{code}")]
        public virtual async Task<IActionResult> GetByCode(string code)
        {
            var user = await _service.GetByCodeAsync(code);
            return Ok(user);
        }

        [Authorize]
        [HttpPut("{id}")]
        public virtual async Task<IActionResult> Update(string id, TQuery query)
        {
            if (string.IsNullOrEmpty(id))
            {
                return Ok(new DataResult<bool> { Entity = false, Errors = new List<string> { "Id is required" } });
            }
            // set UpdatedBy  = CurrentUser
            string username = User.Identity.Name;
            query.GetType().GetProperty("UpdatedBy").SetValue(query, username);

            var result = await _service.UpdateAsync(query, id);
            return Ok(result);
        }
        [Authorize]
        [HttpPost]
        public virtual async Task<IActionResult> Create(TQuery query)
        {
            if (query == null)
            {
                return Ok(new DataResult<bool> { Entity = false, Errors = new List<string> { "Data is required" } });
            }
            // set CreatedBy  = CurrentUser
            string username = User.Identity.Name;
            query.GetType().GetProperty("CreatedBy").SetValue(query, username);
            var result = await _service.CreateAsync(query);
            return Ok(result);
        }
        [Authorize]
        [HttpPost]
        public virtual async Task<IActionResult> Delete(string[] id)
        {
            if (id == null || id.Length == 0)
            {
                return Ok(new DataResult<bool> { Entity = false, Errors = new List<string> { "Id is required" } });
            }
            var result = await _service.DeleteAsync(id);
            return Ok(result);
        }
    }
}
