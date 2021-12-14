using AutoMapper;
using FluentAssertions;
using Moq;
using Rookie.AMO.Business;
using Rookie.AMO.Business.Interfaces;
using Rookie.AMO.Business.Services;
using Rookie.AMO.DataAccessor.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using MockQueryable.Moq;
using Rookie.AMO.Contracts.Dtos.Filter;
using Rookie.AMO.Contracts.Dtos;

namespace Rookie.AMO.UnitTests.Business
{
    /// <summary>
    /// https://codewithshadman.com/repository-pattern-csharp/
    /// https://dotnettutorials.net/lesson/generic-repository-pattern-csharp-mvc/
    /// https://fluentassertions.com/exceptions/
    /// https://stackoverflow.com/questions/37422476/moq-expression-with-constraint-it-isexpressionfunct-bool
    /// </summary>
    public class AssetServiceShould
    {
        private readonly AssetService _assetService;
        private readonly Mock<IBaseRepository<Category>> _categoryRepository;
        private readonly Mock<IBaseRepository<Asset>> _assetRepository;
        private readonly IMapper _mapper;

        public AssetServiceShould()
        {
            _categoryRepository = new Mock<IBaseRepository<Category>>();
            _assetRepository = new Mock<IBaseRepository<Asset>>();

            var config = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperProfile>());
            _mapper = config.CreateMapper();

            _assetService = new AssetService(
                _assetRepository.Object,
                _categoryRepository.Object,
                _mapper
            );
        }

        [Fact]
        public async Task GetAsyncShouldThrowExceptionAsync()
        {
            var id = Guid.NewGuid();
            _assetRepository
                  .Setup(x => x.GetByIdAsync(id))
                  .Returns(Task.FromResult<Asset>(null));
            Func<Task> act = async () => await _assetService.GetByIdAsync(It.IsAny<Guid>());
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task GetAsyncShouldReturnObjectAsync()
        {
            var asset = new Asset()
            {
                Id = Guid.NewGuid(),
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                Pubished = true,
                Code = $"LP0001",
                Name = $"DELL G1",
                Location = "HN",
                Specification = $"this is G1",
                CategoryId = Guid.NewGuid(),
                InstalledDate = DateTime.Now.AddDays(-5)
            };

            _assetRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).Returns(Task.FromResult(asset));
            var result = await _assetService.GetByIdAsync(It.IsAny<Guid>());
            result.Should().NotBeNull();
            result.Id.Should().Be(asset.Id);

            _assetRepository.Verify(mock => mock.GetByIdAsync(It.IsAny<Guid>()), Times.Once);
        }

