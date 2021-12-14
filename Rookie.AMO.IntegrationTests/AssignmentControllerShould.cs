using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
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
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.Extensions.Primitives;
using Rookie.AMO.Contracts.Dtos.Filter;

namespace Rookie.AMO.IntegrationTests
{
    public class AssignmentControllerShould : IClassFixture<SqliteInMemoryFixture>
    {
        private readonly SqliteInMemoryFixture _fixture;
        private readonly IBaseRepository<Category> _categoryRepository;
        private readonly IBaseRepository<Asset> _assetRepository;
        private readonly IBaseRepository<Assignment> _assignmentRepository;
        private readonly Mock<IIdentityProvider> _identityProvider;
        private readonly IMapper _mapper;

        public AssignmentControllerShould(SqliteInMemoryFixture fixture)
        {
            _fixture = fixture;
            _fixture.CreateDatabase();

            _categoryRepository = new BaseRepository<Category>(_fixture.Context);
            _assetRepository = new BaseRepository<Asset>(_fixture.Context);
            _assignmentRepository = new BaseRepository<Assignment>(_fixture.Context);
            _identityProvider = new Mock<IIdentityProvider>();


            var config = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public async Task Add_New_Assignment_Success()
        {
            // Arrange
            var assignmentService = new AssignmentService(_assignmentRepository, _assetRepository,  _mapper);
            var assetService = new AssetService(_assetRepository, _categoryRepository, _mapper);
            var assignmentController = new AssignmentController(assignmentService, _identityProvider.Object, assetService);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                                        new Claim("sub", Guid.NewGuid().ToString())
                                   }, "TestAuthentication"));
            assignmentController.ControllerContext.HttpContext = new DefaultHttpContext { User = user };

            var categoryId = Guid.NewGuid();
            var assetId = Guid.NewGuid();

            var category = new Category()
            {
                Id = categoryId,
                Name = "Laptop",
                Prefix = "LP",
            };

            var asset = new Asset()
            {
                Id = assetId,
                State = State.Available,
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

            var assignment = new AssignmentRequest()
            {
                AssignedDate = DateTime.Now.AddDays(30),
                AssetID = assetId,
                UserID = Guid.NewGuid(),
                Note = "abc"
            };

            await _categoryRepository.AddAsync(category);
            await _assetRepository.AddAsync(asset);

            // Act
            var result = await assignmentController.CreateAsync(assignment);

            // Assert
            Assert.NotNull(result);
            var resultContent = (CreatedResult)result.Result;
            Assert.Equal(201, resultContent.StatusCode);

            var assignedAsset = await assetService.GetByIdAsync(asset.Id);
            var createdAssignment = await assignmentService.GetByIdAsync(((AssignmentDto)resultContent.Value).Id);

            Assert.Equal(assignment.AssetID, createdAssignment.AssetID);
            Assert.Equal(assignment.UserID, createdAssignment.UserID);
            Assert.Equal(assignment.Note, createdAssignment.Note);
            Assert.Equal(assignment.AssignedDate.Date, createdAssignment.AssignedDate.Date);
            Assert.Equal("6", assignedAsset.State); //State.Assigned = 6
        }

