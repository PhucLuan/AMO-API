using AutoMapper;
using FluentAssertions;
using Moq;
using Rookie.AMO.Business;
using Rookie.AMO.Business.Interfaces;
using Rookie.AMO.Business.Services;
using Rookie.AMO.Contracts.Dtos.Assignment;
using Rookie.AMO.DataAccessor.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using MockQueryable.Moq;
using Rookie.AMO.Contracts.Dtos.Filter;
using System.Linq.Expressions;

namespace Rookie.AMO.UnitTests.Business
{
    /// <summary>
    /// https://codewithshadman.com/repository-pattern-csharp/
    /// https://dotnettutorials.net/lesson/generic-repository-pattern-csharp-mvc/
    /// https://fluentassertions.com/exceptions/
    /// https://stackoverflow.com/questions/37422476/moq-expression-with-constraint-it-isexpressionfunct-bool
    /// </summary>
    public class AssignmentServiceShould
    {
        private readonly AssignmentService _assignmentService;
        private readonly Mock<IBaseRepository<Assignment>> _assignmentRepository;
        private readonly Mock<IBaseRepository<Asset>> _assetRepository;
        private readonly IMapper _mapper;

        public AssignmentServiceShould()
        {
            _assignmentRepository = new Mock<IBaseRepository<Assignment>>();
            _assetRepository = new Mock<IBaseRepository<Asset>>();

            var config = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperProfile>());
            _mapper = config.CreateMapper();

            _assignmentService = new AssignmentService(
                _assignmentRepository.Object,
                _assetRepository.Object,
                _mapper
            );
        }

