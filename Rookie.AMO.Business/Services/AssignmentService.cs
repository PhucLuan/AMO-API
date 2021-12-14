using AutoMapper;
using EnsureThat;
using Rookie.AMO.Business.Extensions;
using Rookie.AMO.Business.Interfaces;
using Rookie.AMO.Contracts;
using Rookie.AMO.Contracts.Dtos.Assignment;
using Rookie.AMO.Contracts.Dtos.Filter;
using Rookie.AMO.DataAccessor.Data;
using Rookie.AMO.DataAccessor.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Rookie.AMO.Contracts.Dtos;
using System.Linq.Expressions;

namespace Rookie.AMO.Business.Services
{
    public class AssignmentService : IAssignmentService
    {
        private readonly IBaseRepository<Assignment> _baseRepository;
        private readonly IBaseRepository<Asset> _assetRepository;
        private readonly IMapper _mapper;

        public AssignmentService(IBaseRepository<Assignment> baseRepository, IBaseRepository<Asset> assetRepository, IMapper mapper)
        {
            _baseRepository = baseRepository;
            _assetRepository = assetRepository;
            _mapper = mapper;
        }

        public async Task<AssignmentDto> AddAsync(AssignmentRequest assignmentRequest, Guid adminId)
        {
            Ensure.Any.IsNotNull(assignmentRequest, nameof(assignmentRequest));
            var assignment = _mapper.Map<Assignment>(assignmentRequest);
            assignment.State = State.WaitingAccept;
            assignment.CreatorId = adminId;
            assignment.Pubished = true;
            assignment.CreatedDate = DateTime.Now;
            assignment.UpdatedDate = DateTime.Now;
            var item = await _baseRepository.AddAsync(assignment);
            return _mapper.Map<AssignmentDto>(item);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _baseRepository.DeleteAsync(id);
        }

        public async Task UpdateAsync(Guid id, AssignmentRequest request)
        {

            var assignmentUpdate = _mapper.Map<Assignment>(request);

            var assignment = await _baseRepository.GetByIdAsync(id);
            Ensure.Any.IsNotNull(assignment, nameof(assignment));

            assignment.UserID = assignmentUpdate.UserID;
            assignment.AssetID = assignmentUpdate.AssetID;
            assignment.AssignedDate = assignmentUpdate.AssignedDate;
            assignment.Note = assignmentUpdate.Note;
            assignment.UpdatedDate = DateTime.Now;

            await _baseRepository.UpdateAsync(assignment);
        }

        public async Task<IEnumerable<AssignmentDto>> GetAllAsync()
        {
            var assignments = await _baseRepository.GetAllAsync();
            return _mapper.Map<List<AssignmentDto>>(assignments);
        }

        public async Task<AssignmentDto> GetByIdAsync(Guid id)
        {
            var assignment = await _baseRepository.GetByAsync(x => x.Id == id, "Asset");
            return _mapper.Map<AssignmentDto>(assignment);
        }

        public async Task<IEnumerable<AssignmentDto>> GetByUserIdAsync(Guid userId)
        {
            var res = from assignments in _baseRepository.Entities
                      join asset in _assetRepository.Entities on assignments.AssetID equals asset.Id
                      where assignments.Pubished == true && assignments.UserID == userId && (DateTime.Compare(assignments.AssignedDate.Date, DateTime.Now.Date) <= 0)
                      select new AssignmentDto()
                      {
                          AssetCode = asset.Code,
                          AssetName = asset.Name,
                          AssignedDate = assignments.AssignedDate,
                          AssetSpecification = asset.Specification,
                          StateName = EnumConverExtension.GetDescription<State>(assignments.State),
                          Note = assignments.Note,
                          AssetID = asset.Id,
                          UserID = assignments.UserID,
                          CategoryName = asset.Category.Name,
                          Id = assignments.Id,
                          CreatorId = assignments.CreatorId,
                          RequestAssignmentId = assignments.RequestAssignmentId,
                      };
            var assignmentList = await res.ToListAsync();
            return _mapper.Map<IEnumerable<AssignmentDto>>(assignmentList);
        }

        public async Task<PagedResponseModel<AssignmentDto>> PagedQueryAsync(FilterAssignmentModel filter)
        {

            var query = _baseRepository.Entities.Include(x => x.Asset).AsQueryable();
            query = query.Where(x => x.Pubished == true);
            query = query.Where(x => string.IsNullOrEmpty(filter.KeySearch) || x.Asset.Name.Contains(filter.KeySearch)
                                || x.Asset.Code.Contains(filter.KeySearch) || filter.userFilter.Contains(x.UserID));


            if (!string.IsNullOrEmpty(filter.State))
            {
                IEnumerable<int> stateFilter = filter.State.Trim().Split(' ').Select(s => EnumConverExtension.GetValueInt<State>(s));

                query = query.Where(x => stateFilter.Contains(((int)x.State)));
            }
            if (filter.AssignedDate != null)
            {
                var dateFilter = (DateTime)filter.AssignedDate;
                query = query.Where(x => x.AssignedDate.Date.CompareTo(dateFilter.Date) == 0);
            }


            switch (filter.OrderProperty)
            {
                case "AssetCode":
                    if (filter.Desc)
                        query = query.OrderByDescending(a => a.Asset.Code);
                    else
                        query = query.OrderBy(a => a.Asset.Code);
                    break;
                case "AssetName":
                    if (filter.Desc)
                        query = query.OrderByDescending(a => a.Asset.Name);
                    else
                        query = query.OrderBy(a => a.Asset.Name);
                    break;
                case "":
                    break;
                default:
                    query = query.OrderByPropertyName(filter.OrderProperty, filter.Desc);
                    break;
            }


            var assignments = await query
                .PaginateAsync(filter.Page, filter.Limit);

            return new PagedResponseModel<AssignmentDto>
            {
                CurrentPage = assignments.CurrentPage,
                TotalPages = assignments.TotalPages,
                TotalItems = assignments.TotalItems,
                Items = _mapper.Map<IEnumerable<AssignmentDto>>(assignments.Items)
            };
        }

        public async Task<AssignmentDto> AcceptRespond(Guid id)
        {
            var assignment = await _baseRepository.GetByIdAsync(id);
			Ensure.Any.IsNotNull(assignment, nameof(assignment));
            if(assignment.State != State.Accepted)
            {
                assignment.State = State.Accepted;
                await _baseRepository.UpdateAsync(assignment);
                return _mapper.Map<AssignmentDto>(assignment);
            }
            return null;
        }

        public async Task<IEnumerable<HistoryAssignmentDto>> GetHistoryAssignmentById(Guid assetid)
        {
            var res = await _baseRepository.Entities
                //.Include(x => x.RequestAssignment)
                .Include(x => x.Asset).Where(x => x.Asset.Id == assetid)
                .OrderByDescending(x => x.AssignedDate).Take(3)
                .Select(x => new HistoryAssignmentDto { 
                    AssignedDate = x.AssignedDate,
                    ReturnDate = x.RequestAssignment.ReturnDate,
                    UserAssignedById = x.CreatorId,
                    UserAssignedToId = x.UserID
                }).ToListAsync();
            return res;
        }

        public async Task<bool> ExistAsync(Expression<Func<Assignment, bool>> predicate)
        {
            return await _baseRepository.ExistAsync(predicate);
        }

    }
}
