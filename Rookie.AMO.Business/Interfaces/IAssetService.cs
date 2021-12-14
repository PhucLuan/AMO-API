using Rookie.AMO.Contracts;
using Rookie.AMO.Contracts.Dtos;
using Rookie.AMO.Contracts.Dtos.Filter;
using Rookie.AMO.DataAccessor.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rookie.AMO.Business.Interfaces
{
    public interface IAssetService
    {
        Task<IEnumerable<AssetDto>> GetAllAsync();
        Task<ListFilterAsset> GetFilterAssetAsync();//Get list item for filter
        Task<PagedResponseModel<AssetDto>> PagedQueryAsync(FilterAssetModel filter);
        Task<AssetDto> GetByIdAsync(Guid id);
        Task<string> AutoGenerateAssetCode(Asset asset);
        Task<AssetDto> UpdateAsync(AssetDto request);

        Task<AssetDto> AddAsync(AssetDto assetRequest);
        Task DeleteAsync(Guid id);
        Task SetStateAsync(Guid id, State state);
    }
}
