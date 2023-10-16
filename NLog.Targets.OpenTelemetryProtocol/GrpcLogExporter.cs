using Grpc.Core;
using Grpc.Net.Client;
using OpenTelemetry.Proto.Collector.Logs.V1;

namespace NLog.Targets.OpenTelemetryProtocol
{
    internal class GrpcLogExporter : LogExporter
    {
        private readonly LogsService.LogsServiceClient _client;
        private readonly GrpcChannel _channel;
        private Metadata _headers = new Metadata();

        public GrpcLogExporter(string endpoint)
        {
            var grpcChannelOptions = new GrpcChannelOptions();
  

            _channel = GrpcChannel.ForAddress(endpoint, grpcChannelOptions);
            _client = new LogsService.LogsServiceClient(_channel);
        }

        protected override void SendExportRequest(ExportLogsServiceRequest request)
        {
            _headers.Add("user-agent", "OTel-OTLP-Exporter-Dotnet/1.6.1-alpha.0.54+865bcb64a5c67d713ded373a2839c49dba07dfc2");
            // _headers.Add("api-key", "eu01xx4ab133a94e4c5ab3b745eb05ee1802NRAL");
            _client.Export(request, _headers);
        }

        public override void Dispose()
        {
            _channel.Dispose();
        }
    }
}
