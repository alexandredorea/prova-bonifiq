namespace ProvaPub.Application.Commons.Models;

public sealed class Result
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public object? Data { get; init; }
    public List<Error> Error { get; init; } = new();

    public static Result Ok(string message = "Processamento realizado com sucesso", object? data = null) =>
        new() { Success = true, Message = message, Data = data };

    public static Result Fail(string message, params Error[] errors) =>
        new() { Success = false, Message = message, Error = errors.ToList() };

    public static Result Forbidden(string message = "Proibido") =>
        Fail(message, new Error("FORBIDDEN", message));

    public static Result NotFound(string message = "Não encontrado") =>
        Fail(message, new Error("NOT_FOUND", message));

    public static Result Conflict(string message) =>
        Fail(message, new Error("CONFLICT", message));

    public static Result Validation(List<Error> errors, string message = "Erro de validação") =>
        new() { Success = false, Message = message, Error = errors };
}

public sealed class Result<T>
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public T? Data { get; init; }
    public List<Error> Error { get; init; } = new();

    public static Result<T> Ok(T data, string message = "Processamento realizado com sucesso") =>
        new() { Success = true, Message = message, Data = data };

    public static Result<T> Fail(string message, params Error[] errors) =>
        new() { Success = false, Message = message, Error = errors.ToList() };

    public static Result<T> InternalError(string message) =>
        Fail("Ocorreu um erro interno", new Error("INTERNAL_SERVER_ERROR", message));

    public static Result<T> Forbidden(string message = "Proibido") =>
        Fail(message, new Error("FORBIDDEN", message));

    public static Result<T> NotFound(string message) =>
        Fail("Não encontrado", new Error("NOT_FOUND", message));

    public static Result<T> Conflict(string message) =>
        Fail(message, new Error("CONFLICT", message));

    public static Result<T> Validation(List<Error> errors, string message = "Erro de validação") =>
        new() { Success = false, Message = message, Error = errors };
}