namespace Finzla.Application.Dtos
{
    public sealed record LoginRequest(string Username, string Password);
    public sealed record LoginResponse(string Token, string TokenType, int ExpiresIn, UserResponse User);
}