        [Fact]
        public async Task GetAsyncShouldReturnNullAsync()
        {
            var id = Guid.NewGuid();
            _assignmentRepository
                  .Setup(x => x.GetByAsync(It.IsAny<Expression<Func<Assignment, bool>>>(), It.IsAny<string>()))
                  .Returns(Task.FromResult<Assignment>(null));

            var result = await _assignmentService.GetByIdAsync(id);
            result.Should().BeNull();

            _assignmentRepository.Verify(mock => mock.GetByAsync(It.IsAny<Expression<Func<Assignment, bool>>>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetAsyncShouldReturnObjectAsync()
        {
            var entity = new Assignment()
            {
                Id = Guid.NewGuid(),
                AssignedDate = DateTime.Now,
                State = State.WaitingAccept,
                AssetID = Guid.NewGuid(),
                CreatorId = Guid.NewGuid(),
                UserID = Guid.NewGuid(),
                RequestAssignmentId = null,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                Pubished = true
            };

            _assignmentRepository.Setup(x => x.GetByAsync(It.IsAny<Expression<Func<Assignment, bool>>>(), It.IsAny<string>())).Returns(Task.FromResult(entity));
            var result = await _assignmentService.GetByIdAsync(It.IsAny<Guid>());
            result.Should().NotBeNull();
            result.Id.Should().Be(entity.Id);

            _assignmentRepository.Verify(mock => mock.GetByAsync(It.IsAny<Expression<Func<Assignment, bool>>>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetAllAsyncShouldReturnEmptyListAsync()
        {
            List<Assignment> emptyList = new List<Assignment>();
            _assignmentRepository
                  .Setup(x => x.GetAllAsync())
                  .Returns(Task.FromResult(emptyList.AsEnumerable()));

            var result = await _assignmentService.GetAllAsync();
            result.Should().HaveCount(0);
            _assignmentRepository.Verify(mock => mock.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllAsyncShouldReturnObjectAsync()
        {
            var assignmentList = new List<Assignment>();

            for (int i = 0; i < 5; i++) {
                assignmentList.Add(
                    new Assignment()
                    {
                        Id = Guid.NewGuid(),
                        AssignedDate = DateTime.Now,
                        State = State.WaitingAccept,
                        AssetID = Guid.NewGuid(),
                        CreatorId = Guid.NewGuid(),
                        UserID = Guid.NewGuid(),
                        RequestAssignmentId = null,
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now,
                        Pubished = true
                    }
                );
            }

            _assignmentRepository.Setup(x => x.GetAllAsync()).Returns(Task.FromResult(assignmentList.AsEnumerable()));
            var result = await _assignmentService.GetAllAsync();

            result.Should().HaveCount(5);

            _assignmentRepository.Verify(mock => mock.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task AddAssignmentShouldThrowExceptionAsync()
        {
            Func<Task> act = async () => await _assignmentService.AddAsync(null, It.IsAny<Guid>());
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task AddAssignmentShouldBeSuccessfullyAsync()
        {
            var assignment = new Assignment()
            {
                Id = Guid.NewGuid(),
                AssignedDate = DateTime.Now,
                State = State.WaitingAccept,
                AssetID = Guid.NewGuid(),
                CreatorId = Guid.NewGuid(),
                UserID = Guid.NewGuid(),
                RequestAssignmentId = null,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                Pubished = true
            };

            var assignmentRequest = new AssignmentRequest()
            {
                UserID = Guid.NewGuid(),
                AssetID = Guid.NewGuid(),
                AssignedDate = DateTime.Now,
                Note = "this is test note"
            };

            _assignmentRepository.Setup(x => x.AddAsync(It.IsAny<Assignment>())).Returns(Task.FromResult(assignment));

            var result = await _assignmentService.AddAsync(assignmentRequest, It.IsAny<Guid>());

            result.Should().NotBeNull();

            _assignmentRepository.Verify(mock => mock.AddAsync(It.IsAny<Assignment>()), Times.Once());
        }

        [Fact]
        public async Task UpdateAssignmentShouldThrowExceptionAsync()
        {
            _assignmentRepository.Setup(x => x.GetByIdAsync(It.IsAny<object>())).Returns(Task.FromResult<Assignment>(null));
            Func<Task> act = async () => await _assignmentService.UpdateAsync(It.IsAny<Guid>(), It.IsAny<AssignmentRequest>());

            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task GetAssignmentByUserIdReturnEmptyListAsync()
        {
            List<Assignment> assigmentEntity = new List<Assignment>();
            var mock = assigmentEntity.AsQueryable().BuildMock();
            _assignmentRepository.Setup(x => x.Entities).Returns(mock.Object);

            var result = await _assignmentService.GetByUserIdAsync(It.IsAny<Guid>());

            result.Should().HaveCount(0);

            _assignmentRepository.Verify(mock => mock.Entities, Times.Once());
        }

        [Fact]
        public async Task GetAssignmentByUserIdShouldReturnObjectAsync()
        {
            var userId = Guid.NewGuid();
            List<Assignment> assigmentEntity = new List<Assignment>();
            List<Asset> assetEntity = new List<Asset>();
            for (int i = 0; i < 5; i++)
            {
                var assetId = Guid.NewGuid();
                var assignment = new Assignment()
                {
                    Id = Guid.NewGuid(),
                    AssignedDate = DateTime.Now.AddDays(-30),
                    State = State.WaitingAccept,
                    AssetID = assetId,
                    CreatorId = Guid.NewGuid(),
                    UserID = userId,
                    RequestAssignmentId = null,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    Pubished = true
                };
                if (i == 2) assignment.Pubished = false; //this line is not match the condition
                if (i == 3) assignment.UserID = Guid.NewGuid(); //this line is not match the condition
                if (i == 4) assignment.AssignedDate = DateTime.Now.AddDays(30); //this line is not match the condition

                var asset = new Asset()
                {
                    Id = assetId,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    Pubished = true,
                    Code = $"LP000{i}",
                    Name = $"DELL G{i}",
                    Location = "HN",
                    Specification = $"this is {i}",
                    CategoryId = Guid.NewGuid(),
                    InstalledDate = DateTime.Now.AddDays(-5)
                };
                asset.Category = new Category()
                {
                    Id = Guid.NewGuid(),
                    Name = $"Category{i}",
                    Prefix = $"Cat0{i}"
                };


                assigmentEntity.Add(assignment);
                assetEntity.Add(asset);
            }

            var mockAssignment = assigmentEntity.AsQueryable().BuildMock();
            var mockAsset = assetEntity.AsQueryable().BuildMock();

            _assignmentRepository.Setup(x => x.Entities).Returns(mockAssignment.Object);
            _assetRepository.Setup(x => x.Entities).Returns(mockAsset.Object);

            var result = await _assignmentService.GetByUserIdAsync(userId);

            result.Should().HaveCount(2);

            _assignmentRepository.Verify(mock => mock.Entities, Times.Once());
            _assetRepository.Verify(mock => mock.Entities, Times.Once());
        }

        [Fact]
        public async Task PagedQueryShouldReturnObjectAsync()
        {
            var userId = Guid.NewGuid();
            List<Assignment> assigmentEntity = new List<Assignment>();

            for (int i = 0; i < 9; i++)
            {
                var assetId = Guid.NewGuid();
                var assignment = new Assignment()
                {
                    Id = Guid.NewGuid(),
                    AssignedDate = DateTime.Now,
                    State = State.WaitingAccept,
                    AssetID = assetId,
                    CreatorId = Guid.NewGuid(),
                    UserID = userId,
                    RequestAssignmentId = null,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    Pubished = true
                };
                if (i == 5) assignment.Pubished = false; //this line is not match the condition
                if (i == 6) assignment.UserID = Guid.NewGuid(); //this line is not match the filter
                if (i == 7) {
                    assignment.AssignedDate = DateTime.Now.AddDays(15); //this line makes it comes first
                    assignment.State = State.Accepted; //this line is use to determine   
                }
                if (i == 4) assignment.UserID = Guid.NewGuid(); //this line is not match the filter
                if (i == 9) assignment.UserID = Guid.NewGuid(); //this line is not match the filter
                var asset = new Asset()
                {
                    Id = assetId,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    Pubished = true,
                    Code = $"LP000{i}",
                    Name = $"DELL G{i}",
                    Location = "HN",
                    Specification = $"this is {i}",
                    CategoryId = Guid.NewGuid(),
                    InstalledDate = DateTime.Now.AddDays(-5)
                };
                if (i == 4) asset.Name = "HP ba1535"; //this line is not match the filter

                assignment.Asset = asset;
                assigmentEntity.Add(assignment);
            }

            var filter = new FilterAssignmentModel()
            {
                OrderProperty = "AssignedDate",
                userFilter = new List<Guid>() { userId },
                Desc = true,
                KeySearch = "dell",
                Limit = 10
            };

            var mockAssigment = assigmentEntity.AsQueryable().BuildMock();

            _assignmentRepository.Setup(x => x.Entities).Returns(mockAssigment.Object);

            var result = await _assignmentService.PagedQueryAsync(filter);

            result.Items.Should().HaveCount(6);
            result.Items.FirstOrDefault().State.Should().Be(State.Accepted);
            result.TotalItems.Should().Be(6);
            result.TotalPages.Should().Be(1);

            _assignmentRepository.Verify(mock => mock.Entities, Times.Once());
        }

        [Fact]
        public async Task PagedQueryShouldPagingSuccessfullyAsync()
        {
            var userId = Guid.NewGuid();
            List<Assignment> assigmentEntity = new List<Assignment>();

            for (int i = 0; i < 9; i++)
            {
                var assetId = Guid.NewGuid();
                var assignment = new Assignment()
                {
                    Id = Guid.NewGuid(),
                    AssignedDate = DateTime.Now,
                    State = State.WaitingAccept,
                    AssetID = assetId,
                    CreatorId = Guid.NewGuid(),
                    UserID = userId,
                    RequestAssignmentId = null,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    Pubished = true
                };

                var asset = new Asset()
                {
                    Id = assetId,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    Pubished = true,
                    Code = $"LP000{i}",
                    Name = $"DELL G{i}",
                    Location = "HN",
                    Specification = $"this is {i}",
                    CategoryId = Guid.NewGuid(),
                    InstalledDate = DateTime.Now.AddDays(-5)
                };

                assignment.Asset = asset;
                assigmentEntity.Add(assignment);
            }

            var filter = new FilterAssignmentModel()
            {
                OrderProperty = "AssetName",
                Desc = true,
                Limit = 2,
                Page = 2
            };

            var mockAssigment = assigmentEntity.AsQueryable().BuildMock();

            _assignmentRepository.Setup(x => x.Entities).Returns(mockAssigment.Object);

            var result = await _assignmentService.PagedQueryAsync(filter);

            result.Items.Should().HaveCount(2);
            result.Items.FirstOrDefault().AssetName.Should().Be("DELL G6");
            result.TotalItems.Should().Be(9);
            result.TotalPages.Should().Be(5);

            _assignmentRepository.Verify(mock => mock.Entities, Times.Once());
        }

        [Fact]
        public async Task AcceptRespondShouldReturnNullAsync()
        {
            var assignment = new Assignment()
            {
                Id = Guid.NewGuid(),
                AssignedDate = DateTime.Now,
                State = State.Accepted,
                AssetID = Guid.NewGuid(),
                CreatorId = Guid.NewGuid(),
                UserID = Guid.NewGuid(),
                RequestAssignmentId = null,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                Pubished = true
            };
            _assignmentRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).Returns(Task.FromResult(assignment));

            var result = await _assignmentService.AcceptRespond(It.IsAny<Guid>());

            result.Should().BeNull();

            _assignmentRepository.Verify(mock => mock.GetByIdAsync(It.IsAny<Guid>()), Times.Once());
        }

        [Fact]
        public async Task AcceptRespondShouldReturnObjectAsync()
        {
            var assignment = new Assignment()
            {
                Id = Guid.NewGuid(),
                AssignedDate = DateTime.Now,
                State = State.WaitingAccept,
                AssetID = Guid.NewGuid(),
                CreatorId = Guid.NewGuid(),
                UserID = Guid.NewGuid(),
                RequestAssignmentId = null,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                Pubished = true
            };
            _assignmentRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).Returns(Task.FromResult(assignment));

            var result = await _assignmentService.AcceptRespond(It.IsAny<Guid>());

            result.State.Should().Be(State.Accepted);

            _assignmentRepository.Verify(mock => mock.GetByIdAsync(It.IsAny<Guid>()), Times.Once());
        }

        [Fact]
        public async Task GetHistoryAssignmentByIdShouldReturnObjectAsync()
        {
            var userId = Guid.NewGuid();
            var assetId = Guid.NewGuid();
            List<Assignment> assigmentEntity = new List<Assignment>();
            var asset = new Asset()
            {
                Id = assetId,
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

            for (int i = 0; i < 5; i++)
            {
                
                var assignment = new Assignment()
                {
                    Id = Guid.NewGuid(),
                    AssignedDate = DateTime.Now.AddDays(i), //for orderBy testing
                    State = State.WaitingAccept,
                    AssetID = assetId,
                    CreatorId = Guid.NewGuid(),
                    UserID = userId,
                    RequestAssignmentId = null,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    Pubished = true
                };
                if (i == 0) {
                    assignment.AssetID = Guid.NewGuid();
                    asset.Id = assignment.AssetID;
                } //this line is not match the condition

                var requestAssignment = new RequestAssignment()
                {
                    Id = Guid.NewGuid(),
                    ReturnDate = DateTime.Now,
                    UserAcceptId = Guid.NewGuid(),
                    UserRequestId = Guid.NewGuid(),
                };

                assignment.RequestAssignment = requestAssignment;
                assignment.Asset = asset;
                if (i == 0) asset.Id = assetId;
                assigmentEntity.Add(assignment);
            }

            var mockAssigment = assigmentEntity.AsQueryable().BuildMock();

            _assignmentRepository.Setup(x => x.Entities).Returns(mockAssigment.Object);

            var result = await _assignmentService.GetHistoryAssignmentById(assetId);

            result.Should().HaveCount(3);
            result.FirstOrDefault().AssignedDate.Date.Should().Be(DateTime.Now.AddDays(4).Date);

            _assignmentRepository.Verify(mock => mock.Entities, Times.Once());
        }

        [Fact]
        public async Task GetHistoryAssignmentByIdShouldReturnEmptyListAsync()
        {
            var userId = Guid.NewGuid();
            var assetId = Guid.NewGuid();
            List<Assignment> assigmentEntity = new List<Assignment>();
            var asset = new Asset()
            {
                Id = assetId,
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

            for (int i = 0; i < 5; i++)
            {

                var assignment = new Assignment()
                {
                    Id = Guid.NewGuid(),
                    AssignedDate = DateTime.Now.AddDays(i),
                    State = State.WaitingAccept,
                    AssetID = assetId,
                    CreatorId = Guid.NewGuid(),
                    UserID = userId,
                    RequestAssignmentId = null,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    Pubished = true
                };

                var requestAssignment = new RequestAssignment()
                {
                    Id = Guid.NewGuid(),
                    ReturnDate = DateTime.Now,
                    UserAcceptId = Guid.NewGuid(),
                    UserRequestId = Guid.NewGuid(),
                };

                assignment.RequestAssignment = requestAssignment;
                assignment.Asset = asset;
                assigmentEntity.Add(assignment);
            }

            var mockAssigment = assigmentEntity.AsQueryable().BuildMock();

            _assignmentRepository.Setup(x => x.Entities).Returns(mockAssigment.Object);

            var result = await _assignmentService.GetHistoryAssignmentById(Guid.NewGuid());

            result.Should().HaveCount(0);

            _assignmentRepository.Verify(mock => mock.Entities, Times.Once());
        }

        [Fact]
        public async Task ExistAsyncShouldReturnBoolAsync()
        {
            _assignmentRepository.Setup(x => x.ExistAsync(It.IsAny<Expression<Func<Assignment, bool>>>())).Returns(Task.FromResult(true));

            var result = await _assignmentService.ExistAsync(It.IsAny<Expression<Func<Assignment, bool>>>());

            result.Should().BeTrue();

            _assignmentRepository.Verify(mock => mock.ExistAsync(It.IsAny<Expression<Func<Assignment, bool>>>()), Times.Once());
        }

    }
}
