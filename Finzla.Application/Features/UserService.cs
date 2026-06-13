using Finzla.Application.Contracts.Services;
using Finzla.Application.Dtos;
using Finzla.Domain.Entities;
using Finzla.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace Finzla.Application.Features
{
    public sealed class UserService(
        IUserRepository userRepo,
        ILogger<UserService> logger)
    {
        public async Task<Result<UserResponse>> CreateAsync(
            CreateUserRequest request,
            CancellationToken ct = default)
        {
            if (await userRepo.ExistsAsync(request.Username, request.Email, ct))
                return Result<UserResponse>.Failure(DomainError.User.AlreadyExists);

            var hash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            var user = AppUser.Create(
                request.Username, request.Email, hash,
                request.FirstName, request.LastName, request.PhoneNumber);

            await userRepo.AddAsync(user, ct);
            logger.LogInformation("User {Username} created", user.Username);
            return Result<UserResponse>.Success(MapToResponse(user));
        }

        public async Task<Result<UserResponse>> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var user = await userRepo.GetByIdAsync(id, ct);
            if (user is null)
                return Result<UserResponse>.Failure(DomainError.User.NotFound);
            return Result<UserResponse>.Success(MapToResponse(user));
        }

        public async Task<Result<IReadOnlyList<UserResponse>>> GetAllAsync(CancellationToken ct = default)
        {
            var users = await userRepo.GetAllAsync(ct);
            var dtos = users.Select(MapToResponse).ToList().AsReadOnly();
            return Result<IReadOnlyList<UserResponse>>.Success(dtos);
        }

        public async Task<Result<UserResponse>> ToggleActiveAsync(Guid id, bool activate, CancellationToken ct = default)
        {
            var user = await userRepo.GetByIdAsync(id, ct);
            if (user is null)
                return Result<UserResponse>.Failure(DomainError.User.NotFound);
            if (activate) user.Activate(); else user.Deactivate();
            await userRepo.UpdateAsync(user, ct);
            logger.LogInformation("User {Username} {Status}", user.Username, activate ? "activated" : "deactivated");
            return Result<UserResponse>.Success(MapToResponse(user));
        }

        private static UserResponse MapToResponse(AppUser u) =>
            new(u.Id, u.Username, u.Email, u.FirstName, u.LastName,
                u.PhoneNumber, u.IsActive, u.CreatedAt, u.LastLoginAt);
    }
}
