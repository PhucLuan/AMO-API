using EnsureThat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rookie.AMO.Business.Interfaces;
using Rookie.AMO.Contracts;
using Rookie.AMO.Contracts.Constants;
using Rookie.AMO.Contracts.Dtos.Filter;
using Rookie.AMO.Contracts.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rookie.AMO.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "ADMIN_ROLE_POLICY")]
    public class AssetController : ControllerBase
    {
        private readonly IAssetService _assetService;
        private readonly IAssignmentService _assignmentService;
        public AssetController(IAssetService assetService, IAssignmentService assignmentService)
        {
            _assetService = assetService;
            _assignmentService = assignmentService;
        }

        [HttpPost]
        public async Task<ActionResult<AssetDto>> CreateAsync([FromBody] AssetDto assetDto)
        {
            Ensure.Any.IsNotNull(assetDto, nameof(assetDto));
            var adminLocation = User.Claims.FirstOrDefault(x => x.Type == "location").Value; assetDto.Location = adminLocation;
            var asset = await _assetService.AddAsync(assetDto);
            return Created(Endpoints.Asset, asset);
        }
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAssetAsync([FromRoute] Guid id)
        {
            var checkexistinAssignment = await _assignmentService.ExistAsync(x => x.AssetID == id);
            if (checkexistinAssignment)
            {
                return Content("existed");
            }
            var assetDto = await _assetService.GetByIdAsync(id);
            Ensure.Any.IsNotNull(assetDto, nameof(assetDto));
            await _assetService.DeleteAsync(id);
            return NoContent();
        }

        [HttpGet("IsAssetExitInAssignmentAsync/{id}")]
        public async Task<ActionResult> IsAssetExitInAssignmentAsync([FromRoute] Guid id)
        {
            var checkexistinAssignment = await _assignmentService.ExistAsync(x => x.AssetID == id);
            if (checkexistinAssignment)
            {
                return Content("true");
            }
            return Content("false");
        }

        [HttpGet("{id}")]
        public async Task<AssetDto> GetByIdAsync(Guid id)
            => await _assetService.GetByIdAsync(id);

        [HttpPut]
        public async Task<ActionResult> UpdateAsync([FromBody] AssetDto assetDto)
        {
            Ensure.Any.IsNotNull(assetDto, nameof(assetDto));
            //Ensure.Any.IsNotNull(assetDto.Id, nameof(assetDto.Id));
            await _assetService.UpdateAsync(assetDto);
            return NoContent();
        }

        [HttpGet("GetAsync")]
        public async Task<IEnumerable<AssetDto>> GetAsync()
        => await _assetService.GetAllAsync();

        [HttpGet("GetFilterAssetAsync")]
        public async Task<ListFilterAsset> GetFilterProductAsync()
        {
            return await _assetService.GetFilterAssetAsync();
        }

        [HttpPost("find")]
        public async Task<PagedResponseModel<AssetDto>> FindAsync(FilterAssetModel filter)
        {
            var adminLocation = User.Claims.FirstOrDefault(x => x.Type == "location").Value;
            filter.Location = adminLocation;
            return await _assetService.PagedQueryAsync(filter);
        }

        [HttpPost("find/available")]
        public async Task<PagedResponseModel<AssetDto>> FindAvailableAsync(FilterAssetModel filter) {
            var adminLocation = User.Claims.FirstOrDefault(x => x.Type == "location").Value;
            filter.Location = adminLocation;
            filter.MustBeAvailable = true;
            return await _assetService.PagedQueryAsync(filter);
        }
    }
}
