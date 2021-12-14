using AutoMapper;
using EnsureThat;
using Microsoft.EntityFrameworkCore;
using Rookie.AMO.Business.Extensions;
using Rookie.AMO.Business.Interfaces;
using Rookie.AMO.Contracts;
using Rookie.AMO.Contracts.Dtos;
using Rookie.AMO.Contracts.Dtos.Filter;
using Rookie.AMO.Contracts.Dtos.Request;
using Rookie.AMO.DataAccessor.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rookie.AMO.Business.Services
{
    public class RequestService : IRequestService
    {
        private readonly IBaseRepository<RequestAssignment> _baseRepository;
        private readonly IBaseRepository<Assignment> _assignmentRepository;
        private readonly IBaseRepository<Asset> _assetRepository;
        private readonly IMapper _mapper;

        public RequestService(IBaseRepository<RequestAssignment> baseRepository, IBaseRepository<Assignment> assignmentRepository, IBaseRepository<Asset> assetRepository, IMapper mapper)
        {
            _baseRepository = baseRepository;
            _assignmentRepository = assignmentRepository;
            _assetRepository = assetRepository;
            _mapper = mapper;
        }

        public async Task<RequestDto> AddAsync(RequestReturn returnRequest)
        {
            Ensure.Any.IsNotNull(returnRequest, nameof(returnRequest));
            var request = _mapper.Map<RequestAssignment>(returnRequest);
            request.UserRequestId = returnRequest.UserId;
            request.State = State.WaitingForReturning;
            var item = await _baseRepository.AddAsync(request);

            var assignment = await _assignmentRepository.GetByIdAsync(returnRequest.Id);
            assignment.RequestAssignmentId = item.Id;
            await _assignmentRepository.UpdateAsync(assignment);

            return _mapper.Map<RequestDto>(item);
        }

        public async Task<RequestDto> AcceptRespond(Guid id,Guid userId)
        {
            var request = await _baseRepository.GetByIdAsync(id);
            request.State = State.Completed;
            request.UserAcceptId = userId;
            request.ReturnDate = DateTime.Now;

            var assignment = _assignmentRepository.Entities.Where(x => x.RequestAssignmentId == id).FirstOrDefault();
            assignment.Pubished = false;
            var asset = await _assetRepository.GetByIdAsync(assignment.AssetID);
            asset.State = State.Available;

            await _assignmentRepository.UpdateAsync(assignment);
            await _baseRepository.UpdateAsync(request);
            await _assetRepository.UpdateAsync(asset);

            return _mapper.Map<RequestDto>(request);
        }

        public async Task<RequestDto> GetByIdAsync(Guid id)
        {
            var request = await _baseRepository.GetByIdAsync(id);
            return _mapper.Map<RequestDto>(request);
        }

        public async Task<IEnumerable<RequestDto>> GetAllAsync()
        {
            var assignments = await _baseRepository.GetAllAsync();
            return _mapper.Map<List<RequestDto>>(assignments);
        }

        public async Task<PagedResponseModel<ReturnRequestDto>> PagedQueryAsync(FilterRequestsModel filter)
        {
            var query = _baseRepository.Entities;
            var checknull = await query.AnyAsync();
            if (checknull == false)
            {
                //var test = new IEnumerable<ReturnRequestDto>();
                return new PagedResponseModel<ReturnRequestDto>
                {
                    CurrentPage = 1,
                    TotalPages = 1,
                    TotalItems = 0,
                    Items = null
                };
            }
            var returnRequestQuery = query.Include(x => x.Assignment)
                .ThenInclude(x => x.Asset).Select(x => new ReturnRequestDto
                {
                    AssetCode = x.Assignment.Asset.Code,
                    AssetName = x.Assignment.Asset.Name,
                    Id = x.Id,
                    ReturnDate = x.ReturnDate,
                    AssignedDate = x.Assignment.AssignedDate,
                    State = x.State,
                    UserRequestId = x.UserRequestId,
                    UserAcceptId = x.UserAcceptId,
                });




            returnRequestQuery = returnRequestQuery.Where(x => string.IsNullOrEmpty(filter.KeySearch) 
                                || x.AssetName.Contains(filter.KeySearch)
                                || x.AssetCode.Contains(filter.KeySearch)
                                || filter.userFilter.Contains(x.UserAcceptId)
                                || filter.userFilter.Contains(x.UserRequestId));
            
            //returnRequestQuery = returnRequestQuery.Where(x => x.UserRequestId.Contains(filter.KeySearch));


            if (!string.IsNullOrEmpty(filter.State))
            {
                IEnumerable<int> stateFilter = filter.State.Trim().Split(' ').Select(s => EnumConverExtension.GetValueInt<State>(s));

                returnRequestQuery = returnRequestQuery.Where(x => stateFilter.Contains(((int)x.State)));
            }
            if (filter.ReturnDate != null)
            {
                returnRequestQuery = returnRequestQuery.Where(x => x.ReturnDate.Value.Date.CompareTo(filter.ReturnDate.Value.Date) == 0);
            }


            switch (filter.OrderProperty)
            {
                case "assetCode":
                    if (filter.Desc)
                        returnRequestQuery = returnRequestQuery.OrderByDescending(a => a.AssetCode);
                    else
                        returnRequestQuery = returnRequestQuery.OrderBy(a => a.AssetCode);
                    break;
                case "assetName":
                    if (filter.Desc)
                        returnRequestQuery = returnRequestQuery.OrderByDescending(a => a.AssetName);
                    else
                        returnRequestQuery = returnRequestQuery.OrderBy(a => a.AssetName);
                    break;
                case "returnDate":
                    if (filter.Desc)
                        query = query.OrderByDescending(a => a.ReturnDate.Value.Date)
                            .ThenBy(x => x.ReturnDate.Value.TimeOfDay);
                    else
                        query = query.OrderBy(a => a.ReturnDate.Value.Date)
                            .ThenBy(x => x.ReturnDate.Value.TimeOfDay);
                    break;
                case "":
                    break;
                default:
                    //returnRequestQuery = returnRequestQuery.OrderByPropertyName(filter.OrderProperty, filter.Desc);
                    break;
            }



            var requests = await returnRequestQuery
                .PaginateAsync(filter.Page, filter.Limit);

           


            return new PagedResponseModel<ReturnRequestDto>
            {
                CurrentPage = requests.CurrentPage,
                TotalPages = requests.TotalPages,
                TotalItems = requests.TotalItems,
                Items = _mapper.Map<IEnumerable<ReturnRequestDto>>(requests.Items)
            };
        }

        public async Task DeleteAsync(Guid id)
        {
            var assignment = _assignmentRepository.Entities.Where(x => x.RequestAssignmentId == id).FirstOrDefault();
            assignment.RequestAssignmentId = null;
            await _assignmentRepository.UpdateAsync(assignment);
            //var request = await _baseRepository.GetByIdAsync(id);
            await _baseRepository.DeleteAsync(id);
        }
    }
}
