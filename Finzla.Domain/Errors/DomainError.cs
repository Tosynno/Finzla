namespace Finzla.Domain.Errors
{
    public sealed record DomainError(string Code, string Message)
    {
        public static class Transaction
        {
            public static readonly DomainError MissingTraceId = new("TXN_001", "TraceId is required.");
            public static readonly DomainError MissingExternalId = new("TXN_001", "ExternalId is required.");
            public static readonly DomainError InvalidAmount      = new("TXN_002", "Amount must be greater than zero.");
            public static readonly DomainError InvalidType        = new("TXN_003", "Type must be 'Credit' or 'Debit'.");
        }

        public static class Account
        {
            public static readonly DomainError InvalidAccountId = new("ACC_001", "AccountId is required.");
            public static readonly DomainError NotFound         = new("ACC_002", "No summary found for the specified account.");
        }

        public static class Auth
        {
            public static readonly DomainError InvalidCredentials = new("AUTH_001", "Invalid username or password.");
            public static readonly DomainError MissingSecurityKey = new("AUTH_002", "AppSecurityKey header is required.");
            public static readonly DomainError InvalidSecurityKey = new("AUTH_003", "AppSecurityKey header is invalid.");
            public static readonly DomainError AccountInactive    = new("AUTH_004", "User account is inactive.");
        }

        public static class User
        {
            public static readonly DomainError NotFound        = new("USR_001", "User not found.");
            public static readonly DomainError AlreadyExists   = new("USR_002", "A user with that username or email already exists.");
            public static readonly DomainError InvalidId       = new("USR_003", "Invalid user ID.");
        }
    }

    public sealed class Result<T>
    {
        public bool IsFailure { get; }
        public T? Value { get; }
        public DomainError? Error { get; }

        private Result(T value)           { Value = value; IsFailure = false; }
        private Result(DomainError error) { Error = error; IsFailure = true;  }

        public static Result<T> Success(T value)         => new(value);
        public static Result<T> Failure(DomainError error) => new(error);
    }
}
