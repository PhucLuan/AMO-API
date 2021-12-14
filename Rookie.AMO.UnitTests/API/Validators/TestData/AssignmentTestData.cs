using Rookie.AMO.Contracts.Constants;
using Rookie.AMO.Contracts.Dtos;
using Rookie.AMO.Contracts.Dtos.Assignment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rookie.AMO.UnitTests.API.Validators.TestData
{
    public class AssignmentTestData
    {
        public static IEnumerable<object[]> ValidNotes()
        {
            return new object[][]
            {
                new object[] { "note" },
                new object[] { "note @#$%" },
            };
        }

        public static IEnumerable<object[]> ValidId()
        {
            return new object[][]
            {
                new object[] { Guid.NewGuid() },
            };
        }

        public static IEnumerable<object[]> ValidDates()
        {
            return new object[][]
            {
                new object[] { DateTime.Now },
                new object[] { DateTime.Now.AddDays(30) },
            };
        }

        public static IEnumerable<object[]> InvalidUserId()
        {
            return new object[][]
            {
                new object[] { null, string.Format(ErrorTypes.Common.RequiredError, nameof(AssignmentRequest.UserID)) },
            };
        }

        public static IEnumerable<object[]> InvalidAssetId()
        {
            return new object[][]
            {
                new object[] { null, string.Format(ErrorTypes.Common.RequiredError, nameof(AssignmentRequest.AssetID)) },
            };
        }

        public static IEnumerable<object[]> InvalidDates()
        {
            return new object[][]
            {
                new object[] { DateTime.Now.AddDays(-30), ErrorTypes.Assignment.ToDateGreaterThanFromDateError },
                new object[] { null, string.Format(ErrorTypes.Common.RequiredError, nameof(AssignmentRequest.AssignedDate)) },
            };
        }

        public static IEnumerable<object[]> InvalidNotes()
        {
            string note101 = "YEHgBd0HeyGkX02ABoWnB2fL5bjmSaGalBArUgrCJ1U4K34L7SN3ZFIa3FvfdWyWctJWXc4KXtwuP122uBY57mFiP122uBY57mFii";
            for (int i = 0; i < 10; i++) {
                note101 = note101 + note101;
            }
            return new object[][]
            {
                new object[] { note101, string.Format(ErrorTypes.Common.MaxLengthError, ValidationRules.AssignmentRules.MaxLenghCharactersForNote) },
            };
        }
    }
}
