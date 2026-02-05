using BookSystem.Requests;
using FluentValidation;
using System.Data;

namespace BookSystem.Validators
{
    public class CreateFolderValidator:AbstractValidator<CreateFolderRequest>
    {

        public CreateFolderValidator() 
        {
            RuleFor(x => x.FolderName).NotEmpty()
             .WithMessage("Description is required")
                .MinimumLength(2)
                .WithMessage("Description must be at least 10 characters")
                .MaximumLength(9);


        }
    }
}
