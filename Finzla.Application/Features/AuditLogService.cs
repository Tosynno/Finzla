using Finzla.Application.Contracts.Services;
using Finzla.Application.Dtos;
using Finzla.Domain.Entities;
using Finzla.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace Finzla.Application.Features
{
    public sealed class AuditLogService(
        IAuditLogRepository auditRepo,
        ILogger<AuditLogService> logger)
    {
        public async Task RecordAsync(
            string username,
            string action,
            string resource,
            string httpMethod,
            int statusCode,
            string? ipAddress,
            string? userAgent,
            string? requestBody,
            long durationMs,
            string? errorMessage = null,
            CancellationToken ct = default)
        {
            try
            {
                var entry = AuditLog.Create(
                    username, action, resource, httpMethod,
                    statusCode, ipAddress, userAgent,
                    requestBody, durationMs, errorMessage);

                await auditRepo.LogAsync(entry, ct);
            }
            catch (Exception ex)
            {
                
                logger.LogError(ex, "Failed to write audit log for {Action} {Resource}", action, resource);
            }
        }

        public async Task<Result<IReadOnlyList<AuditLogDto>>> GetByUsernameAsync(
            string username,
            int page = 1,
            int pageSize = 50,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(username))
                return Result<IReadOnlyList<AuditLogDto>>.Failure(DomainError.User.NotFound);

            var logs = await auditRepo.GetByUsernameAsync(username.Trim().ToLowerInvariant(), page, pageSize, ct);
            var dtos = logs.Select(l => new AuditLogDto(
                l.Id, l.Username, l.Action, l.Resource, l.HttpMethod,
                l.StatusCode, l.IpAddress, l.DurationMs, l.ErrorMessage, l.OccurredAt))
                .ToList().AsReadOnly();

            return Result<IReadOnlyList<AuditLogDto>>.Success(dtos);
        }

        public async Task<Result<IReadOnlyList<AuditLogDto>>> GetAllAsync(
            int page = 1,
            int pageSize = 50,
            CancellationToken ct = default)
        {
            var all = await auditRepo.GetAsync(
                   predicate: null,
                   orderBy: q => q.OrderByDescending(l => l.OccurredAt),
                   includeString: null,
                   disableTracking: true,
                   cancellationToken: ct);

            var paged = all.Skip((page - 1) * pageSize).Take(pageSize);
            var dtos  = paged.Select(l => new AuditLogDto(
                l.Id, l.Username, l.Action, l.Resource, l.HttpMethod,
                l.StatusCode, l.IpAddress, l.DurationMs, l.ErrorMessage, l.OccurredAt))
                .ToList().AsReadOnly();

            return Result<IReadOnlyList<AuditLogDto>>.Success(dtos);
        }
    }
}