        [Fact]
        public async Task Update_Assignment_Success()
        {
            // Arrange
            var assignmentService = new AssignmentService(_assignmentRepository, _assetRepository, _mapper);
            var assetService = new AssetService(_assetRepository, _categoryRepository, _mapper);
            var assignmentController = new AssignmentController(assignmentService, _identityProvider.Object, assetService);

            var categoryId = Guid.NewGuid();
            var assetId = Guid.NewGuid();
            var assetUpdateId = Guid.NewGuid();
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

            var assetUpdate = new Asset()
            {
                Id = assetUpdateId,
                State = State.Available,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                Pubished = true,
                Code = $"LP0002",
                Name = $"DELL G2",
                Location = "HN",
                Specification = $"this is 2",
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
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                Pubished = true
            };

            var assignmentRequest = new AssignmentRequest()
            {
                AssignedDate = DateTime.Now.AddDays(30),
                AssetID = assetUpdateId,
                UserID = Guid.NewGuid(),
                Note = "abc"
            };

            await _categoryRepository.AddAsync(category);
            await _assetRepository.AddAsync(asset);
            await _assetRepository.AddAsync(assetUpdate);
            await _assignmentRepository.AddAsync(assignment);

            // Act
            var result = await assignmentController.UpdateAsync(assignmentId, assignmentRequest);

            // Assert
            Assert.NotNull(result);
            var resultContent = (NoContentResult)result; // <-- Cast is before using it.
            Assert.Equal(204, resultContent.StatusCode);

            var availableAsset = await assetService.GetByIdAsync(asset.Id);
            var assignedAsset = await assetService.GetByIdAsync(assetUpdate.Id);
            var updateAssignment = await assignmentService.GetByIdAsync(assignment.Id);

            Assert.Equal(assetUpdateId, updateAssignment.AssetID);
            Assert.Equal("0", availableAsset.State); //State.Available = 0
            Assert.Equal("6", assignedAsset.State); //State.Assigned = 6
        }


        [Fact]
        public void Update_Assignment_Fails()
        {
            // Arrange
            var assignmentService = new AssignmentService(_assignmentRepository, _assetRepository, _mapper);
            var assetService = new AssetService(_assetRepository, _categoryRepository, _mapper);
            var assignmentController = new AssignmentController(assignmentService, _identityProvider.Object, assetService);

            var assignmentRequest = new AssignmentRequest()
            {
                AssignedDate = DateTime.Now.AddDays(30),
                AssetID = Guid.NewGuid(),
                UserID = Guid.NewGuid(),
                Note = "abc"
            };

            // Act
            var ex = Assert.ThrowsAsync<ArgumentNullException>(async () => await assignmentController.UpdateAsync(Guid.NewGuid(), assignmentRequest));

            // Assert
            Assert.Equal("Value can not be null. (Parameter 'assignmentDto')", ex.Result.Message);
        }

        [Fact]
        public async Task Delete_Assignment_Success()
        {
            // Arrange
            var assignmentService = new AssignmentService(_assignmentRepository, _assetRepository, _mapper);
            var assetService = new AssetService(_assetRepository, _categoryRepository, _mapper);
            var assignmentController = new AssignmentController(assignmentService, _identityProvider.Object, assetService);

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
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                Pubished = true
            };

            await _categoryRepository.AddAsync(category);
            await _assetRepository.AddAsync(asset);
            await _assignmentRepository.AddAsync(assignment);

            // Act
            var result = await assignmentController.DeleteAssignmentAsync(assignmentId);

            // Assert
            Assert.NotNull(result);
            var resultContent = (NoContentResult)result; // <-- Cast is before using it.
            Assert.Equal(204, resultContent.StatusCode);

            var availableAsset = await assetService.GetByIdAsync(asset.Id);
            var assignmentList = await assignmentService.GetAllAsync();

