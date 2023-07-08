using Polly;
using Polly.Retry;
using System;
using System.Net;
using System.Net.Http;

namespace Sechat.Service.Services.HttpClients.PollyPolicies;

public class BasicHttpClientPolicy
{
    public AsyncRetryPolicy<HttpResponseMessage> ImmediateHttpRetry { get; }
    public AsyncRetryPolicy<HttpResponseMessage> LinearHttpRetry { get; }
    public AsyncRetryPolicy<HttpResponseMessage> ExponentialHttpRetry { get; }

    public BasicHttpClientPolicy()
    {
        ImmediateHttpRetry = Policy.HandleResult<HttpResponseMessage>(
                res => !res.IsSuccessStatusCode && res.StatusCode != HttpStatusCode.BadRequest)
            .RetryAsync(5);

        LinearHttpRetry = Policy.HandleResult<HttpResponseMessage>(
                res => !res.IsSuccessStatusCode && res.StatusCode != HttpStatusCode.BadRequest)
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(3));

        ExponentialHttpRetry = Policy.HandleResult<HttpResponseMessage>(
                res => !res.IsSuccessStatusCode && res.StatusCode != HttpStatusCode.BadRequest)
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }
}
