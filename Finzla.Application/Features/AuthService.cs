using Finzla.Application.Contracts.Services;
using Finzla.Application.Dtos;
using Finzla.Domain.Errors;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Finzla.Application.Features
{
    public sealed class AuthService(
        IUserRepository userRepo,
        IAuditLogRepository auditRepo,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        public async Task<Result<LoginResponse>> LoginAsync(
            LoginRequest request,
            string appSecurityKey,
            CancellationToken ct = default)
        {
            
            var expectedKey = configuration["Auth:AppSecurityKey"];
            if (string.IsNullOrWhiteSpace(appSecurityKey))
                return Result<LoginResponse>.Failure(DomainError.Auth.MissingSecurityKey);
            if (appSecurityKey != expectedKey)
                return Result<LoginResponse>.Failure(DomainError.Auth.InvalidSecurityKey);

            
            var user = await userRepo.FindByUsernameAsync(request.Username.Trim().ToLowerInvariant(), ct);
            if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                logger.LogWarning("Failed login attempt for {Username}", request.Username);
                return Result<LoginResponse>.Failure(DomainError.Auth.InvalidCredentials);
            }

            if (!user.IsActive)
                return Result<LoginResponse>.Failure(DomainError.Auth.AccountInactive);

            
            user.RecordLogin();
            await userRepo.UpdateAsync(user, ct);

            var token = GenerateJwtToken(user.Username, user.Id, user.Email);
            logger.LogInformation("User {Username} authenticated", user.Username);

            var userResponse = new UserResponse(
                user.Id, user.Username, user.Email,
                user.FirstName, user.LastName, user.PhoneNumber,
                user.IsActive, user.CreatedAt, user.LastLoginAt);

            return Result<LoginResponse>.Success(new LoginResponse(token, "Bearer", 3600, userResponse));
        }

        private string GenerateJwtToken(string username, Guid userId, string email)
        {
            var jwtKey = configuration["Auth:JwtSecret"]
                ?? throw new InvalidOperationException("Auth:JwtSecret not configured.");

            var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name,             username),
                new Claim(ClaimTypes.NameIdentifier,   userId.ToString()),
                new Claim(ClaimTypes.Email,            email),
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer:            configuration["Auth:Issuer"],
                audience:          configuration["Auth:Audience"],
                claims:            claims,
                expires:           DateTime.UtcNow.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
