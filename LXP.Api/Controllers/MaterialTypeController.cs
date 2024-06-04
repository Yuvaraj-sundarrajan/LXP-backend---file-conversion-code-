using LXP.Core.IServices;
using LXP.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LXP.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MaterialTypeController : BaseController
    {
        private readonly IMaterialTypeServices _materialTypeServices;   
        public MaterialTypeController(IMaterialTypeServices materialTypeServices) 
        {
            _materialTypeServices = materialTypeServices;
        }

        ///<summary>
        ///getting all materialType and its id
        ///</summary>
        /// ///<response code="200">Success</response>
        ///<response code="500">Internal server Error</response>
        [HttpGet("/lxp/course/materialtype")]
        public IActionResult GetAllMaterialType()
        {
            return Ok(CreateSuccessResponse(_materialTypeServices.GetAllMaterialType()));
        }
    }
}
