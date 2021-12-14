using AutoMapper;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using Moq;
using Rookie.AMO.Business;
using Rookie.AMO.Business.Interfaces;
using Rookie.AMO.Business.Services;
using Rookie.AMO.Contracts.Dtos;
using Rookie.AMO.Tests.Validations;
using Rookie.AMO.UnitTests.API.Validators.TestData;
using Rookie.AMO.WebApi.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Rookie.AMO.UnitTests.API.Validators
{
    public class AssetDtoValidatorShould : BaseValidatorShould
    {
        private readonly ValidationTestRunner<AssetDtoValidator, AssetDto> _testRunner;
        private readonly Mock<ICategoryService> _categoryService;
        public AssetDtoValidatorShould()
        {
            _categoryService = new Mock<ICategoryService>();

            _testRunner = ValidationTestRunner
                .Create<AssetDtoValidator, AssetDto>(new AssetDtoValidator(_categoryService.Object));
        }
        [Theory]
        [MemberData(nameof(AssetTestData.ValidTexts), MemberType = typeof(AssetTestData))]
        public void NotHaveErrorWhenNameIsvalid(string name) =>
            _testRunner
                .For(m => m.Name = name)
                .ShouldNotHaveErrorsFor(m => m.Name);
        [Theory]
        [MemberData(nameof(AssetTestData.InvalidNames), MemberType = typeof(AssetTestData))]
        public void HaveErrorWhenNameIsInvalid(string name, string errorMessage) =>
            _testRunner
                .For(m => m.Name = name)
                .ShouldHaveErrorsFor(m => m.Name, errorMessage);

        [Theory]
        [MemberData(nameof(AssetTestData.ValidTexts), MemberType = typeof(AssetTestData))]
        public void NotHaveErrorWhenSpecificationIsvalid(string specification) =>
            _testRunner
                .For(m => m.Specification = specification)
                .ShouldNotHaveErrorsFor(m => m.Specification);
        [Theory]
        [MemberData(nameof(AssetTestData.InvalidSpecification), MemberType = typeof(AssetTestData))]
        public void HaveErrorWhenSpecificationIsInvalid(string specification, string errorMessage) =>
                    _testRunner
                        .For(m => m.Specification = specification)
                        .ShouldHaveErrorsFor(m => m.Specification, errorMessage);
    }
}
