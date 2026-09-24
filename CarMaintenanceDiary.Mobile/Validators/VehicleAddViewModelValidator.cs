using CarMaintenanceDiary.Mobile.ViewModels;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarMaintenanceDiary.Mobile.Validators
{
    public class VehicleAddViewModelValidator : AbstractValidator<VehicleAddViewModel>
    {
        public VehicleAddViewModelValidator()
        {
            RuleFor(x => x.Make)
                .NotEmpty().WithMessage("Make is required");

            RuleFor(x => x.Model)
                .NotEmpty().WithMessage("Model is required");

            RuleFor(x => x.Year)
                .NotEmpty().WithMessage("Year is required")
                .Matches(@"^\d{4}$").WithMessage("Enter a valid 4-digit year");

            RuleFor(x => x.LicensePlate)
                .NotEmpty().WithMessage("License plate is required");

            RuleFor(x => x.Vin)
                .NotEmpty().WithMessage("VIN is required")
                .Length(11, 17).WithMessage("VIN must be between 11 and 17 characters");
        }
    }
}
