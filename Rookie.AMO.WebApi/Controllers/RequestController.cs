using EnsureThat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Rookie.AMO.Business.Interfaces;
using Rookie.AMO.Contracts;
using Rookie.AMO.Contracts.Constants;
using Rookie.AMO.Contracts.Dtos;
using Rookie.AMO.Contracts.Dtos.Filter;
using Rookie.AMO.Contracts.Dtos.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rookie.AMO.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RequestController : ControllerBase
    {
        private readonly IRequestService _requestService;
        private readonly IIdentityProvider _identityProvider;
        public RequestController(IRequestService requestService, IIdentityProvider identityProvider)
        {
            _requestService = requestService;
            _identityProvider = identityProvider;
        }

        [HttpPost]
        public async Task<ActionResult<RequestDto>> CreateAsync([FromBody] RequestReturn returnRequest)
        {
            Ensure.Any.IsNotNull(returnRequest, nameof(returnRequest));
            var request = await _requestService.AddAsync(returnRequest);
            return Created(Endpoints.ReturnRequest, request);
        }

        [Authorize(Policy = "ADMIN_ROLE_POLICY")]
        [HttpPut("accept/{id}")]
        public async Task<ActionResult> AcceptAsync([FromRoute] Guid id)
        {
            var userId = new Guid(User.Claims.FirstOrDefault(x => x.Type == "sub").Value);
            var result = await _requestService.AcceptRespond(id,userId);
            if (result == null)
            {
                return BadRequest();
            }
            return Ok();
        }

        [Authorize(Policy = "ADMIN_ROLE_POLICY")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteRequestAsync([FromRoute] Guid id)
        {
            var returnRequestDto = await _requestService.GetByIdAsync(id);
            Ensure.Any.IsNotNull(returnRequestDto, nameof(returnRequestDto));
            await _requestService.DeleteAsync(id);
            return NoContent();
        }

        [Authorize(Policy = "ADMIN_ROLE_POLICY")]
        [HttpGet("{id}")]
        public async Task<RequestDto> GetByIdAsync(Guid id)
            => await _requestService.GetByIdAsync(id);

        [Authorize(Policy = "ADMIN_ROLE_POLICY")]
        [HttpGet("GetAsync")]
        public async Task<IEnumerable<RequestDto>> GetAsync()
        => await _requestService.GetAllAsync();

        [Authorize(Policy = "ADMIN_ROLE_POLICY")]
        [HttpPost("find")]
        public async Task<PagedResponseModel<ReturnRequestDto>> FindAsync(FilterRequestsModel filterRequestsModel)
        {
            string filterByUser = "";
            if (filterRequestsModel.OrderProperty == "requestedBy" || filterRequestsModel.OrderProperty == "acceptedBy")
            {
                filterByUser = filterRequestsModel.OrderProperty;
                filterRequestsModel.OrderProperty = "";
            }

            //get user list form identity server
            var accessToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
            var userList = await _identityProvider.GetAllUser(accessToken);
            var userIdList = userList.Where(x => string.IsNullOrEmpty(filterRequestsModel.KeySearch)
                || x.UserName.ToLower().Contains(filterRequestsModel.KeySearch.ToLower())).Select(x => x.Id);
            filterRequestsModel.userFilter = userIdList;
            var adminId = new Guid(User.Claims.FirstOrDefault(x => x.Type == "sub").Value);
            filterRequestsModel.AdminId = adminId;
            var requestList = await _requestService.PagedQueryAsync(filterRequestsModel);
            if (requestList.Items == null)
            {
                return requestList;
            }
            foreach (var request in requestList.Items)
            {
                var requestUser = userList.FirstOrDefault(x => x.Id == request.UserRequestId);
                var acceptUser = userList.FirstOrDefault(x => x.Id == request.UserAcceptId);

                request.RequestedBy = requestUser != null ? requestUser.UserName : "";
                request.AcceptedBy = acceptUser != null ? acceptUser.UserName : "";
            }

            switch (filterByUser)
            {
                case "requestedBy":
                    if (filterRequestsModel.Desc)
                        requestList.Items = requestList.Items.OrderByDescending(x => x.RequestedBy);
                    else
                        requestList.Items = requestList.Items.OrderBy(x => x.RequestedBy);
                    break;
                case "acceptedBy":
                    if (filterRequestsModel.Desc)
                        requestList.Items = requestList.Items.OrderByDescending(x => x.AcceptedBy);
                    else
                        requestList.Items = requestList.Items.OrderBy(x => x.AcceptedBy);
                    break;
                case "":
                    break;
                default:
                    break;
            }

            return requestList;


        }
    }
}
