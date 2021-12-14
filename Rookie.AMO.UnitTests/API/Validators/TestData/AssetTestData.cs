using Rookie.AMO.Contracts.Constants;
using Rookie.AMO.Contracts.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rookie.AMO.UnitTests.API.Validators.TestData
{
    public class AssetTestData
    {
        public static IEnumerable<object[]> ValidTexts()
        {
            return new object[][]
            {
                new object[] { "asset name" },
                new object[] { "asset" },
            };
        }

        public static IEnumerable<object[]> InvalidNames()
        {
            return new object[][]
            {
                new object[] { "", string.Format(ErrorTypes.Common.RequiredError, nameof(AssetDto.Name))},
                new object[] { null, string.Format(ErrorTypes.Common.RequiredError, nameof(AssetDto.Name))},
            };
        }
        public static IEnumerable<object[]> InvalidSpecification()
        {
            return new object[][]
            {
                new object[] { "", string.Format(ErrorTypes.Common.RequiredError, nameof(AssetDto.Specification))},
                new object[] { null, string.Format(ErrorTypes.Common.RequiredError, nameof(AssetDto.Specification))},
            };
        }
    }
}
