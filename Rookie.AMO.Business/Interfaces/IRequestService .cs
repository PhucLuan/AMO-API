using Rookie.AMO.Contracts;
using Rookie.AMO.Contracts.Dtos;
using Rookie.AMO.Contracts.Dtos.Filter;
using Rookie.AMO.Contracts.Dtos.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rookie.AMO.Business.Interfaces
{
    public interface IRequestService
    {
        Task<RequestDto> AddAsync(RequestReturn returnRequest);
        Task<RequestDto> GetByIdAsync(Guid id);
        Task<RequestDto> AcceptRespond(Guid id, Guid userId);
        Task<IEnumerable<RequestDto>> GetAllAsync();
        Task<PagedResponseModel<ReturnRequestDto>> PagedQueryAsync(FilterRequestsModel filter);
        Task DeleteAsync(Guid id);
    }
}
