namespace OrderOps.Infrastructure.Exceptions;

public abstract class AppException(string message, string code) : Exception(message)
{
    public string Code { get; } = code;
}

public sealed class NotFoundException(string message)
    : AppException(message, "NOT_FOUND");

public sealed class BadRequestException(string message)
    : AppException(message, "BAD_REQUEST");

public sealed class ConflictException(string message)
    : AppException(message, "CONFLICT");
