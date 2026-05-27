using TAKSuite.Data.Services;

namespace TAKSuite.TAK
{
    public class TakHttpLoggingHandler : DelegatingHandler
    {
        private readonly TakTrafficLogger _logger;

        public TakHttpLoggingHandler(TakTrafficLogger logger)
        {
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var path = request.RequestUri?.PathAndQuery ?? "?";
            _logger.Write($"REST-{request.Method}", path);

            var response = await base.SendAsync(request, cancellationToken);

            _logger.Write($"REST-{request.Method}", $"  → {(int)response.StatusCode} {response.ReasonPhrase}");
            return response;
        }
    }
}
