using AutoMapper;
using EnsureThat;
using Rookie.AMO.Contracts;
using Rookie.AMO.Contracts.Dtos;
using Rookie.AMO.Contracts.Dtos.Filter;
using Rookie.AMO.DataAccessor.Data;
using Rookie.AMO.DataAccessor.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Rookie.AMO.Business.Interfaces;
using Rookie.AMO.Business.Extensions;

namespace Rookie.AMO.Business.Services
{
    public class AssetService : IAssetService
    {
        private readonly IBaseRepository<Asset> _baseRepository;
        private readonly IBaseRepository<Category> _categoryRepository;
        private readonly IMapper _mapper;

        public AssetService(IBaseRepository<Asset> baseRepository, IBaseRepository<Category> categoryRepository, IMapper mapper)
        {
            _baseRepository = baseRepository;
            _categoryRepository = categoryRepository;
            _mapper = mapper;
		
        }
        public async Task<AssetDto> AddAsync(AssetDto assetRequest)
        {
            Ensure.Any.IsNotNull(assetRequest, nameof(assetRequest));
            var asset = _mapper.Map<Asset>(assetRequest);
            // Generate Asset Code
            asset.Code = await AutoGenerateAssetCode(asset);
            asset.Pubished = true;
            asset.CreatedDate = DateTime.Now;
            asset.UpdatedDate = DateTime.Now;
            asset.Location = assetRequest.Location;// location take in creator location
            var item = await _baseRepository.AddAsync(asset);
            return _mapper.Map<AssetDto>(item);
        }

        public async Task<string> AutoGenerateAssetCode(Asset asset)
        {
            Ensure.Any.IsNotNull(asset, nameof(asset));
            var categoryId = asset.CategoryId;

            var Category = await _categoryRepository.Entities.Include(x=>x.Assets).FirstOrDefaultAsync(x => x.Id == categoryId);
            Ensure.Any.IsNotNull(Category.Prefix, nameof(Category.Prefix));
            //add code category and auto number
            // assetcode = Categoryname + number

            var listAssets = _baseRepository.Entities;

            var result = listAssets.Where(x => x.CategoryId == categoryId);

            if (result.Count() > 0)
            {

                var maxNuber = Category.Assets.OrderByDescending(x => x.Code).First();

                int number = Convert.ToInt32(maxNuber.Code.Substring(2));

                return Category.Prefix + GenerateAutoNumber(number);

            }
            else
            {
                return Category.Prefix + "000001";
            }
        }

        public async Task<IEnumerable<AssetDto>> GetAllAsync()
        {
            var categories = await _baseRepository.GetAllAsync();
            return _mapper.Map<List<AssetDto>>(categories);
        }

        public async Task<AssetDto> GetByIdAsync(Guid id)
        {
            var asset = await _baseRepository.GetByIdAsync(id);
            Ensure.Any.IsNotNull(asset, nameof(asset));
            var res = _mapper.Map<AssetDto>(asset);
            res.State = ((int)asset.State).ToString();
            return res;
        }

        public async Task<AssetDto> UpdateAsync(AssetDto assetDto)
        {
            var assetUpdate = _mapper.Map<Asset>(assetDto);
            Asset asset = await _baseRepository.GetByIdAsync(assetDto.Id);
            Ensure.Any.IsNotNull(asset, nameof(asset));
            asset.Name = assetUpdate.Name;
            asset.Specification = assetUpdate.Specification;
            asset.InstalledDate = assetUpdate.InstalledDate;
            asset.State = assetUpdate.State;
            asset.UpdatedDate = DateTime.Now;
            await _baseRepository.UpdateAsync(asset);
            return _mapper.Map<AssetDto>(asset);
        }
        public async Task SetStateAsync(Guid id, State state)
        {
            var asset = await _baseRepository.GetByIdAsync(id);
            Ensure.Any.IsNotNull(asset, nameof(asset));
            asset.State = state;
            await _baseRepository.UpdateAsync(asset);
        }
        private string GenerateAutoNumber(int number)
         {
            number++;
            string result = number.ToString("000000");
            return result;
         }

		public async Task<ListFilterAsset> GetFilterAssetAsync()
        {
            ListFilterAsset model = new ListFilterAsset();

            //Get category list
            var categories = await _categoryRepository.GetAllAsync();
            var categorySelectList = categories.Select(x => new SelectListItem
            {
                Name = x.Name,
                Id = x.Id.ToString()
            }).ToList();

            model.CategoryList = categorySelectList.ToList();

            var statelist = Enum.GetValues(typeof(State)).Cast<State>().Select(x => new SelectListItem { 
                Name = x.GetDescription<State>(),
                Id = ((int)x).ToString()
            }).ToList();

            model.StateList = statelist;
            return model;
        }

		public async Task<PagedResponseModel<AssetDto>> PagedQueryAsync(FilterAssetModel filter)
        {

            var query = _baseRepository.Entities.Where(x => x.Pubished == true);

            query = query.Where(x => x.Location == filter.Location);

            query = query.Where(x => string.IsNullOrEmpty(filter.KeySearch) 
                || x.Name.ToLower().Contains(filter.KeySearch.ToLower()) 
                || x.Code.ToLower().Contains(filter.KeySearch.ToLower()));

            query = query.Include(x => x.Category);

            if (filter.MustBeAvailable) {
                query = query.Where(x => x.State == State.Available);
            }

            if (!string.IsNullOrEmpty(filter.Category))
            {
                query = query.Where(x => filter.Category.ToLower().Contains(x.Category.Name.ToLower()));
            }

            if (!string.IsNullOrEmpty(filter.State))
            {
                IEnumerable<int> stateFilter = filter.State.Trim().Split(' ').Select(s => EnumConverExtension.GetValueInt<State>(s));

                query = query.Where(x => stateFilter.Contains(((int)x.State)));
            }

            switch (filter.OrderProperty)
            {
                case "code":
                    if (filter.Desc)
                        query = query.OrderByDescending(a => a.Code);
                    else
                        query = query.OrderBy(a => a.Code);
                    break;
                case "name":
                    if (filter.Desc)
                        query = query.OrderByDescending(a => a.Name);
                    else
                        query = query.OrderBy(a => a.Name);
                    break;
                case "categoryName":
                    if (filter.Desc)
                        query = query.OrderByDescending(a => a.Category.Name);
                    else
                        query = query.OrderBy(a => a.Category.Name);
                    break;
                case "state":
                    if (filter.Desc)
                        query = query.OrderByDescending(a => a.State);
                    else
                        query = query.OrderBy(a => a.State);
                    break;
                case "":
                    break;
                default:
                    query = query.OrderByPropertyName(filter.OrderProperty, filter.Desc);
                    break;
            }

            var assets = await query
                .PaginateAsync(filter.Page, filter.Limit);
            List < AssetDto > assetList = new List<AssetDto>();

            foreach(var items in assets.Items)
            {
                AssetDto assetDto = _mapper.Map<AssetDto>(items);
                assetDto.CategoryName = items.Category.Name;
                assetDto.State = ((State)items.State).GetDescription<State>();
                assetList.Add(assetDto);
            }

            return new PagedResponseModel<AssetDto>
            {
                CurrentPage = assets.CurrentPage,
                TotalPages = assets.TotalPages,
                TotalItems = assets.TotalItems,
                Items = assetList,
            };
        }

        public async Task DeleteAsync(Guid id)
        {
            //_context.Assignments.RemoveRange(_context.Assignments.Where(a => a.AssetID == id));
            await _baseRepository.DeleteAsync(id);
        }
    }
}