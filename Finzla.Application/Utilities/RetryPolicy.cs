using Polly;
using System;
using System.Collections.Generic;
using System.Text;

namespace Finzla.Application.Utilities
{
    public static class RetryPolicy
    {
        public static async Task ExecuteAsync(Func<Task> action, int retryCount = 3, TimeSpan? delay = null)
        {
            var policy = Policy
                .Handle<Exception>(ex => !(ex is ObjectDisposedException))
                .WaitAndRetryAsync(
                    retryCount,
                    attempt => delay ?? TimeSpan.FromSeconds(Math.Pow(2, attempt)), 
                    onRetry: (exception, timespan, attempt, context) =>
                    {
                        Console.WriteLine($"Retry {attempt}: {exception.GetType().Name} - {exception.Message}");
                    });

            await policy.ExecuteAsync(action);
        }
    }

}
