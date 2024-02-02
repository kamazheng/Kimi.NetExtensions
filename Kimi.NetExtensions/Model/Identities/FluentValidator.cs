using FluentValidation;

namespace Kimi.NetExtensions.Model.Identities;

public class SettingValidator : AbstractValidator<Setting>
{
    public SettingValidator()
    {
        RuleFor(x => x.Key)
            .NotEmpty()
            .Length(1, 100);
        RuleFor(x => x.BoolValue).NotEmpty().When(x => x.DateValue == default && x.NumericValue == default && x.StringValue == default).WithMessage("Must have one of values not empty");
        RuleFor(x => x.DateValue).NotEmpty().When(x => x.BoolValue == default && x.NumericValue == default && x.StringValue == default).WithMessage("Must have one of values not empty");
        RuleFor(x => x.NumericValue).NotEmpty().When(x => x.DateValue == default && x.BoolValue == default && x.StringValue == default).WithMessage("Must have one of values not empty");
        RuleFor(x => x.StringValue).NotEmpty().When(x => x.DateValue == default && x.NumericValue == default && x.BoolValue == default).WithMessage("Must have one of values not empty");
    }
}

public class RoleValidator : AbstractValidator<Role>
{
    public RoleValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .Length(1, 100);
    }
}

public class RolePermissionValidator : AbstractValidator<RolePermission>
{
    public RolePermissionValidator()
    {
        RuleFor(x => x.RoleFk)
            .NotEmpty();
        RuleFor(x => x.PermissionName)
            .NotEmpty();
    }
}

public class UserRoleValidator : AbstractValidator<UserRole>
{
    public UserRoleValidator()
    {
        RuleFor(x => x.Domain)
            .NotEmpty();
        RuleFor(x => x.UserId)
            .NotEmpty();
    }
}