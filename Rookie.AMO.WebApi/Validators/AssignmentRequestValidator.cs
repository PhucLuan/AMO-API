using FluentValidation;
using Rookie.AMO.Contracts.Constants;
using Rookie.AMO.Contracts.Dtos.Assignment;
using System;

namespace Rookie.AMO.WebApi.Validators
{

    public class AssignmentRequestValidator : BaseValidator<AssignmentRequest>
    {
        public AssignmentRequestValidator()
        {
            RuleFor(m => m.AssignedDate)
                 .NotEmpty()
                 .WithMessage(x => string.Format(ErrorTypes.Common.RequiredError, nameof(x.AssignedDate)));

            RuleFor(m => m.UserID)
                  .NotEmpty()
                  .WithMessage(x => string.Format(ErrorTypes.Common.RequiredError, nameof(x.UserID)));

            RuleFor(m => m.AssetID)
              .NotEmpty()
              .WithMessage(x => string.Format(ErrorTypes.Common.RequiredError, nameof(x.AssetID)));

            RuleFor(x => x.AssignedDate)
                .Must(BeNotLessThanToday)
                .WithMessage(ErrorTypes.Assignment.ToDateGreaterThanFromDateError);

            RuleFor(m => m.Note)
               .MaximumLength(ValidationRules.AssignmentRules.MaxLenghCharactersForNote)
               .WithMessage(string.Format(ErrorTypes.Common.MaxLengthError, ValidationRules.AssignmentRules.MaxLenghCharactersForNote))
               .When(m => !string.IsNullOrWhiteSpace(m.Note));
        }

        private bool BeNotLessThanToday(DateTime AssignedTo)
        {
            var today = DateTime.Now;

            return AssignedTo.Date >= today.Date;
        }
    }
}
