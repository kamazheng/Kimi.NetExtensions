using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Results;
using System.Linq.Expressions;
using System.Reflection;


/// <summary>
/// Dynamic fluent validation used for MudBlazor 
/// In Mudblzor input tag: Validation="FluentValidationService.ValidateValue" For="@(() => Role.Name)"
/// </summary>
public static class FluentValidationService
{
    static FluentValidationService() => LicenceHelper.CheckLicense();
    public static Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        Type modelType = model.GetType();

        Type validationContextType = typeof(ValidationContext<>).MakeGenericType(new[] { modelType });
        Type validationStrategyType = typeof(ValidationStrategy<>).MakeGenericType(new[] { modelType });
        Type AbstractValidatorType = typeof(AbstractValidator<>).MakeGenericType(new[] { modelType });

        MethodInfo createWithOptionsMethod = validationContextType.GetMethod("CreateWithOptions", new[] { modelType, typeof(Action<>).MakeGenericType(validationStrategyType) })!;

        var includePropertiesMethod = validationStrategyType.GetMethod("IncludeProperties", new[] { typeof(string[]) });
        var strategyParameter = Expression.Parameter(validationStrategyType, "x");
        var includePropertiesCall = Expression.Call(strategyParameter, includePropertiesMethod!, Expression.Constant(new[] { propertyName }));
        var options = Expression.Lambda(typeof(Action<>).MakeGenericType(validationStrategyType), includePropertiesCall, strategyParameter).Compile();

        object context = createWithOptionsMethod.Invoke(null, new object[] { model, options })!;

        MethodInfo validateAsyncMethod = AbstractValidatorType.GetMethod("ValidateAsync", new[] { validationContextType, typeof(CancellationToken) })!;
        var validatorClass = GetValidatorClass(modelType);
        if (validatorClass == null)
        {
            return Array.Empty<string>();
        }
        Task<ValidationResult> task = (Task<ValidationResult>)validateAsyncMethod.Invoke(validatorClass, new object[] { context, new CancellationToken() })!;
        ValidationResult result = await task;

        if (result.IsValid)
        {
            return Array.Empty<string>();
        }
        else
        {
            return result.Errors.Select(e => e.ErrorMessage);
        }
    };

    public static object? GetValidatorClass(Type modelType)
    {
        var validatorType = typeof(AbstractValidator<>).MakeGenericType(modelType);
        var result = TypeExtensions.NotSystemAssemblies
            .SelectMany(a => a.GetTypes())
            .FirstOrDefault(t => t.IsSubclassOf(validatorType));

        return result != null ? Activator.CreateInstance(result) : null;
    }
}
