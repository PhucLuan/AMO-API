using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rookie.AMO.Business;
using Rookie.AMO.Business.Interfaces;
using Rookie.AMO.Business.Services;
using Rookie.AMO.Contracts.Dtos;
using Rookie.AMO.Contracts.Dtos.Filter;
using Rookie.AMO.DataAccessor.Entities;
using Rookie.AMO.IntegrationTests.Common;
using Rookie.AMO.Tests;
using Rookie.AMO.WebApi.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
namespace Rookie.AMO.IntegrationTests
{
    public class AssetControllerShould : IClassFixture<SqliteInMemoryFixture>
    {
        private readonly SqliteInMemoryFixture _fixture;
        private readonly IBaseRepository<Asset> _assetRepository;
        private readonly IBaseRepository<Category> _categoryRepository;
        private readonly IBaseRepository<Assignment> _assignmentRepository;
        //private readonly IAssetService _assetService;
        private readonly IAssignmentService _assignmentService;
        private readonly IMapper _mapper;
        public AssetControllerShould(SqliteInMemoryFixture fixture)
        {
            _fixture = fixture;
            fixture.CreateDatabase();
            _assetRepository = new BaseRepository<Asset>(_fixture.Context);
            _categoryRepository = new BaseRepository<Category>(_fixture.Context);
            _assignmentRepository = new BaseRepository<Assignment>(_fixture.Context);
            var config = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperProfile>());
            _mapper = config.CreateMapper();
            //_assetService = new AssetService(_assetRepository, _categoryRepository, _mapper);
            _assignmentService = new AssignmentService(_assignmentRepository, _assetRepository, _mapper);
        }
        [Fact]
        public async Task Create_Assets()
        {
            //Arrange
            var assetService = new AssetService(_assetRepository, _categoryRepository, _mapper);
            var assignmentService = new AssignmentService(_assignmentRepository, _assetRepository, _mapper);
            var assetController = new AssetController(assetService, assignmentService);
            var categoryId = Guid.NewGuid();
            var category = new Category()
            {
                Id = categoryId,
                Name = "Laptop",
                Prefix = "LA",
            };
            var asset1 = new AssetDto
            {
                Id = Guid.NewGuid(),
                State = "Available",
                Code = null,
                Name = "Laptop11",
                Location = null,
                Specification = "cool",
                CategoryId = categoryId,
                InstalledDate = DateTime.Now.AddDays(-5),
            };
            await _categoryRepository.AddAsync(category);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                                        new Claim("location", "HN")
                                   }, "TestAuthentication"));
            assetController.ControllerContext = new ControllerContext();
            assetController.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
            //Act
            var result = await assetController.CreateAsync(asset1);
            //Assert
            var createCode = result.Result as CreatedResult;
            Assert.Equal(201, createCode.StatusCode);
            result.Should().NotBeNull();
            var createdResult = Assert.IsType<CreatedResult>(result.Result);
            var returnValue = Assert.IsType<AssetDto>(createdResult.Value);
            var assetExists = await assetService.GetAllAsync();
            Assert.Equal(asset1.Id, returnValue.Id);
            //Assert.Equal(asset1.Code, returnValue.Code);
            Assert.Equal(asset1.Name, returnValue.Name);
            Assert.Equal(asset1.Location, returnValue.Location);
            Assert.Equal(asset1.Specification, returnValue.Specification);
            Assert.Equal(asset1.CategoryId, returnValue.CategoryId);
            Assert.Equal(asset1.InstalledDate, returnValue.InstalledDate);
            assetExists.FirstOrDefault().Id.Should().NotBe(default(Guid));
        }
        [Fact]
        public async Task Get_Asset_Successfully()
        {
            var assetService = new AssetService(_assetRepository, _categoryRepository, _mapper);
            var assignmentService = new AssignmentService(_assignmentRepository, _assetRepository, _mapper);
            var assetController = new AssetController(assetService, assignmentService);
            List<Category> categoryList = new List<Category>() {
                new Category()
                {
                    Id = Guid.NewGuid(),
                    Name = "Name",
                    Prefix = "LA"
                }
            };
            var asset1 = new Asset
            {
                Id = Guid.NewGuid(),
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                Pubished = true,
                Code = $"LA000011",
                Name = $"Laptop11",
                Location = "HN",
                Specification = $"cool",
                CategoryId = Guid.NewGuid(),
                InstalledDate = DateTime.Now.AddDays(-5),
                State = State.Available,
                Category = categoryList.FirstOrDefault()
            };
            var asset2 = new Asset
            {
                Id = Guid.NewGuid(),
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                Pubished = true,
                Code = $"LA000012",
                Name = $"Laptop12",
                Location = "HN",
                Specification = $"cooler",
                CategoryId = Guid.NewGuid(),
                InstalledDate = DateTime.Now.AddDays(-5),
                State = State.Available,
                Category = categoryList.FirstOrDefault()
            };
            await _assetRepository.AddAsync(asset1);
            await _assetRepository.AddAsync(asset2);
            var result = await assetController.GetAsync();
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().Contain(a => a.Name == "Laptop11");
        }
        //[Fact]
        //public async Task Create_Asset_Failed()
        //{
        //    // Arrange
        //    Guid id = Guid.NewGuid();
        //    List<Category> categoryList = new List<Category>() {
        //        new Category()
        //        {
        //            Id = Guid.NewGuid(),
        //            Name = "Name",
        //            Prefix = "LA"
        //        }
        //    };
        //    var existingAsset = new Asset { Id = id, Name = "Laptop1", Specification = "cool", Category = categoryList.FirstOrDefault() };
        //    var modifiedAsset = new AssetDto { Id = Guid.NewGuid(), Name = "Cool Laptop", Specification = "cool", CategoryId = Guid.NewGuid() };
        //    await _assetRepository.AddAsync(existingAsset);
        //    var assetService = new AssetService(_assetRepository, _categoryRepository, _mapper);
        //    var assetController = new AssetController(assetService, _assignmentService);
        //    // Act
        //    var result = await assetController.UpdateAsync(modifiedAsset);
        //    var badResult = result as BadRequestObjectResult;
        //    // assert
        //    Assert.NotNull(badResult);
        //    Assert.Equal(400, badResult.StatusCode);
        //}
        [Fact]
        public async Task Delete_Asset_Success()
        {
            // Arrange
            var assetService = new AssetService(_assetRepository, _categoryRepository, _mapper);
            var assetController = new AssetController(assetService, _assignmentService);
            var categoryId = Guid.NewGuid();
            var assetId = Guid.NewGuid();
            var category = new Category()
            {
                Id = categoryId,
                Name = "Name",
                Prefix = "LA"
            };
            var asset1 = new Asset()
            {
                Id = assetId,
                State = State.Available,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                Pubished = true,
                Code = $"LA000011",
                Name = $"Laptop11",
                Location = "HN",
                Specification = $"cool",
                CategoryId = categoryId,
                InstalledDate = DateTime.Now.AddDays(-5),
                CreatorId = Guid.NewGuid(),
            };
            await _categoryRepository.AddAsync(category);
            await _assetRepository.AddAsync(asset1);
            // Act
            var result = await assetController.DeleteAssetAsync(asset1.Id);
            var okResult = result as NoContentResult;
            // assert
            Assert.NotNull(okResult);
            Assert.Equal(204, okResult.StatusCode);
        }
        [Fact]
        public async Task Update_Asset_Success()
        {
            // Arrange
            var assetService = new AssetService(_assetRepository, _categoryRepository, _mapper);
            var assetController = new AssetController(assetService, _assignmentService);
            var categoryId = Guid.NewGuid();
            var assetId = Guid.NewGuid();
            var category = new Category()
            {
                Id = categoryId,
                Name = "Laptop",
                Prefix = "LA",
            };
            var asset1 = new Asset
            {
                Id = assetId,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                Pubished = true,
                Code = $"LA000011",
                Name = $"Laptop11",
                Location = "HN",
                Specification = $"cool",
                CategoryId = categoryId,
                InstalledDate = DateTime.Now.AddDays(-5),
                State = State.Available,
            };
            var updatedAsset = new AssetDto
            {
                Id = assetId,
                State = "Available",
                Code = null,
                Name = "Laptop11",
                Location = null,
                Specification = "cool",
                CategoryId = categoryId,
                InstalledDate = DateTime.Now.AddDays(-5),
            };
            await _categoryRepository.AddAsync(category);
            await _assetRepository.AddAsync(asset1);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                                        new Claim("location", "HN")
                                   }, "TestAuthentication"));
            assetController.ControllerContext = new ControllerContext();
            assetController.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
            //Act
            var result = await assetController.UpdateAsync(updatedAsset);
            var okResult = result as NoContentResult;
            var assetExisted = await assetService.GetByIdAsync(updatedAsset.Id);
            //Assert
            Assert.NotNull(okResult);
            Assert.Equal(204, okResult.StatusCode);
            //Assert.Equal("Update success", okResult.Value);
            Assert.Equal(updatedAsset.Name, assetExisted.Name);
        }
        [Fact]
        public async Task Find_Asset_Success()
        {
            var assetService = new AssetService(_assetRepository, _categoryRepository, _mapper);
            var assetController = new AssetController(assetService, _assignmentService);
            var categoryId = Guid.NewGuid();
            var category = new Category()
            {
                Id = categoryId,
                Name = "Laptop1",
                Prefix = "LA",
            };
            await _categoryRepository.AddAsync(category);
            for (int i = 0; i < 9; i++)
            {
                var assetId = Guid.NewGuid();
                var asset1 = new Asset
                {
                    Id = assetId,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    Pubished = true,
                    Code = $"LA000{i}",
                    Name = $"Laptop{i}",
                    Location = "HN",
                    Specification = $"This is Laptop{i}",
                    CategoryId = categoryId,
                    InstalledDate = DateTime.Now.AddDays(-5),
                    State = State.Available,
                };
                await _assetRepository.AddAsync(asset1);
            }
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                                        new Claim("location", "HN")
                                   }, "TestAuthentication"));
            assetController.ControllerContext = new ControllerContext();
            assetController.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
            FilterAssetModel filter = new()
            {
                KeySearch = "Laptop",
                OrderProperty = "code",
                Desc = true,
                Limit = 2,
                Page = 2,
                Location = "HN"
            };
            //Act
            var result = await assetController.FindAsync(filter);
            //Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.CurrentPage);
            Assert.Equal(5, result.TotalPages);
            Assert.Equal(9, result.TotalItems);
            Assert.Equal(2, result.Items.Count());
            Assert.DoesNotContain(result.Items, x => x.CategoryName != "Laptop1");
            Assert.Equal("LA0006", result.Items.FirstOrDefault().Code);
        }
    }
}

