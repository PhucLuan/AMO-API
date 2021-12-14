using FluentAssertions;
using FluentValidation.Results;
using Rookie.AMO.Contracts.Dtos;
using Rookie.AMO.Contracts.Dtos.Assignment;
using Rookie.AMO.Tests.Validations;
using Rookie.AMO.UnitTests.API.Validators.TestData;
using Rookie.AMO.WebApi.Validators;
using System;
using System.Linq;
using Xunit;

namespace Rookie.AMO.UnitTests.API.Validators
{
    public class AssignmentDtoValidatorShould : BaseValidatorShould
    {
        private readonly ValidationTestRunner<AssignmentRequestValidator, AssignmentRequest> _testRunner;

        public AssignmentDtoValidatorShould()
        {
            _testRunner = ValidationTestRunner
                .Create<AssignmentRequestValidator, AssignmentRequest>(new AssignmentRequestValidator());
        }

        [Theory]
        [MemberData(nameof(AssignmentTestData.ValidDates), MemberType = typeof(AssignmentTestData))]
        public void NotHaveErrorWhenAssignedDateIsValid(DateTime date) =>
            _testRunner
                .For(m => m.AssignedDate = date)
                .ShouldNotHaveErrorsFor(m => m.AssignedDate);

        [Theory]
        [MemberData(nameof(AssignmentTestData.InvalidDates), MemberType = typeof(AssignmentTestData))]
        public void HaveErrorWhenAssignedDateIsInvalid(DateTime date, string errorMessage) =>
            _testRunner
                .For(m => m.AssignedDate = date)
                .ShouldHaveErrorsFor(m => m.AssignedDate, errorMessage);

        [Theory]
        [MemberData(nameof(AssignmentTestData.ValidNotes), MemberType = typeof(AssignmentTestData))]
        public void NotHaveErrorWhenNoteIsValid(string note) =>
            _testRunner
                .For(m => m.Note = note)
                .ShouldNotHaveErrorsFor(m => m.Note);

        [Theory]
        [MemberData(nameof(AssignmentTestData.InvalidNotes), MemberType = typeof(AssignmentTestData))]
        public void HaveErrorWhenNoteIsInvalid(string note, string errorMessage) =>
            _testRunner
                .For(m => m.Note = note)
                .ShouldHaveErrorsFor(m => m.Note, errorMessage);

        [Theory]
        [MemberData(nameof(AssignmentTestData.ValidId), MemberType = typeof(AssignmentTestData))]
        public void NotHaveErrorWhenAssetIDIsvalid(Guid id) =>
           _testRunner
               .For(m => m.AssetID = id)
               .ShouldNotHaveErrorsFor(m => m.AssetID);

        [Theory]
        [MemberData(nameof(AssignmentTestData.ValidId), MemberType = typeof(AssignmentTestData))]
        public void NotHaveErrorWhenUserIDIsvalid(Guid id) =>
           _testRunner
               .For(m => m.UserID = id)
               .ShouldNotHaveErrorsFor(m => m.UserID);

        [Theory]
        [MemberData(nameof(AssignmentTestData.InvalidAssetId), MemberType = typeof(AssignmentTestData))]
        public void HaveErrorWhenAssetIDIsInvalid(Guid id, string errorMessage) =>
           _testRunner
               .For(m => m.AssetID = id)
               .ShouldHaveErrorsFor(m => m.AssetID, errorMessage);

        [Theory]
        [MemberData(nameof(AssignmentTestData.InvalidUserId), MemberType = typeof(AssignmentTestData))]
        public void HaveErrorWhenUserIDIsInvalid(Guid id, string errorMessage) =>
           _testRunner
               .For(m => m.UserID = id)
               .ShouldHaveErrorsFor(m => m.UserID, errorMessage);
    }
}
