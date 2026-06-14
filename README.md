Finzla Webhook Transaction Processor
Overview
This project implements the Senior Backend Engineer (.NET) pre‑screening challenge for Finzla Technologies Nigeria Limited.
The core focus is a webhook transaction processor that ingests external transaction events, enforces idempotency, and produces derived account summaries.

Core Features
Webhook Endpoint: POST /api/webhooks/transactions

Idempotency: Enforced via TraceId check and PostgreSQL unique index

Persistence: Transactions and summaries stored in PostgreSQL

Derived Computation: Real‑time rolling balance, credits, debits, transaction count

Structured Output: Exposed via AccountSummary endpoint

Automated Tests: xUnit + NSubstitute + FluentAssertions

Additional Supporting Features
Basic JWT authentication and audit logging (production‑readiness demonstration)

AWS deployment plan (ECS Fargate, RDS PostgreSQL, Secrets Manager, CloudWatch)

CI/CD pipeline outline (GitHub Actions: build, test, docker push, deploy)

How to Run
Clone the repository

Configure PostgreSQL connection string in appsettings.json

Run migrations (dotnet ef database update)

Start the API (dotnet run)

Test the webhook endpoint with a valid IngestTransactionRequest payload
