using Polly;

namespace CRM.Infrastructure.CrossCutting;
public static class PollyPolicies
{
    public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(response => (int)response.StatusCode >= 500)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    var error = outcome.Exception?.Message
                                   ?? $"HTTP {(int)outcome.Result.StatusCode} - {outcome.Result.ReasonPhrase}";
                    Console.WriteLine($"[Retry {retryAttempt}] Retrying in {timespan.TotalSeconds}s due to: {error}");
                });
    }
}