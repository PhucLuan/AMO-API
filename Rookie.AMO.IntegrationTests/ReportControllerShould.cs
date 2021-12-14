using AutoMapper;
using ClosedXML.Excel;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rookie.AMO.Business;
using Rookie.AMO.Business.Interfaces;
using Rookie.AMO.Business.Services;
using Rookie.AMO.Contracts.Dtos;
using Rookie.AMO.DataAccessor.Entities;
using Rookie.AMO.IntegrationTests.Common;
using Rookie.AMO.Tests;
using Rookie.AMO.WebApi.Controllers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace Rookie.AMO.IntegrationTests
{
    public class ReportControllerShould : IClassFixture<SqliteInMemoryFixture>
    {
        private readonly SqliteInMemoryFixture _fixture;
        private readonly IBaseRepository<Category> _categoryRepository;
        private readonly IBaseRepository<Asset> _assetRepository;
        private readonly IMapper _mapper;

        public ReportControllerShould(SqliteInMemoryFixture fixture)
        {
            _fixture = fixture;
            _fixture.CreateDatabase();

            _categoryRepository = new BaseRepository<Category>(_fixture.Context);
            _assetRepository = new BaseRepository<Asset>(_fixture.Context);

            var config = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public async Task Get_Report_Success()
        {
            // Arrange
            var reportService = new ReportService(_assetRepository, _mapper);
            var reportController = new ReportController(reportService);

            Array values = Enum.GetValues(typeof(State));
            Random random = new Random();
            var listState = new List<State> { State.Assigned, State.Available, State.NotAvailable, State.WaitingForRecycle, State.Recycled};
            var lpId = Guid.NewGuid();
            var pcId = Guid.NewGuid();
            var moId = Guid.NewGuid();
            var msId = Guid.NewGuid();

            List<Category> categoryList = new List<Category>
            { 
                new Category{ Id = lpId, Name = "Laptop", Prefix = "LP"},
                new Category{ Id = pcId, Name = "Personal Computer", Prefix = "PC"},
                new Category{ Id = moId, Name = "Monitor", Prefix = "MO"},
                new Category{ Id = msId, Name = "Mouse", Prefix = "MS"}
            };

            foreach (var category in categoryList)
            {
                await _categoryRepository.AddAsync(category);
            }
            var state = 0;
            List<Asset> assetList = new List<Asset>();
            for(int i =1; i <= 10; i++)
            {
                if (i >= 5)
                {
                    state = i % 4;
                }
                else
                {
                    state = i;
                }

                var asset = new Asset()
                {
                    Id = Guid.NewGuid(),
                    State = listState[state],
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    Pubished = true,
                    Code = $"LP000" + i,
                    Name = $"DELL G" + i,
                    Location = "HN",
                    Specification = $"this is " + i,
                    CategoryId = categoryList[random.Next(categoryList.Count)].Id,
                    InstalledDate = DateTime.Now.AddDays(-5),
                    CreatorId = Guid.NewGuid(),
                };
                assetList.Add(asset);
            }

            foreach (var asset in assetList)
            {
                await _assetRepository.AddAsync(asset);
            }

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                                        new Claim("location", "HN")
                                   }, "TestAuthentication"));
            reportController.ControllerContext = new ControllerContext();
            reportController.ControllerContext.HttpContext = new DefaultHttpContext { User = user };

            // Act
            var result = await reportController.Index();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(4);
        }

        [Fact]
        public async Task Export_Report_Success()
        {
            // Arrange
            var reportService = new ReportService(_assetRepository, _mapper);
            var reportController = new ReportController(reportService);

            Array values = Enum.GetValues(typeof(State));
            Random random = new Random();
            var listState = new List<State> { State.Assigned, State.Available, State.NotAvailable, State.WaitingForRecycle, State.Recycled };
            var lpId = Guid.NewGuid();
            var pcId = Guid.NewGuid();
            var moId = Guid.NewGuid();
            var msId = Guid.NewGuid();

            List<Category> categoryList = new List<Category>
            {
                new Category{ Id = lpId, Name = "Laptop", Prefix = "LP"},
                new Category{ Id = pcId, Name = "Personal Computer", Prefix = "PC"},
                new Category{ Id = moId, Name = "Monitor", Prefix = "MO"},
                new Category{ Id = msId, Name = "Mouse", Prefix = "MS"}
            };

            foreach (var category in categoryList)
            {
                await _categoryRepository.AddAsync(category);
            }
            var state = 0;
            List<Asset> assetList = new List<Asset>();
            for (int i = 1; i <= 10; i++)
            {
                if (i >= 5)
                {
                    state = i % 4;
                }
                else
                {
                    state = i;
                }

                var asset = new Asset()
                {
                    Id = Guid.NewGuid(),
                    State = listState[state],
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    Pubished = true,
                    Code = $"LP000" + i,
                    Name = $"DELL G" + i,
                    Location = "HN",
                    Specification = $"this is " + i,
                    CategoryId = categoryList[random.Next(categoryList.Count)].Id,
                    InstalledDate = DateTime.Now.AddDays(-5),
                    CreatorId = Guid.NewGuid(),
                };
                assetList.Add(asset);
            }

            foreach (var asset in assetList)
            {
                await _assetRepository.AddAsync(asset);
            }

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                                        new Claim("location", "HN")
                                   }, "TestAuthentication"));
            reportController.ControllerContext = new ControllerContext();
            reportController.ControllerContext.HttpContext = new DefaultHttpContext { User = user };

            // Act
            var result = await reportController.ExportReport();
            var file = result as FileContentResult;

            // Assert
            file.FileContents.Should().NotBeEmpty();
            result.Should().NotBeNull();
        }
    }
}