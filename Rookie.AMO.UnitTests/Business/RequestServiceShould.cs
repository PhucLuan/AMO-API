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
using System.Linq.Expressions;
using Rookie.AMO.Contracts.Dtos.Assignment;

namespace Rookie.AMO.UnitTests.Business
{
    /// <summary>
    /// https://codewithshadman.com/repository-pattern-csharp/
    /// https://dotnettutorials.net/lesson/generic-repository-pattern-csharp-mvc/
    /// https://fluentassertions.com/exceptions/
    /// https://stackoverflow.com/questions/37422476/moq-expression-with-constraint-it-isexpressionfunct-bool
    /// </summary>
    public class RequestServiceShould
    {
        private readonly RequestService _requestService;
        private readonly Mock<IBaseRepository<RequestAssignment>> _requestRepository;
        private readonly Mock<IBaseRepository<Assignment>> _assignmentRepository;
        private readonly Mock<IBaseRepository<Asset>> _assetRepository;
        private readonly IMapper _mapper;

        public RequestServiceShould()
        {
            _requestRepository = new Mock<IBaseRepository<RequestAssignment>>();
            _assignmentRepository = new Mock<IBaseRepository<Assignment>>();
            _assetRepository = new Mock<IBaseRepository<Asset>>();

            var config = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperProfile>());
            _mapper = config.CreateMapper();

            _requestService = new RequestService(
                _requestRepository.Object,
                _assignmentRepository.Object,
                _assetRepository.Object,
                _mapper
            );
        }

        [Fact]
        public async Task GetAsyncShouldReturnNullAsync()
        {
            var id = Guid.NewGuid();
            _requestRepository
                  .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                  .Returns(Task.FromResult<RequestAssignment>(null));

            var result = await _requestService.GetByIdAsync(id);
            result.Should().BeNull();

            _requestRepository.Verify(mock => mock.GetByIdAsync(It.IsAny<Guid>()), Times.Once);
        }

        [Fact]
        public async Task GetAsyncShouldReturnObjectAsync()
        {
            var requestId = Guid.NewGuid();
            var entity = new Assignment()
            {
                Id = Guid.NewGuid(),
                AssignedDate = DateTime.Now,
                State = State.WaitingAccept,
                AssetID = Guid.NewGuid(),
                CreatorId = Guid.NewGuid(),
                UserID = Guid.NewGuid(),
                RequestAssignmentId = requestId,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                Pubished = true
            };

            var request = new RequestAssignment()
            {
                Id = requestId,
                State = State.WaitingForReturning,
                ReturnDate = null,
                UserRequestId = Guid.NewGuid()
            };

            _requestRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).Returns(Task.FromResult(request));
            var result = await _requestService.GetByIdAsync(It.IsAny<Guid>());
            result.Should().NotBeNull();
            result.Id.Should().Be(request.Id);

