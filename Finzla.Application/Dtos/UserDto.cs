namespace Finzla.Application.Dtos
{
    public sealed record CreateUserRequest(
        string Username,
        string Email,
        string Password,
        string FirstName,
        string LastName,
        string? PhoneNumber);

    public sealed record UserResponse(
        Guid Id,
        string Username,
        string Email,
        string FirstName,
        string LastName,
        string? PhoneNumber,
        bool IsActive,
        DateTime CreatedAt,
        DateTime? LastLoginAt);

    public sealed record AuditLogDto(
        long Id,
        string Username,
        string Action,
        string Resource,
        string HttpMethod,
        int StatusCode,
        string? IpAddress,
        long DurationMs,
        string? ErrorMessage,
        DateTime OccurredAt);
}
