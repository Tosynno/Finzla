using Finzla.Application.Contracts.Services;
using Finzla.Application.Dtos;
using Finzla.Application.Features;
using Finzla.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Xunit;

namespace Finzla.Tests
{
    public sealed class IngestTransactionServiceTests
    {
        private readonly ITransactionRepository _txRepo;
        private readonly IAccountSummaryRepository _summaryRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IngestTransactionService _sut;
        private static string? _acceptedTraceId;
        public IngestTransactionServiceTests()
        {
            _txRepo = Substitute.For<ITransactionRepository>();
            _summaryRepo = Substitute.For<IAccountSummaryRepository>();
            _unitOfWork = Substitute.For<IUnitOfWork>();

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "Auth:GLAccount", "2098509840" }
                })
                .Build();

            _sut = new IngestTransactionService(
                _txRepo,
                _summaryRepo,
                _unitOfWork,
                config,
                NullLogger<IngestTransactionService>.Instance);
        }

        [Fact]
        public async Task RepositoryFailure_ThrowsException()
        {
            var request = ValidRequest("1234567890", 5000m, "Credit");

            _txRepo.ExistsAsync(request.TraceId, Arg.Any<CancellationToken>())
                .Returns(false);
            _summaryRepo.FindAsync(request.AccountId, Arg.Any<CancellationToken>())
                .ReturnsNull();

            _txRepo.When(x => x.AddAsync(Arg.Any<Transaction>(), Arg.Any<CancellationToken>()))
                .Do(_ => throw new Exception("Database error"));

            await Assert.ThrowsAsync<Exception>(() => _sut.ExecuteAsync(request));
        }


        [Fact]
        public async Task NewTransaction_ReturnsAccepted_AndPersistsBothEntities()
        {
            var request = ValidRequest("1234567890", 5000m, "Credit");

            _txRepo.ExistsAsync(request.TraceId, Arg.Any<CancellationToken>()).Returns(false);
            _summaryRepo.FindAsync(request.AccountId, Arg.Any<CancellationToken>()).ReturnsNull();

            var result = await _sut.ExecuteAsync(request);

            result.IsFailure.Should().BeFalse();
            result.Value!.Status.Should().Be("Accepted");
            result.Value.AccountSummary.Balance.Should().Be(5000m);

            _acceptedTraceId = request.TraceId;

            await _txRepo.Received(1).AddAsync(Arg.Any<Transaction>(), Arg.Any<CancellationToken>());
            await _summaryRepo.Received(1).AddAsync(Arg.Any<AccountSummary>(), Arg.Any<CancellationToken>());
            await _unitOfWork.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
        }


        [Fact]
        public async Task DebitTransaction_DecreasesBalance_Correctly()
        {
            var request = ValidRequest("9876543210", 1500m, "Debit");
            var existing = AccountSummary.Init("acc-001");

            _txRepo.ExistsAsync(request.TraceId, Arg.Any<CancellationToken>()).Returns(false);
            _summaryRepo.FindAsync(request.AccountId, Arg.Any<CancellationToken>()).Returns(existing);

            var result = await _sut.ExecuteAsync(request);

            result.IsFailure.Should().BeFalse();
            result.Value!.AccountSummary.Balance.Should().Be(-1500m);
            result.Value.AccountSummary.TotalDebits.Should().Be(1500m);
        }

        [Fact]
        public async Task DuplicateTransaction_ReturnsDuplicate_WithoutPersisting()
        {
            var traceId = _acceptedTraceId ?? Guid.NewGuid().ToString();
            var request = new IngestTransactionRequest(
                traceId, "1234567890", "acc-001", "NGN", 2500m, "Debit", DateTime.UtcNow.AddMinutes(-1));

            var existing = AccountSummary.Init("acc-001");

            _txRepo.ExistsAsync(request.TraceId, Arg.Any<CancellationToken>()).Returns(true);
            _summaryRepo.FindAsync(request.AccountId, Arg.Any<CancellationToken>()).Returns(existing);

            var result = await _sut.ExecuteAsync(request);

            result.IsFailure.Should().BeFalse();
            result.Value!.Status.Should().Be("Duplicate");

            await _txRepo.DidNotReceive().AddAsync(Arg.Any<Transaction>(), Arg.Any<CancellationToken>());
            await _summaryRepo.DidNotReceive().AddAsync(Arg.Any<AccountSummary>(), Arg.Any<CancellationToken>());
            await _unitOfWork.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task InvalidAmount_ReturnsFailure_TXN002()
        {
            var request = ValidRequest("1234567890", -100m, "Credit");
            _txRepo.ExistsAsync(request.TraceId, Arg.Any<CancellationToken>()).Returns(false);

            var result = await _sut.ExecuteAsync(request);

            result.IsFailure.Should().BeTrue();
            result.Error!.Code.Should().Be("TXN_002");
        }

        [Fact]
        public async Task InvalidType_ReturnsFailure_TXN003()
        {
            var request = ValidRequest("1234567890", 500m, "Transfer");
            _txRepo.ExistsAsync(request.TraceId, Arg.Any<CancellationToken>()).Returns(false);

            var result = await _sut.ExecuteAsync(request);

            result.IsFailure.Should().BeTrue();
            result.Error!.Code.Should().Be("TXN_003");
        }

        private static IngestTransactionRequest ValidRequest(string externalId, decimal amount, string type) =>
            new(Guid.NewGuid().ToString(), externalId, "acc-001", "NGN", amount, type, DateTime.UtcNow.AddMinutes(-1));
    }
}
