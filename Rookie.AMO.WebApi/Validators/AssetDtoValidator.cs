using FluentValidation;
using Rookie.AMO.Business.Interfaces;
using Rookie.AMO.Contracts.Constants;
using Rookie.AMO.Contracts.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rookie.AMO.WebApi.Validators
{
    public class AssetDtoValidator : BaseValidator<AssetDto>
    {
        public AssetDtoValidator(ICategoryService categoryService)
        {
            RuleFor(m => m.CategoryId)
                 .NotNull()
                 .WithMessage(x => string.Format(ErrorTypes.Common.RequiredError, nameof(x.Id)));

            RuleFor(m => m.Name)
                  .NotEmpty()
                  .WithMessage(x => string.Format(ErrorTypes.Common.RequiredError, nameof(x.Name)));

            RuleFor(m => m.Specification)
                  .NotEmpty()
                  .WithMessage(x => string.Format(ErrorTypes.Common.RequiredError, nameof(x.Specification)));

            RuleFor(m => m.State)
                  .NotEmpty()
                  .WithMessage(x => string.Format(ErrorTypes.Common.RequiredError, nameof(x.State)));

            RuleFor(m => m.InstalledDate)
                  .NotEmpty()
                  .WithMessage(x => string.Format(ErrorTypes.Common.RequiredError, nameof(x.InstalledDate)));

            RuleFor(x => x).MustAsync(
             async (dto, cancellation) =>
             {
                 var exit = await categoryService.GetByIdAsync(dto.CategoryId);
                 return exit != null && !String.IsNullOrEmpty(dto.Name);
             }
            ).WithMessage("Category is not exist");
        }
    }
}
