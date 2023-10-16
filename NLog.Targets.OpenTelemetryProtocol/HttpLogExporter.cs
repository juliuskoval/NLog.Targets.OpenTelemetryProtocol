using Google.Protobuf;
using OpenTelemetry.Proto.Collector.Logs.V1;
using System.Net.Http;

namespace NLog.Targets.OpenTelemetryProtocol
{
    internal class HttpLogExporter : LogExporter
    {
        private readonly HttpClient _client;

        public HttpLogExporter(string endpoint)
        {
            _client = new HttpClient();
            _client.BaseAddress = new Uri(endpoint);
        }

        protected override void SendExportRequest(ExportLogsServiceRequest request)
        {
            var httpRequest = CreateHttpRequestMessage(request);

            var response = _client.Send(httpRequest);

            response.EnsureSuccessStatusCode();
        }

        private HttpRequestMessage CreateHttpRequestMessage(ExportLogsServiceRequest request)
        {
            var dataSize = request.CalculateSize();
            var buffer = new byte[dataSize];

            request.WriteTo(buffer.AsSpan());

            var content = new ByteArrayContent(buffer, 0, dataSize);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-protobuf");

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "");
            httpRequest.Content = content;
            return httpRequest;
        }

        public override void Dispose()
        {
            _client.Dispose();
        }
    }
}
