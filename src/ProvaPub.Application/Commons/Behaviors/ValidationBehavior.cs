using FluentValidation;
using MediatR;

namespace ProvaPub.Application.Commons.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse> where TRequest : class
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next(cancellationToken);

        var context = new ValidationContext<TRequest>(request);

        // Executa todas as validações em paralelo
        var validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken))
        );

        // Coleta todos os erros
        var failures = validationResults
            .Where(r => !r.IsValid)
            .SelectMany(r => r.Errors)
            .ToList();

        if (failures.Count != 0)
            throw new ValidationException(failures);

        return await next(cancellationToken);
    }

    //public async Task<TResponse> Handle(
    //    TRequest request,
    //    RequestHandlerDelegate<TResponse> next,
    //    CancellationToken cancellationToken)
    //{
    //    if (!validators.Any())
    //        return await next(cancellationToken);

    //    var context = new ValidationContext<TRequest>(request);

    //    // Executa todas as validações em paralelo
    //    var validationResults = await Task.WhenAll(
    //        validators.Select(v => v.ValidateAsync(context, cancellationToken)));

    //    // Coleta todos os erros
    //    var failures = validationResults
    //        .SelectMany(r => r.Errors)
    //        .Where(f => f is not null)
    //        .ToList();

    //    if (failures.Count == 0)
    //        return await next(cancellationToken);

    //    // Converte para Error objects
    //    var errors = failures
    //        .Select(f => new Error($"{f.PropertyName}", $"{f.ErrorMessage}"))
    //        .ToList();

    //    // Retorna Result com erros de validação
    //    return CreateValidationResult<TResponse>(errors);
    //}

    //private static TResponse CreateValidationResult<T>(List<Error> errors)
    //{
    //    // Para Result (não genérico)
    //    if (typeof(T) == typeof(Result))
    //        return (TResponse)(object)Result.Validation(errors);

    //    // Para Result<T>
    //    if (typeof(T).IsGenericType &&
    //        typeof(T).GetGenericTypeDefinition() == typeof(Result<>))
    //    {
    //        var resultType = typeof(T).GenericTypeArguments[0];

    //        var validationMethod = typeof(Result<>)
    //            .MakeGenericType(resultType)
    //            .GetMethod(nameof(Result<object>.Validation),
    //                new[] { typeof(List<Error>), typeof(string) });

    //        var result = validationMethod!.Invoke(null,
    //            new object[] { errors, "Um ou mais erros de validação ocorreram." });

    //        return (TResponse)result!;
    //    }

    //    // Fallback: lança exceção se não for Result
    //    throw new ValidationException(errors.Select(e =>
    //        new ValidationFailure(e.Code, e.Message)).ToList());
    //}
}