        [Fact]
        public async Task GetAllAsyncShouldReturnEmptyListAsync()
        {
            List<Asset> emptyList = new List<Asset>();
            _assetRepository
                  .Setup(x => x.GetAllAsync())
                  .Returns(Task.FromResult(emptyList.AsEnumerable()));

            var result = await _assetService.GetAllAsync();
            result.Should().HaveCount(0);
            _assetRepository.Verify(mock => mock.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllAsyncShouldReturnObjectAsync()
        {
            var assetList = new List<Asset>();

            for (int i = 0; i < 5; i++) {
                assetList.Add(
                    new Asset()
                    {
                        Id = Guid.NewGuid(),
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now,
                        Pubished = true,
                        Code = $"LP000{i}",
                        Name = $"DELL G{i}",
                        Location = "HN",
                        Specification = $"this is {i}",
                        CategoryId = Guid.NewGuid(),
                        InstalledDate = DateTime.Now.AddDays(-5)
                    }
                );
            }

            _assetRepository.Setup(x => x.GetAllAsync()).Returns(Task.FromResult(assetList.AsEnumerable()));
            var result = await _assetService.GetAllAsync();

            result.Should().HaveCount(5);

            _assetRepository.Verify(mock => mock.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task AddAssetShouldThrowExceptionAsync()
        {
            Func<Task> act = async () => await _assetService.AddAsync(null);
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task AddAssetShouldBeSuccessfullyAsync()
        {
            List<Category> categoryList = new List<Category>() {
                new Category()
                {
                    Id = Guid.NewGuid(),
                    Name = "Name",
                    Prefix = "LT"
                }
            };
            var asset = new Asset()
            {
                Id = Guid.NewGuid(),
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                Pubished = true,
                Code = $"LP0001",
                Name = $"DELL G1",
                Location = "HN",
                Specification = $"this is G1",
                CategoryId = Guid.NewGuid(),
                InstalledDate = DateTime.Now.AddDays(-5),
                State = State.Available,
                Category = categoryList.FirstOrDefault()
            };
            var assetState = EnumConverExtension.GetValueInt<State>(asset.State.ToString()).ToString();
            var assetDto = new AssetDto() { 
                Name = asset.Name,
                CategoryId = asset.Category.Id,
                Specification = asset.Specification,
                InstalledDate = asset.InstalledDate,
                State = assetState

            };
            var mockCategory = categoryList.AsQueryable().BuildMock();

            _categoryRepository.Setup(x => x.Entities).Returns(mockCategory.Object);

            _assetRepository.Setup(x => x.AddAsync(It.IsAny<Asset>())).Returns(Task.FromResult(asset));
            var result = await _assetService.AddAsync(assetDto);

            result.Should().NotBeNull();

            _assetRepository.Verify(mock => mock.AddAsync(It.IsAny<Asset>()), Times.Once());
            _categoryRepository.Verify(mock => mock.Entities, Times.Once());
        }

        [Fact]
        public void AutoGenerateAssetCodeShouldBeThrowNullExceptionAsync()
        {
            Func<Task<string>> act = async () => await _assetService.AutoGenerateAssetCode(null);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void AutoGenerateAssetCodeShouldBeThrowExceptionAsync()
        {
            var categoryId = Guid.NewGuid();
            List<Category> categoryList = new List<Category>() {
                new Category()
                {
                    Id = categoryId,
                    Name = "Name"
                }
            };
            var asset = new Asset()
            {
                Id = Guid.NewGuid(),
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                Pubished = true,
                Name = $"DELL G1",
                Location = "HN",
                Specification = $"this is G1",
                CategoryId = categoryId,
                InstalledDate = DateTime.Now.AddDays(-5),
                State = State.Available,
                Category = categoryList.FirstOrDefault()
            };
            var mockCategory = categoryList.AsQueryable().BuildMock();
            _categoryRepository.Setup(x => x.Entities).Returns(mockCategory.Object);

            Func<Task<string>> act = async () => await _assetService.AutoGenerateAssetCode(asset);
            act.Should().Throw<ArgumentNullException>();

            _categoryRepository.Verify(mock => mock.Entities, Times.Once());
        }

        [Fact]
        public async void AutoGenerateAssetCodeShouldReturnCodeWithCorrectFormatForFirstCodeAsync()
        {
            var categoryId = Guid.NewGuid();
            List<Category> categoryList = new List<Category>() {
                new Category()
                {
                    Id = categoryId,
                    Name = "Name",
                    Prefix = "LT"
                }
            };
            var asset = new Asset()
            {
                Id = Guid.NewGuid(),
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                Pubished = true,
                Name = $"DELL G1",
                Location = "HN",
                Specification = $"this is G1",
                CategoryId = categoryId,
                InstalledDate = DateTime.Now.AddDays(-5),
                State = State.Available,
                Category = categoryList.FirstOrDefault()
            };
            var mockCategory = categoryList.AsQueryable().BuildMock();

            _categoryRepository.Setup(x => x.Entities).Returns(mockCategory.Object);

            var result = await _assetService.AutoGenerateAssetCode(asset);

            result.Should().NotBeNull();
            result.Should().Be("LT000001");

            _categoryRepository.Verify(mock => mock.Entities, Times.Once());
        }

        [Fact]
        public async void AutoGenerateAssetCodeShouldReturnCodeWithCorrectFormatForNextCodeAsync()
        {
            var categoryId = Guid.NewGuid();
            List<Asset> assetEntity = new List<Asset>();
            List<Category> categoryList = new List<Category>() {
                new Category()
                {
                    Id = categoryId,
                    Name = "Name",
                    Prefix = "LT"
                }
            };
            var asset = new Asset()
            {
                Id = Guid.NewGuid(),
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                Pubished = true,
                Name = $"DELL G1",
                Code = "LT000005",
                Location = "HN",
                Specification = $"this is G1",
                CategoryId = categoryId,
                InstalledDate = DateTime.Now.AddDays(-5),
                State = State.Available,
                Category = categoryList.FirstOrDefault()
            };
            categoryList.ElementAt(0).Assets = assetEntity;
            assetEntity.Add(asset);
            var mockCategory = categoryList.AsQueryable().BuildMock();
            var mockAsset = assetEntity.AsQueryable().BuildMock();

            _categoryRepository.Setup(x => x.Entities).Returns(mockCategory.Object);
            _assetRepository.Setup(x => x.Entities).Returns(mockAsset.Object);

            var result = await _assetService.AutoGenerateAssetCode(asset);

            result.Should().NotBeNull();
            result.Should().Be("LT000006");

            _categoryRepository.Verify(mock => mock.Entities, Times.Once());
            _assetRepository.Verify(mock => mock.Entities, Times.Once());
        }

        [Fact]
        public async Task UpdateAssetShouldThrowExceptionAsync()
        {
            var assetDto = new AssetDto()
            {
                Id = Guid.NewGuid(),
                Name = "DELL",
                Specification = "This is dell",
                InstalledDate = DateTime.Now.AddDays(-5),
                State = "0"
            };
            _assetRepository.Setup(x => x.GetByIdAsync(It.IsAny<object>())).Returns(Task.FromResult<Asset>(null));
            Func<Task> act = async () => await _assetService.UpdateAsync(assetDto);

            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task UpdateAssetShouldReturnObjectAsync()
        {
            var asset = new Asset()
            {
                Id = Guid.NewGuid(),
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                Pubished = true,
                Code = $"LP0001",
                Name = $"DELL G1",
                Location = "HN",
                Specification = $"this is G1",
                CategoryId = Guid.NewGuid(),
                InstalledDate = DateTime.Now.AddDays(-5)
            };
            var assetState = EnumConverExtension.GetValueInt<State>(asset.State.ToString()).ToString();
            var assetDto = new AssetDto()
            {
                Name = "DELL",
                Specification = asset.Specification,
                InstalledDate = asset.InstalledDate,
                State = assetState
            };
            _assetRepository.Setup(x => x.GetByIdAsync(It.IsAny<object>())).Returns(Task.FromResult(asset));
            var result = await _assetService.UpdateAsync(assetDto);

            result.Should().NotBeNull();
            result.Name.Should().Be("DELL");

            _assetRepository.Verify(mock => mock.GetByIdAsync(It.IsAny<object>()), Times.Once());
        }

        [Fact]
        public async Task SetStateShouldThrowExceptionAsync()
        {
            _assetRepository.Setup(x => x.GetByIdAsync(It.IsAny<object>())).Returns(Task.FromResult<Asset>(null));
            Func<Task> act = async () => await _assetService.SetStateAsync(It.IsAny<Guid>(), It.IsAny<State>());

            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task GetFilterAssetShouldReturnEmptyCategoryListAsync()
        {
            List<Category> emptyList = new List<Category>();
            _categoryRepository
                  .Setup(x => x.GetAllAsync())
                  .Returns(Task.FromResult(emptyList.AsEnumerable()));

            var result = await _assetService.GetFilterAssetAsync();
            result.Should().NotBeNull();
            result.CategoryList.Should().HaveCount(0);
            result.StateList.Should().HaveCount(9);

            _categoryRepository.Verify(mock => mock.GetAllAsync(), Times.Once());
        }

        [Fact]
        public async Task GetFilterAssetShouldReturnObjectAsync()
        {
            List<Category> categoryList = new List<Category>() {
                new Category()
                {
                    Id = Guid.NewGuid(),
                    Name = "Name"
                },
                new Category()
                {
                    Id = Guid.NewGuid(),
                    Name = "Name1"
                },
                new Category()
                {
                    Id = Guid.NewGuid(),
                    Name = "Name2"
                },
            };
            _categoryRepository
                  .Setup(x => x.GetAllAsync())
                  .Returns(Task.FromResult(categoryList.AsEnumerable()));

            var result = await _assetService.GetFilterAssetAsync();
            result.Should().NotBeNull();
            result.CategoryList.Should().HaveCount(3);
            result.StateList.Should().HaveCount(9);

            _categoryRepository.Verify(mock => mock.GetAllAsync(), Times.Once());
        }

        [Fact]
        public async Task PagedQueryShouldReturnObjectAsync()
        {
            List<Asset> assetEntity = new List<Asset>();
            List<Category> categoryList = new List<Category>() {
                new Category()
                {
                    Id = Guid.NewGuid(),
                    Name = "Name"
                },
                new Category()
                {
                    Id = Guid.NewGuid(),
                    Name = "job"
                }
            };
            for (int i = 0; i < 8; i++)
            {
                var asset = new Asset()
                {
                    Id = Guid.NewGuid(),
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    Pubished = true,
                    Code = $"LP000{i}",
                    Name = $"DELL G{i}",
                    Location = "HN",
                    Specification = $"this is {i}",
                    CategoryId = Guid.NewGuid(),
                    InstalledDate = DateTime.Now.AddDays(-5),
                    Category = categoryList.ElementAt(0),
                    State = State.Available
                };
                if (i == 3) asset.Code = "LP0009"; //this line makes it comes first
                if (i == 4) asset.Category = categoryList.ElementAt(1); //this line is not match the filter
                if (i == 5) asset.Pubished = false; //this line is not match the condition
                if (i == 6) asset.Location = "HCM"; //this line is not match the filter
                if (i == 7) asset.State = State.Assigned; //this line is not match the filter
                
                assetEntity.Add(asset);
            }

            var filter = new FilterAssetModel()
            {
                OrderProperty = "code",
                State = "0",
                Desc = true,
                KeySearch = "dell",
                Limit = 10,
                Category = "Name",
                Location = "HN"
            };

            var mockAsset = assetEntity.AsQueryable().BuildMock();

            _assetRepository.Setup(x => x.Entities).Returns(mockAsset.Object);

            var result = await _assetService.PagedQueryAsync(filter);

            result.Items.Should().HaveCount(4);
            result.Items.FirstOrDefault().Code.Should().Be("LP0009");
            result.TotalItems.Should().Be(4);
            result.TotalPages.Should().Be(1);

            _assetRepository.Verify(mock => mock.Entities, Times.Once());
        }

        [Fact]
        public async Task PagedQueryShouldPagingSuccessfullyAsync()
        {
            List<Asset> assetEntity = new List<Asset>();
            List<Category> categoryList = new List<Category>() {
                new Category()
                {
                    Id = Guid.NewGuid(),
                    Name = "Name"
                }
            };
            for (int i = 0; i < 9; i++)
            {
                var asset = new Asset()
                {
                    Id = Guid.NewGuid(),
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    Pubished = true,
                    Code = $"LP000{i}",
                    Name = $"DELL G{i}",
                    Location = "HN",
                    Specification = $"this is {i}",
                    CategoryId = Guid.NewGuid(),
                    InstalledDate = DateTime.Now.AddDays(-5),
                    Category = categoryList.FirstOrDefault(),
                    State = State.Available
                };
                assetEntity.Add(asset);
            }

            var filter = new FilterAssetModel()
            {
                OrderProperty = "code",
                Desc = true,
                Limit = 2,
                Page = 2,
                Location = "HN"
            };

            var mockAsset = assetEntity.AsQueryable().BuildMock();

            _assetRepository.Setup(x => x.Entities).Returns(mockAsset.Object);

            var result = await _assetService.PagedQueryAsync(filter);

            result.Items.Should().HaveCount(2);
            result.Items.FirstOrDefault().Code.Should().Be("LP0006");
            result.TotalItems.Should().Be(9);
            result.TotalPages.Should().Be(5);

            _assetRepository.Verify(mock => mock.Entities, Times.Once());
        }

        [Fact]
        public async Task PagedQueryShouldShouldReturnOnlyAvailableAsync()
        {
            List<Asset> assetEntity = new List<Asset>();
            List<Category> categoryList = new List<Category>() {
                new Category()
                {
                    Id = Guid.NewGuid(),
                    Name = "Name"
                }
            };
            for (int i = 0; i < 4; i++)
            {
                var asset = new Asset()
                {
                    Id = Guid.NewGuid(),
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    Pubished = true,
                    Code = $"LP000{i}",
                    Name = $"DELL G{i}",
                    Location = "HN",
                    Specification = $"this is {i}",
                    CategoryId = Guid.NewGuid(),
                    InstalledDate = DateTime.Now.AddDays(-5),
                    Category = categoryList.FirstOrDefault(),
                    State = State.Available
                };
                if (i == 3) asset.State = State.NotAvailable; //this line is not match the filter
                assetEntity.Add(asset);
            }

            var filter = new FilterAssetModel()
            {
                OrderProperty = "code",
                Desc = true,
                Limit = 10,
                MustBeAvailable = true,
                Location = "HN"
            };

            var mockAsset = assetEntity.AsQueryable().BuildMock();

            _assetRepository.Setup(x => x.Entities).Returns(mockAsset.Object);

            var result = await _assetService.PagedQueryAsync(filter);

            result.Items.Should().HaveCount(3);
            var notAvailable = ((State)State.NotAvailable).GetDescription<State>();
            result.Items.Any(x => x.State == notAvailable).Should().BeFalse();

            _assetRepository.Verify(mock => mock.Entities, Times.Once());
        }
    }
}
