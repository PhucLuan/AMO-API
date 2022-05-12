using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Rookie.AMO.Business.Interfaces;
using Rookie.AMO.Contracts;
using Rookie.AMO.Contracts.Constants;
using Rookie.AMO.Contracts.Dtos.Assignment;
using Rookie.AMO.Contracts.Dtos.Filter;
using Rookie.AMO.DataAccessor.Entities;

namespace Rookie.AMO.WebApi.Controllers
{
    [EnableCors("MyPolicy")]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AssignmentController : ControllerBase
    {
        private readonly IAssignmentService _assignmentService;
        private readonly IAssetService _assetService;
        private readonly IIdentityProvider _identityProvider;
        public AssignmentController(IAssignmentService assignmentService, IIdentityProvider identityProvider, IAssetService assetService)
        {
            _assignmentService = assignmentService;
            _identityProvider = identityProvider;
            _assetService = assetService;
        }

        [Authorize(Policy = "ADMIN_ROLE_POLICY")]
        [HttpPost]
        public async Task<ActionResult<AssignmentDto>> CreateAsync([FromBody] AssignmentRequest assignmentRequest)
        {
            var adminId = new Guid(User.Claims.FirstOrDefault(x => x.Type == "sub").Value);
            Ensure.Any.IsNotNull(assignmentRequest, nameof(assignmentRequest));
            var assignment = await _assignmentService.AddAsync(assignmentRequest, adminId);
            await _assetService.SetStateAsync(assignment.AssetID, State.Assigned);
            return Created(Endpoints.Assignment, assignment);
        }

        [Authorize(Policy = "ADMIN_ROLE_POLICY")]
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAsync(Guid id, [FromBody] AssignmentRequest request)
        {
            var assignmentDto = await _assignmentService.GetByIdAsync(id);
            Ensure.Any.IsNotNull(assignmentDto, nameof(assignmentDto));
            Ensure.Any.IsNotNull(request, nameof(request));
            if (assignmentDto.AssetID != request.AssetID) {
                await _assetService.SetStateAsync(assignmentDto.AssetID, State.Available);
                await _assetService.SetStateAsync(request.AssetID, State.Assigned);
            }
            await _assignmentService.UpdateAsync(id, request);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAssignmentAsync([FromRoute] Guid id)
        {
            var assignmentDto = await _assignmentService.GetByIdAsync(id);
            Ensure.Any.IsNotNull(assignmentDto, nameof(assignmentDto));
            await _assignmentService.DeleteAsync(id);
            await _assetService.SetStateAsync(assignmentDto.AssetID, State.Available);
            return NoContent();
        }

        [Authorize(Policy = "ADMIN_ROLE_POLICY")]
        [HttpGet("{id}")]
        public async Task<AssignmentDto> GetByIdAsync(Guid id)
        {
            //get user list form identity server
            var accessToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
            var userList = await _identityProvider.GetAllUser(accessToken);

            var assignment = await _assignmentService.GetByIdAsync(id);
            if (assignment == null) return null;

            var assignUser = assignment.CreatorId != null ? userList.FirstOrDefault(x => x.Id == assignment.CreatorId) : null;
            var assignedUser = userList.FirstOrDefault(x => x.Id == assignment.UserID);

            assignment.AssignedTo = assignedUser != null ? assignedUser.UserName : "";
            assignment.AssignedBy = assignUser != null ? assignUser.UserName : "";

            return assignment;
        }

        [Authorize(Policy = "ADMIN_ROLE_POLICY")]
        [HttpGet("Gethistory")]
        public async Task<IActionResult> GetHistoryAsync([FromQuery] string assetid)
        {
            var historyAssignment = await _assignmentService.GetHistoryAssignmentById(new Guid(assetid));
            var listUserId = historyAssignment.Select(x => x.UserAssignedById.ToString()).ToList()
                                .Union(historyAssignment.Select(x => x.UserAssignedToId.ToString()).ToList())
                                .ToList();
            //get user list form identity server
            var accessToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");


            //var userList = await _identityProvider.GetUserbyListId(listUserId, accessToken);
            var userList = await _identityProvider.GetAllUser(accessToken);
            foreach (var item in historyAssignment)
            {
                var UserAssignedBy = userList.FirstOrDefault(x => x.Id == item.UserAssignedById);
                item.AssignedBy = UserAssignedBy != null ? UserAssignedBy.UserName : "";
                var UserAssignedTo = userList.FirstOrDefault(x => x.Id == item.UserAssignedToId);
                item.AssignedTo = UserAssignedTo != null ? UserAssignedTo.UserName : "";
            }
            return Ok(historyAssignment);
        }

        [HttpGet("user/{userId}")]
        public async Task<IEnumerable<AssignmentDto>> GetByUserIdAsync(Guid userId)
        {
            var accessToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
            var userList = await _identityProvider.GetAllUser(accessToken);
            var assignmentList = await _assignmentService.GetByUserIdAsync(userId);

            foreach (var assignment in assignmentList)
            {
                var assignUser = assignment.CreatorId != null ? userList.FirstOrDefault(x => x.Id == assignment.CreatorId) : null;
                var assignedUser = userList.FirstOrDefault(x => x.Id == assignment.UserID);

                assignment.AssignedTo = assignedUser != null ? assignedUser.UserName : "";
                assignment.AssignedBy = assignUser != null ? assignUser.UserName : "";
            }

            return assignmentList;
        }

        [HttpGet]
        [Authorize(Policy = "ADMIN_ROLE_POLICY")]
        public async Task<IEnumerable<AssignmentDto>> GetAsync()
            => await _assignmentService.GetAllAsync();

        [Authorize(Policy = "ADMIN_ROLE_POLICY")]
        [HttpPost("find")]
        public async Task<PagedResponseModel<AssignmentDto>> FindAsync(FilterAssignmentModel filterAssignmentsModel)
        {
            string filterByUser = "";
            if (filterAssignmentsModel.OrderProperty == "AssignedTo" || filterAssignmentsModel.OrderProperty == "AssignedBy") {
                filterByUser = filterAssignmentsModel.OrderProperty;
                filterAssignmentsModel.OrderProperty = "";
            }

            //get user list form identity server
            var accessToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
            var userList = await _identityProvider.GetAllUser(accessToken);

            var userIdList = userList.Where(x => string.IsNullOrEmpty(filterAssignmentsModel.KeySearch) 
                || x.UserName.ToLower().Contains(filterAssignmentsModel.KeySearch.ToLower())).Select(x => x.Id);

            filterAssignmentsModel.userFilter = userIdList;

            //get assignment list
            var assignmentList = await _assignmentService.PagedQueryAsync(filterAssignmentsModel);


            foreach (var assignment in assignmentList.Items) {
                var assignUser = assignment.CreatorId != null? userList.FirstOrDefault(x => x.Id == assignment.CreatorId) : null;
                var assignedUser = userList.FirstOrDefault(x => x.Id == assignment.UserID);

                assignment.AssignedTo = assignedUser != null ? assignedUser.UserName : "";
                assignment.AssignedBy = assignUser != null ? assignUser.UserName : "";
            }

            switch (filterByUser)
            {
                case "AssignedTo":
                    if (filterAssignmentsModel.Desc)
                        assignmentList.Items = assignmentList.Items.OrderByDescending(x => x.AssignedTo);
                    else
                        assignmentList.Items = assignmentList.Items.OrderBy(x => x.AssignedTo);
                    break;
                case "AssignedBy":
                    if (filterAssignmentsModel.Desc)
                        assignmentList.Items = assignmentList.Items.OrderByDescending(x => x.AssignedTo);
                    else
                        assignmentList.Items = assignmentList.Items.OrderBy(x => x.AssignedTo);
                    break;
                case "":
                    break;
                default:
                    break;
            }

            return assignmentList;
        }

        [HttpPut("accept/{id}")]
        public async Task<ActionResult> AcceptAsync([FromRoute] Guid id)
        {
            var result = await _assignmentService.AcceptRespond(id);
            if (result == null)
            {
                return BadRequest();
            }
            return Ok();
        }

        [Authorize(Policy = "ADMIN_ROLE_POLICY")]
        [HttpGet("CheckRelation/{userId}")]
        public async Task<IActionResult> IsRelatedToUserAsync(Guid userId)
        {
            var assignments = await _assignmentService.GetByUserIdAsync(userId);
            Ensure.Any.IsNotNull(assignments, nameof(assignments));
            var related = assignments.Count() != 0;
            if (related) return Ok(true);
            else return NoContent();
        }

    }
}