            Assert.Empty(assignmentList);
            Assert.Equal("0", availableAsset.State); //State.Available = 0
        }

        [Fact]
        public void Delete_Assignment_Fails()
        {
            // Arrange
            var assignmentService = new AssignmentService(_assignmentRepository, _assetRepository, _mapper);
            var assetService = new AssetService(_assetRepository, _categoryRepository, _mapper);
            var assignmentController = new AssignmentController(assignmentService, _identityProvider.Object, assetService);

            // Act
            var ex = Assert.ThrowsAsync<ArgumentNullException>(async () => await assignmentController.DeleteAssignmentAsync(Guid.NewGuid()));

            // Assert
            Assert.Equal("Value can not be null. (Parameter 'assignmentDto')", ex.Result.Message);
        }

        [Fact]
        public async Task Get_By_Assignment_Id_Success()
        {
            // Arrange
            var assignmentService = new AssignmentService(_assignmentRepository, _assetRepository, _mapper);
            var assetService = new AssetService(_assetRepository, _categoryRepository, _mapper);
            var assignmentController = new AssignmentController(assignmentService, _identityProvider.Object, assetService);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Add("Authorization", new StringValues("Bearer dodgeThis"));
            assignmentController.ControllerContext.HttpContext = httpContext;

            var categoryId = Guid.NewGuid();
            var assetId = Guid.NewGuid();
            var assignmentId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var userAdminId = Guid.NewGuid();

            var userList = new List<UserDto>() {
                new UserDto()
                {
                    Id = userId,
                    CodeStaff = "SD0001",
                    FirstName = "Le",
                    LastName = "Truong",
                    UserName = "truong",
                    FullName = "Le Truong",
                    DateOfBirth = new DateTime(2001, 1, 6),
                    JoinedDate = DateTime.Now,
                    Gender = "Male",
                    Type = "Admin",
                    Disable = false,
                    Location = "HN",
                    Email = "truong@e.com"
                },
                new UserDto()
                {
                    Id = userAdminId,
                    FirstName = "John",
                    LastName = "Constantine",
                    UserName = "john",
                    FullName = "John Constantine",
                    DateOfBirth = new DateTime(2001, 1, 6),
                    JoinedDate = DateTime.Now,
                    Gender = "Male",
                    Type = "Admin",
                    Disable = false,
                    Location = "HN",
                    Email = "john@e.com"
                }
            };

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
                CreatorId = userAdminId,
                UserID = userId,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                Pubished = true
            };

            await _categoryRepository.AddAsync(category);
            await _assetRepository.AddAsync(asset);
            await _assignmentRepository.AddAsync(assignment);
            _identityProvider.Setup(x => x.GetAllUser(It.IsAny<string>())).Returns(Task.FromResult(userList.AsEnumerable()));

            // Act
            var result = await assignmentController.GetByIdAsync(assignmentId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(asset.Name, result.AssetName);
            Assert.Equal(userList.ElementAt(0).FullName, result.AssignedTo);
            Assert.Equal(userList.ElementAt(1).FullName, result.AssignedBy);
        }

        [Fact]
        public async Task Get_By_Assignment_Id_Return_Null()
        {
            // Arrange
            var assignmentService = new AssignmentService(_assignmentRepository, _assetRepository, _mapper);
            var assetService = new AssetService(_assetRepository, _categoryRepository, _mapper);
            var assignmentController = new AssignmentController(assignmentService, _identityProvider.Object, assetService);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Add("Authorization", new StringValues("Bearer dodgeThis"));
            assignmentController.ControllerContext.HttpContext = httpContext;

            // Act
            var result = await assignmentController.GetByIdAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task Get_History_Assignment_By_Asset_Success()
        {
            // Arrange
            var assignmentService = new AssignmentService(_assignmentRepository, _assetRepository, _mapper);
            var assetService = new AssetService(_assetRepository, _categoryRepository, _mapper);
            var assignmentController = new AssignmentController(assignmentService, _identityProvider.Object, assetService);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Add("Authorization", new StringValues("Bearer dodgeThis"));
            assignmentController.ControllerContext.HttpContext = httpContext;

            var categoryId = Guid.NewGuid();
            var assetId = Guid.NewGuid();
            var assetTestId = Guid.NewGuid();
            var userAdminId = Guid.NewGuid();
            var userAdminName = "john";

            var userList = new List<UserDto>() {
                new UserDto()
                {
                    Id = userAdminId,
                    FirstName = "John",
                    LastName = "Constantine",
                    UserName = userAdminName,
                    FullName = "John Constantine",
                    DateOfBirth = new DateTime(2001, 1, 6),
                    JoinedDate = DateTime.Now,
                    Gender = "Male",
                    Type = "Admin",
                    Disable = false,
                    Location = "HN",
                    Email = "john@e.com"
                }
            };

            var category = new Category()
            {
                Id = categoryId,
                Name = "Laptop",
                Prefix = "LP",
            };
            await _categoryRepository.AddAsync(category);

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
            var assetTest = new Asset()
            {
                Id = assetTestId,
                State = State.Assigned,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                Pubished = true,
                Code = $"LP0002",
                Name = $"DELL G2",
                Location = "HN",
                Specification = $"this is 1",
                CategoryId = categoryId,
                InstalledDate = DateTime.Now.AddDays(-5)
            };
            await _assetRepository.AddAsync(asset);
            await _assetRepository.AddAsync(assetTest);

            for (int i = 0; i < 5; i++)
            {
                var staffId = Guid.NewGuid();
                userList.Add(
                    new UserDto()
                    {
                        Id = staffId,
                        FirstName = "Staff",
                        LastName = $"Test{i}",
                        UserName = $"test{i}",
                        FullName = $"Staff Test{i}",
                        DateOfBirth = new DateTime(2001, 1, 6),
                        JoinedDate = DateTime.Now,
                        Gender = "Male",
                        Type = "Staff",
                        Disable = false,
                        Location = "HN",
                        Email = $"test{i}@e.com"
                    }
                );
                var assignment = new Assignment()
                {
                    Id = Guid.NewGuid(),
                    AssignedDate = DateTime.Now.AddDays(30 - i),
                    State = State.WaitingAccept,
                    AssetID = assetId,
                    CreatorId = userAdminId,
                    UserID = staffId,
                    RequestAssignmentId = null,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    Pubished = true
                };
                if (i == 2) assignment.AssetID = assetTestId;
                await _assignmentRepository.AddAsync(assignment);
            }
            _identityProvider.Setup(x => x.GetAllUser(It.IsAny<string>())).Returns(Task.FromResult(userList.AsEnumerable()));

            // Act
            var result = await assignmentController.GetHistoryAsync(assetId.ToString());
            var resultContent = (OkObjectResult)result;
            var resultValue = (IEnumerable<HistoryAssignmentDto>)resultContent.Value;
            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, resultContent.StatusCode);
            Assert.Equal(3, resultValue.Count());
            Assert.Equal("test0", resultValue.FirstOrDefault().AssignedTo);
            Assert.Equal("test3", resultValue.LastOrDefault().AssignedTo);
            Assert.DoesNotContain(resultValue, x => x.AssignedBy != userAdminName);
        }

        [Fact]
        public async Task Get_History_Assignment_By_Asset_Return_Empty()
        {
            // Arrange
            var assignmentService = new AssignmentService(_assignmentRepository, _assetRepository, _mapper);
            var assetService = new AssetService(_assetRepository, _categoryRepository, _mapper);
            var assignmentController = new AssignmentController(assignmentService, _identityProvider.Object, assetService);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Add("Authorization", new StringValues("Bearer dodgeThis"));
            assignmentController.ControllerContext.HttpContext = httpContext;

            // Act
            var result = await assignmentController.GetHistoryAsync(Guid.NewGuid().ToString());
            var resultContent = (OkObjectResult)result;
            var resultValue = (IEnumerable<HistoryAssignmentDto>)resultContent.Value;
            // Assert
            Assert.NotNull(result);
            Assert.Empty(resultValue);
        }

        [Fact]
        public async Task Get_Assignment_By_User_Id_Success()
        {
            // Arrange
            var assignmentService = new AssignmentService(_assignmentRepository, _assetRepository, _mapper);
            var assetService = new AssetService(_assetRepository, _categoryRepository, _mapper);
            var assignmentController = new AssignmentController(assignmentService, _identityProvider.Object, assetService);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Add("Authorization", new StringValues("Bearer dodgeThis"));
            assignmentController.ControllerContext.HttpContext = httpContext;

            var categoryId = Guid.NewGuid();
            var assetId = Guid.NewGuid();
            var userCreatorId = Guid.NewGuid();
            var userAssigeeId = Guid.NewGuid();

            var userList = new List<UserDto>() {
                new UserDto()
                {
                    Id = userCreatorId,
                    FirstName = "John",
                    LastName = "Constantine",
                    UserName = "john",
                    FullName = "John Constantine",
                    DateOfBirth = new DateTime(2001, 1, 6),
                    JoinedDate = DateTime.Now,
                    Gender = "Male",
                    Type = "Admin",
                    Disable = false,
                    Location = "HN",
                    Email = "john@e.com"
                },
                new UserDto()
                {
                    Id = userAssigeeId,
                    FirstName = "Staff",
                    LastName = "Test0",
                    UserName = "test0",
                    FullName = "Staff Test0",
                    DateOfBirth = new DateTime(2001, 1, 6),
                    JoinedDate = DateTime.Now,
                    Gender = "Male",
                    Type = "Staff",
                    Disable = false,
                    Location = "HN",
                    Email = "test0@e.com"
                }
            };

            var category = new Category()
            {
                Id = categoryId,
                Name = "Laptop",
                Prefix = "LP",
            };
            await _categoryRepository.AddAsync(category);

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

            await _assetRepository.AddAsync(asset);

            for (int i = 0; i < 3; i++)
            {
                var assignment = new Assignment()
                {
                    Id = Guid.NewGuid(),
                    AssignedDate = DateTime.Now.AddDays(-30),
                    State = State.WaitingAccept,
                    AssetID = assetId,
                    CreatorId = userCreatorId,
                    UserID = userAssigeeId,
                    RequestAssignmentId = null,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    Pubished = true
                };
                await _assignmentRepository.AddAsync(assignment);
            }
            _identityProvider.Setup(x => x.GetAllUser(It.IsAny<string>())).Returns(Task.FromResult(userList.AsEnumerable()));

            // Act
            var result = await assignmentController.GetByUserIdAsync(userAssigeeId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
            Assert.DoesNotContain(result, x => x.AssignedTo != "test0");
            Assert.DoesNotContain(result, x => x.AssignedBy != "john");
        }

        [Fact]
        public async Task Get_Assignment_By_User_Id_Empty()
        {
            // Arrange
            var assignmentService = new AssignmentService(_assignmentRepository, _assetRepository, _mapper);
            var assetService = new AssetService(_assetRepository, _categoryRepository, _mapper);
            var assignmentController = new AssignmentController(assignmentService, _identityProvider.Object, assetService);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Add("Authorization", new StringValues("Bearer dodgeThis"));
            assignmentController.ControllerContext.HttpContext = httpContext;

            // Act
            var result = await assignmentController.GetByUserIdAsync(Guid.NewGuid());

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task Get_All_Assignment_Success()
        {
            // Arrange
            var assignmentService = new AssignmentService(_assignmentRepository, _assetRepository, _mapper);
            var assetService = new AssetService(_assetRepository, _categoryRepository, _mapper);
            var assignmentController = new AssignmentController(assignmentService, _identityProvider.Object, assetService);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Add("Authorization", new StringValues("Bearer dodgeThis"));
            assignmentController.ControllerContext.HttpContext = httpContext;

            var categoryId = Guid.NewGuid();
            var assetId = Guid.NewGuid();

            var category = new Category()
            {
                Id = categoryId,
                Name = "Laptop",
                Prefix = "LP",
            };
            await _categoryRepository.AddAsync(category);

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

            await _assetRepository.AddAsync(asset);

            for (int i = 0; i < 3; i++)
            {
                var assignment = new Assignment()
                {
                    Id = Guid.NewGuid(),
                    AssignedDate = DateTime.Now.AddDays(-30),
                    State = State.WaitingAccept,
                    AssetID = assetId,
                    CreatorId = Guid.NewGuid(),
                    UserID = Guid.NewGuid(),
                    RequestAssignmentId = null,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    Pubished = true
                };
                await _assignmentRepository.AddAsync(assignment);
            }

            // Act
            var result = await assignmentController.GetAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
        }

        [Fact]
        public async Task Get_All_Assignment_Return_Empty()
        {
            // Arrange
            var assignmentService = new AssignmentService(_assignmentRepository, _assetRepository, _mapper);
            var assetService = new AssetService(_assetRepository, _categoryRepository, _mapper);
            var assignmentController = new AssignmentController(assignmentService, _identityProvider.Object, assetService);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Add("Authorization", new StringValues("Bearer dodgeThis"));
            assignmentController.ControllerContext.HttpContext = httpContext;

            // Act
            var result = await assignmentController.GetAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        //Page filter testing
        //Since we cover all the filter in the assignment service unit test
        //So I only test filter that the unit test does not cover
        [Fact]
        public async Task Find_Assignment_With_User_Filter_Order_By_Asset_Name_Success()
        {
            // Arrange
            var assignmentService = new AssignmentService(_assignmentRepository, _assetRepository, _mapper);
            var assetService = new AssetService(_assetRepository, _categoryRepository, _mapper);
            var assignmentController = new AssignmentController(assignmentService, _identityProvider.Object, assetService);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Add("Authorization", new StringValues("Bearer dodgeThis"));
            assignmentController.ControllerContext.HttpContext = httpContext;

            var categoryId = Guid.NewGuid();
            var userCreatorId = Guid.NewGuid();
            var userAssigeeId = Guid.NewGuid();
            var anotherUserAssigeeId = Guid.NewGuid();

            var userList = new List<UserDto>() {
                new UserDto()
                {
                    Id = userCreatorId,
                    FirstName = "John",
                    LastName = "Constantine",
                    UserName = "john",
                    FullName = "John Constantine",
                    DateOfBirth = new DateTime(2001, 1, 6),
                    JoinedDate = DateTime.Now,
                    Gender = "Male",
                    Type = "Admin",
                    Disable = false,
                    Location = "HN",
                    Email = "john@e.com"
                },
                new UserDto()
                {
                    Id = userAssigeeId,
                    FirstName = "Staff",
                    LastName = "Test0i",
                    UserName = "test0i",
                    FullName = "Staff Test0i",
                    DateOfBirth = new DateTime(2001, 1, 6),
                    JoinedDate = DateTime.Now,
                    Gender = "Male",
                    Type = "Staff",
                    Disable = false,
                    Location = "HN",
                    Email = "test0i@e.com"
                },
                new UserDto()
                {
                    Id = anotherUserAssigeeId,
                    FirstName = "Staff",
                    LastName = "Test1",
                    UserName = "test1",
                    FullName = "Staff Test1",
                    DateOfBirth = new DateTime(2001, 1, 6),
                    JoinedDate = DateTime.Now,
                    Gender = "Male",
                    Type = "Staff",
                    Disable = false,
                    Location = "HN",
                    Email = "test1@e.com"
                }
            };

            var category = new Category()
            {
                Id = categoryId,
                Name = "Laptop",
                Prefix = "LP",
            };
            await _categoryRepository.AddAsync(category);



            for (int i = 0; i < 10; i++)
            {
                var assetId = Guid.NewGuid();
                var asset = new Asset()
                {
                    Id = assetId,
                    State = State.Assigned,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    Pubished = true,
                    Code = $"LP000{i}",
                    Name = $"DELL G{i}",
                    Location = "HN",
                    Specification = $"this is {i}",
                    CategoryId = categoryId,
                    InstalledDate = DateTime.Now.AddDays(-5)
                };

                await _assetRepository.AddAsync(asset);

                var assignment = new Assignment()
                {
                    Id = Guid.NewGuid(),
                    AssignedDate = DateTime.Now.AddDays(-30),
                    State = State.WaitingAccept,
                    AssetID = assetId,
                    CreatorId = userCreatorId,
                    UserID = userAssigeeId,
                    RequestAssignmentId = null,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    Pubished = true
                };
                if (i == 0) assignment.UserID = anotherUserAssigeeId; //this line makes it eliminated from the list
                await _assignmentRepository.AddAsync(assignment);
            }
            _identityProvider.Setup(x => x.GetAllUser(It.IsAny<string>())).Returns(Task.FromResult(userList.AsEnumerable()));

            FilterAssignmentModel filter = new FilterAssignmentModel() {
                KeySearch = "i",
                Limit = 2,
                Page = 2,
                OrderProperty = "AssetName",
                Desc = true
            };
            // Act
            var result = await assignmentController.FindAsync(filter);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.CurrentPage);
            Assert.Equal(5, result.TotalPages);
            Assert.Equal(9, result.TotalItems);
            Assert.Equal(2, result.Items.Count());
            Assert.DoesNotContain(result.Items, x => x.AssignedTo != "test0i");
            Assert.Equal("LP0007", result.Items.FirstOrDefault().AssetCode);
        }

        [Fact]
        public async Task Accept_Assignment_Success()
        {
            // Arrange
            var assignmentService = new AssignmentService(_assignmentRepository, _assetRepository, _mapper);
            var assetService = new AssetService(_assetRepository, _categoryRepository, _mapper);
            var assignmentController = new AssignmentController(assignmentService, _identityProvider.Object, assetService);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Add("Authorization", new StringValues("Bearer dodgeThis"));
            assignmentController.ControllerContext.HttpContext = httpContext;

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
                State = State.WaitingAccept,
                AssetID = assetId,
                CreatorId = Guid.NewGuid(),
                UserID = Guid.NewGuid(),
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                Pubished = true
            };

            await _categoryRepository.AddAsync(category);
            await _assetRepository.AddAsync(asset);
            await _assignmentRepository.AddAsync(assignment);

            // Act
            var result = await assignmentController.AcceptAsync(assignmentId);
            var resultContent = (OkResult)result;
            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, resultContent.StatusCode);

            var acceptAssignment = await _assignmentRepository.GetByIdAsync(assignment.Id);

            Assert.Equal(State.Accepted, acceptAssignment.State);
        }

        [Fact]
        public async Task Accept_Assignment_Return_Bad_Request()
        {
            // Arrange
            var assignmentService = new AssignmentService(_assignmentRepository, _assetRepository, _mapper);
            var assetService = new AssetService(_assetRepository, _categoryRepository, _mapper);
            var assignmentController = new AssignmentController(assignmentService, _identityProvider.Object, assetService);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Add("Authorization", new StringValues("Bearer dodgeThis"));
            assignmentController.ControllerContext.HttpContext = httpContext;

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
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                Pubished = true
            };

            await _categoryRepository.AddAsync(category);
            await _assetRepository.AddAsync(asset);
            await _assignmentRepository.AddAsync(assignment);

            // Act
            var result = await assignmentController.AcceptAsync(assignmentId);
            var resultContent = (BadRequestResult)result;
            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, resultContent.StatusCode);
        }

        [Fact]
        public async Task Is_Relate_To_User_Return_True()
        {
            // Arrange
            var assignmentService = new AssignmentService(_assignmentRepository, _assetRepository, _mapper);
            var assetService = new AssetService(_assetRepository, _categoryRepository, _mapper);
            var assignmentController = new AssignmentController(assignmentService, _identityProvider.Object, assetService);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Add("Authorization", new StringValues("Bearer dodgeThis"));
            assignmentController.ControllerContext.HttpContext = httpContext;

            var categoryId = Guid.NewGuid();
            var assetId = Guid.NewGuid();
            var assignmentId = Guid.NewGuid();
            var userId = Guid.NewGuid();

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
                UserID = userId,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                Pubished = true
            };

            await _categoryRepository.AddAsync(category);
            await _assetRepository.AddAsync(asset);
            await _assignmentRepository.AddAsync(assignment);

            // Act
            var result = await assignmentController.IsRelatedToUserAsync(userId);
            var resultContent = (OkObjectResult)result;
            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, resultContent.StatusCode);
            Assert.True((bool)resultContent.Value);
        }

        [Fact]
        public async Task Is_Relate_To_User_Return_No_Content()
        {
            // Arrange
            var assignmentService = new AssignmentService(_assignmentRepository, _assetRepository, _mapper);
            var assetService = new AssetService(_assetRepository, _categoryRepository, _mapper);
            var assignmentController = new AssignmentController(assignmentService, _identityProvider.Object, assetService);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Add("Authorization", new StringValues("Bearer dodgeThis"));
            assignmentController.ControllerContext.HttpContext = httpContext;
            // Act
            var result = await assignmentController.IsRelatedToUserAsync(Guid.NewGuid());
            var resultContent = (NoContentResult)result;
            // Assert
            Assert.NotNull(result);
            Assert.Equal(204, resultContent.StatusCode);
        }
    }
}