            _requestRepository.Verify(mock => mock.GetByIdAsync(It.IsAny<Guid>()), Times.Once);
        }

        [Fact]
        public async Task GetAllAsyncShouldReturnEmptyListAsync()
        {
            List<RequestAssignment> emptyList = new List<RequestAssignment>();
            _requestRepository
                  .Setup(x => x.GetAllAsync())
                  .Returns(Task.FromResult(emptyList.AsEnumerable()));

            var result = await _requestService.GetAllAsync();
            result.Should().HaveCount(0);
            _requestRepository.Verify(mock => mock.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllAsyncShouldReturnObjectAsync()
        {
            var assignmentList = new List<Assignment>();
            var requestList = new List<RequestAssignment>();

            for (int i = 0; i < 5; i++)
            {
                var requestId = Guid.NewGuid();
                assignmentList.Add(
                    new Assignment()
                    {
                        Id = Guid.NewGuid(),
                        AssignedDate = DateTime.Now,
                        State = State.Accepted,
                        AssetID = Guid.NewGuid(),
                        CreatorId = Guid.NewGuid(),
                        UserID = Guid.NewGuid(),
                        RequestAssignmentId = requestId,
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now,
                        Pubished = true
                    }
                );
                requestList.Add(
                    new RequestAssignment()
                    {
                        Id = requestId,
                        Assignment = assignmentList[i],
                        State = State.WaitingForReturning,
                        UserRequestId = Guid.NewGuid()
                    }
                );
            }

            _requestRepository.Setup(x => x.GetAllAsync()).Returns(Task.FromResult(requestList.AsEnumerable()));
            var result = await _requestService.GetAllAsync();

            result.Should().HaveCount(5);

            _requestRepository.Verify(mock => mock.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateRequestShouldThrowExceptionAsync()
        {
            Func<Task> act = async () => await _requestService.AddAsync(null);
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task CreateRequestShouldBeSuccessfullyAsync()
        {
            var assignmentId = Guid.NewGuid();
            var assignment = new Assignment()
            {
                Id = assignmentId,
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

            var request = new RequestAssignment()
            {
                Id = Guid.NewGuid(),
                State = State.WaitingForReturning,
                UserRequestId = Guid.NewGuid()
            };

            var assignmentRequest = new AssignmentRequest()
            {
                UserID = Guid.NewGuid(),
                AssetID = Guid.NewGuid(),
                AssignedDate = DateTime.Now,
                Note = "this is test note"
            };

            var requestReturn = new RequestReturn()
            {
                Id = assignmentId,
                UserId = Guid.NewGuid(),
            };

            _requestRepository.Setup(x => x.AddAsync(It.IsAny<RequestAssignment>())).Returns(Task.FromResult(request));
            _assignmentRepository.Setup(x => x.GetByIdAsync(assignmentId)).Returns(Task.FromResult(assignment));
            //_assignmentRepository.Setup(x => x.UpdateAsync(assignment));

            var result = await _requestService.AddAsync(requestReturn);

            result.Should().NotBeNull();

            _requestRepository.Verify(mock => mock.AddAsync(It.IsAny<RequestAssignment>()), Times.Once());
        }

        [Fact]
        public async Task AcceptRequestShouldSuccessfullyAsync()
        {
            List<Assignment> assigmentEntity = new List<Assignment>();
            var assetId = Guid.NewGuid();
            var requestId = Guid.NewGuid();

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
                CategoryId = Guid.NewGuid(),
                InstalledDate = DateTime.Now.AddDays(-5)
            };

            var assignment = new Assignment()
            {
                Id = Guid.NewGuid(),
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
            assigmentEntity.Add(assignment);

            var request = new RequestAssignment()
            {
                Id = requestId,
                UserRequestId = Guid.NewGuid(),
                State = State.WaitingForReturning,
                ReturnDate = DateTime.Now,
                UserAcceptId = Guid.NewGuid()
            };

            _requestRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).Returns(Task.FromResult(request));

            var mockAssigment = assigmentEntity.AsQueryable().BuildMock();
            _assignmentRepository.Setup(x => x.Entities).Returns(mockAssigment.Object);

            _assetRepository.Setup(x => x.GetByIdAsync(assetId)).Returns(Task.FromResult(asset));

            var result = await _requestService.AcceptRespond(requestId, It.IsAny<Guid>());

            result.Should().NotBeNull();

            _requestRepository.Verify(mock => mock.GetByIdAsync(It.IsAny<Guid>()), Times.Once());
        }

        [Fact]
        public async Task DeclineRequestShouldSuccessfullyAsync()
        {
            List<Assignment> assigmentEntity = new List<Assignment>();
            var requestId = Guid.NewGuid();

            var assignment = new Assignment()
            {
                Id = Guid.NewGuid(),
                AssignedDate = DateTime.Now,
                State = State.Accepted,
                AssetID = Guid.NewGuid(),
                CreatorId = Guid.NewGuid(),
                UserID = Guid.NewGuid(),
                RequestAssignmentId = requestId,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                Pubished = true
            };
            assigmentEntity.Add(assignment);

            var request = new RequestAssignment()
            {
                Id = requestId,
                UserRequestId = Guid.NewGuid(),
                State = State.WaitingForReturning,
                ReturnDate = DateTime.Now,
                UserAcceptId = Guid.NewGuid()
            };

            _requestRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).Returns(Task.FromResult(request));

            var mockAssigment = assigmentEntity.AsQueryable().BuildMock();
            _assignmentRepository.Setup(x => x.Entities).Returns(mockAssigment.Object);

            await _requestService.DeleteAsync(requestId);

            //result.Should().BeNull();

            _requestRepository.Verify(mock => mock.DeleteAsync(requestId), Times.Once());
        }

        [Fact]
        public async Task PagedQueryShouldReturnObjectAsync()
        {
            var userId = Guid.NewGuid();
            List<Assignment> assigmentEntity = new List<Assignment>();
            List<RequestAssignment> requestEntity = new List<RequestAssignment>();
            for (int i = 0; i < 9; i++)
            {
                var assetId = Guid.NewGuid();
                var requestId = Guid.NewGuid();
                var assignment = new Assignment()
                {
                    Id = Guid.NewGuid(),
                    AssignedDate = DateTime.Now,
                    State = State.WaitingAccept,
                    AssetID = assetId,
                    CreatorId = Guid.NewGuid(),
                    UserID = userId,
                    RequestAssignmentId = requestId,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    Pubished = true
                };
                var request = new RequestAssignment()
                {
                    Id = requestId,
                    State = State.WaitingForReturning,
                    UserRequestId = Guid.NewGuid()
                };
                if (i == 5) assignment.Pubished = false; //this line is not match the condition
                if (i == 6) assignment.UserID = Guid.NewGuid(); //this line is not match the filter
                if (i == 7)
                {
                    assignment.AssignedDate = DateTime.Now.AddDays(15); //this line makes it comes first
                    assignment.State = State.Accepted; //this line is use to determine   
                    request.UserAcceptId = Guid.NewGuid();
                    request.ReturnDate = DateTime.Now;
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
                request.Assignment = assignment;
                requestEntity.Add(request);
            }

            var filter = new FilterRequestsModel()
            {
                OrderProperty = "ReturnDate",
                userFilter = new List<Guid>() { userId },
                Desc = true,
                KeySearch = "",
                Limit = 10
            };

            var mockRequest = requestEntity.AsQueryable().BuildMock();

            _requestRepository.Setup(x => x.Entities).Returns(mockRequest.Object);

            var result = await _requestService.PagedQueryAsync(filter);

            result.Items.Should().HaveCount(9);
            result.Items.FirstOrDefault().State.Should().Be(State.WaitingForReturning);
            result.TotalItems.Should().Be(9);
            result.TotalPages.Should().Be(1);

            _requestRepository.Verify(mock => mock.Entities, Times.Once());
        }

        [Fact]
        public async Task PagedQueryShouldPagingSuccessfullyAsync()
        {
            var userId = Guid.NewGuid();
            List<Assignment> assigmentEntity = new List<Assignment>();
            List<RequestAssignment> requestEntity = new List<RequestAssignment>();

            for (int i = 0; i < 9; i++)
            {
                var assetId = Guid.NewGuid();
                var requestId = Guid.NewGuid();
                var assignment = new Assignment()
                {
                    Id = Guid.NewGuid(),
                    AssignedDate = DateTime.Now,
                    State = State.WaitingAccept,
                    AssetID = assetId,
                    CreatorId = Guid.NewGuid(),
                    UserID = userId,
                    RequestAssignmentId = requestId,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    Pubished = true
                };
                var request = new RequestAssignment()
                {
                    Id = requestId,
                    State = State.WaitingForReturning,
                    UserRequestId = Guid.NewGuid()
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
                request.Assignment = assignment;
                requestEntity.Add(request);
            }

            var filter = new FilterRequestsModel()
            {
                OrderProperty = "AssetName",
                Desc = true,
                Limit = 2,
                Page = 2
            };

            var mockRequest = requestEntity.AsQueryable().BuildMock();

            _requestRepository.Setup(x => x.Entities).Returns(mockRequest.Object);

            var result = await _requestService.PagedQueryAsync(filter);

            result.Items.Should().HaveCount(2);
            result.Items.FirstOrDefault().AssetName.Should().Be("DELL G2");
            result.TotalItems.Should().Be(9);
            result.TotalPages.Should().Be(5);

            _requestRepository.Verify(mock => mock.Entities, Times.Once());
        }

    }
}
