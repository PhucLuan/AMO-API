using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rookie.AMO.Business;
using Rookie.AMO.Business.Interfaces;
using Rookie.AMO.Business.Services;
using Rookie.AMO.Contracts.Dtos;
using Rookie.AMO.Contracts.Dtos.Assignment;
using Rookie.AMO.DataAccessor.Entities;
using Rookie.AMO.IntegrationTests.Common;
using Rookie.AMO.WebApi.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Rookie.AMO.IntegrationTests
{
    public class RequestControllerShould : IClassFixture<SqliteInMemoryFixture>
    {
        private readonly SqliteInMemoryFixture _fixture;
        private readonly IBaseRepository<Category> _categoryRepository;
        private readonly IBaseRepository<Asset> _assetRepository;
        private readonly IBaseRepository<Assignment> _assignmentRepository;
        private readonly IBaseRepository<RequestAssignment> _requestRepository;
        private readonly IIdentityProvider _identityProvider;
        private readonly IMapper _mapper;

        public RequestControllerShould(SqliteInMemoryFixture fixture)
        {
            _fixture = fixture;
            _fixture.CreateDatabase();

            _categoryRepository = new BaseRepository<Category>(_fixture.Context);
            _assetRepository = new BaseRepository<Asset>(_fixture.Context);
            _assignmentRepository = new BaseRepository<Assignment>(_fixture.Context);
            _requestRepository = new BaseRepository<RequestAssignment>(_fixture.Context);

            var config = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public async Task Add_New_Request_Success()
        {
            // Arrange
            var requestService = new RequestService(_requestRepository, _assignmentRepository, _assetRepository, _mapper);
            var requestController = new RequestController(requestService, _identityProvider);

            var categoryId = Guid.NewGuid();
            var assetId = Guid.NewGuid();
            var assignmentId = Guid.NewGuid();

            var category = new Category()
            {
                Id = categoryId,
                Name = "Laptop",
                Prefix = "LP",
            };

            var asset = new Asset()
            {
                Id = assetId,
                State = State.Assigned,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                Pubished = true,
                Code = $"LP0001",
                Name = $"DELL G1",
                Location = "HN",
                Specification = $"this is 1",
                CategoryId = categoryId,
                InstalledDate = DateTime.Now.AddDays(-5)
            };

            var assignment = new Assignment()
            {
                Id = assignmentId,
                AssignedDate = DateTime.Now,
                State = State.Accepted,
                AssetID = assetId,
                CreatorId = Guid.NewGuid(),
                UserID = Guid.NewGuid(),
                RequestAssignmentId = null,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                Pubished = true
            };

            await _categoryRepository.AddAsync(category);
            await _assetRepository.AddAsync(asset);
            await _assignmentRepository.AddAsync(assignment);

            var newRequest = new RequestReturn {
                Id = assignmentId,
                UserId = Guid.NewGuid(),
            };

            // Act
            var result = await requestController.CreateAsync(newRequest);

            // Assert
            var createCode = result.Result as CreatedResult;
            Assert.Equal(201, createCode.StatusCode);
            result.Should().NotBeNull();

            var createdResult = Assert.IsType<CreatedResult>(result.Result);
            var returnValue = Assert.IsType<RequestDto>(createdResult.Value);

            var requestsExited = await requestService.GetAllAsync();

            Assert.Equal(newRequest.Id, returnValue.AssignmentID);
            Assert.Equal(newRequest.UserId, returnValue.UserRequestId);

            requestsExited.FirstOrDefault().Id.Should().NotBe(default(Guid));
        }

        [Fact]
        public async Task Cancel_Request_Success()
        {
            // Arrange
            var requestService = new RequestService(_requestRepository, _assignmentRepository, _assetRepository, _mapper);
            var requestController = new RequestController(requestService, _identityProvider);

            var categoryId = Guid.NewGuid();
            var assetId = Guid.NewGuid();
            var assignmentId = Guid.NewGuid();
            var requestId = Guid.NewGuid();

            var category = new Category()
            {
                Id = categoryId,
                Name = "Laptop",
                Prefix = "LP",
            };

            var asset = new Asset()
            {
                Id = assetId,
                State = State.Assigned,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                Pubished = true,
                Code = $"LP0001",
                Name = $"DELL G1",
                Location = "HN",
                Specification = $"this is 1",
                CategoryId = categoryId,
                InstalledDate = DateTime.Now.AddDays(-5),
                CreatorId = Guid.NewGuid(),
            };

            var assignment = new Assignment()
            {
                Id = assignmentId,
                AssignedDate = DateTime.Now,
                State = State.Accepted,
                AssetID = assetId,
                CreatorId = Guid.NewGuid(),
                UserID = Guid.NewGuid(),
                RequestAssignmentId = requestId,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                Pubished = true
            };

            var newRequest = new RequestAssignment
            {
                Id = requestId,
                State = State.WaitingForReturning,
                UserRequestId = Guid.NewGuid(),
            };

            await _categoryRepository.AddAsync(category);
            await _assetRepository.AddAsync(asset);
            await _requestRepository.AddAsync(newRequest);
            await _assignmentRepository.AddAsync(assignment);

            // Act
            var result = await requestController.DeleteRequestAsync(newRequest.Id);
            var okResult = result as NoContentResult;
            var requestExisted = await requestService.GetByIdAsync(newRequest.Id);

            // assert
            Assert.NotNull(okResult);
            Assert.Equal(204, okResult.StatusCode);
            requestExisted.Should().Be(null);
        }

        [Fact]
        public async Task Accept_Request_Success()
        {
            // Arrange
            // Arrange
            var requestService = new RequestService(_requestRepository, _assignmentRepository, _assetRepository, _mapper);
            var requestController = new RequestController(requestService, _identityProvider);

            var categoryId = Guid.NewGuid();
            var assetId = Guid.NewGuid();
            var assignmentId = Guid.NewGuid();
            var requestId = Guid.NewGuid();

            var category = new Category()
            {
                Id = categoryId,
                Name = "Laptop",
                Prefix = "LP",
            };

            var asset = new Asset()
            {
                Id = assetId,
                State = State.Assigned,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                Pubished = true,
                Code = $"LP0001",
                Name = $"DELL G1",
                Location = "HN",
                Specification = $"this is 1",
                CategoryId = categoryId,
                InstalledDate = DateTime.Now.AddDays(-5),
                CreatorId = Guid.NewGuid(),
            };

            var assignment = new Assignment()
            {
                Id = assignmentId,
                AssignedDate = DateTime.Now,
                State = State.Accepted,
                AssetID = assetId,
                CreatorId = Guid.NewGuid(),
                UserID = Guid.NewGuid(),
                RequestAssignmentId = requestId,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                Pubished = true
            };

            var request = new RequestAssignment
            {
                Id = requestId,
                State = State.WaitingForReturning,
                UserRequestId = Guid.NewGuid(),
            };

            await _categoryRepository.AddAsync(category);
            await _assetRepository.AddAsync(asset);
            await _requestRepository.AddAsync(request);
            await _assignmentRepository.AddAsync(assignment);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                                        new Claim("sub", Guid.NewGuid().ToString())
                                   }, "TestAuthentication"));
            requestController.ControllerContext = new ControllerContext();
            requestController.ControllerContext.HttpContext = new DefaultHttpContext { User = user };

            // Act
            var result = await requestController.AcceptAsync(requestId);
            var okResult = result as OkResult;
            var requestExisted = await requestService.GetByIdAsync(request.Id);

            // assert
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
            requestExisted.Should().NotBe(null);
        }

    }
}
