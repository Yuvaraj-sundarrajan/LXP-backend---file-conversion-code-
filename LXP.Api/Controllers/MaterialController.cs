using LXP.Core.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using LXP.Common.ViewModels;



using LXP.Common.Constants;
using System.Net;
using LXP.Common.Entities;
using LXP.Core.Services;
namespace LXP.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MaterialController : BaseController
    {
        private readonly IMaterialServices _materialService;
        public MaterialController(IMaterialServices materialService)
        {
            _materialService = materialService;
        }

        ///<summary>
        /// adding new material
        ///</summary>
        ///  ///<response code="200">Success</response>
        ///<response code="422">Sourse is already exists</response>
        ///<response code="500">Internal server Error</response>

        [HttpPost("/lxp/course/material")]
        public async Task<IActionResult> AddMaterial([FromForm]MaterialViewModel material)
        {
            MaterialListViewModel createdMaterial = await _materialService.AddMaterial(material);
            if (createdMaterial!=null)
            {
                return Ok(CreateInsertResponse(createdMaterial));
            }
            else
            {
                return Ok(CreateFailureResponse(MessageConstants.MsgAlreadyExists, (int)HttpStatusCode.PreconditionFailed));

            }
        }
        ///<summary>
        /// getting all material details using topicId and materialTypeId
        ///</summary>
        /// ///<response code="200">Success</response>
        ///<response code="500">Internal server Error</response>

        [HttpGet("/lxp/course/topic/{topicId}/materialtype/{materialTypeId}/")]
        public async Task<IActionResult> GetAllMaterialDetailsByTopicAndMaterialType(string topicId,string materialTypeId)
        {
            List<MaterialListViewModel> materialLists= await _materialService.GetAllMaterialDetailsByTopicAndType(topicId,materialTypeId) ;
            return Ok(CreateSuccessResponse(materialLists));
        }
        ///<summary>
        /// updating the material
        ///</summary>
        ///  ///<response code="200">Success</response>
        ///<response code="422">Sourse is already exists</response>
        ///<response code="500">Internal server Error</response>

        [HttpPut("/lxp/course/material")]
        public async Task<IActionResult> UpdateMaterial([FromForm]MaterialUpdateViewModel material)
        {
            bool isMaterialUpdated = await _materialService.UpdateMaterial(material);

            if (isMaterialUpdated)
            {
                return Ok(CreateSuccessResponse());
            }

            return Ok(CreateFailureResponse(MessageConstants.MsgNotUpdated, (int)HttpStatusCode.MethodNotAllowed));
        }
        ///<summary>
        /// deleting the material using materialId
        ///</summary>
        ///        /// ///<response code="200">Success</response>
        ///<response code="500">Internal server Error</response>
        [HttpDelete("/lxp/course/material/{materialId}")]
        public async Task<IActionResult> DeleteCourseMaterial(string materialId)
        {
            bool deletedStatus = await _materialService.SoftDeleteMaterial(materialId);
            if (deletedStatus)
            {
                return Ok(CreateSuccessResponse());
            }
            return Ok(CreateFailureResponse(MessageConstants.MsgNotDeleted, (int)HttpStatusCode.MethodNotAllowed));
        }
        ///<summary>
        /// getting the particular material using materialId
        ///</summary>
        /// ///<response code="200">Success</response>
        ///<response code="500">Internal server Error</response>


        [HttpGet("/lxp/course/material/{materialId}")]
        public async Task<IActionResult> GetMaterialByMaterialId(string materialId)
        {
            var material = await _materialService.GetMaterialDetailsByMaterialId(materialId);
            return Ok(CreateSuccessResponse(material));
        }

    }
}

