using Rookie.AMO.Contracts;
using Rookie.AMO.Contracts.Dtos.Assignment;
using Rookie.AMO.Contracts.Dtos.Filter;
using Rookie.AMO.DataAccessor.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Rookie.AMO.Business.Interfaces
{
    public interface IAssignmentService
    {
        Task<IEnumerable<AssignmentDto>> GetAllAsync();
        Task<PagedResponseModel<AssignmentDto>> PagedQueryAsync(FilterAssignmentModel filter);
        Task<AssignmentDto> GetByIdAsync(Guid id);
        Task<IEnumerable<AssignmentDto>> GetByUserIdAsync(Guid userId);
        Task<AssignmentDto> AddAsync(AssignmentRequest assignmentRequest, Guid adminId);
        Task UpdateAsync(Guid id, AssignmentRequest request);
        Task<AssignmentDto> AcceptRespond(Guid id);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<HistoryAssignmentDto>> GetHistoryAssignmentById(Guid assetid);
        Task<bool> ExistAsync(Expression<Func<Assignment, bool>> predicate);
    }
}
