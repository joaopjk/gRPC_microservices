using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using ProductGrpc.Protos;

namespace ProductWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;
        private readonly ProductFactory _factory;
        public Worker(ILogger<Worker> logger, IConfiguration configuration, ProductFactory factory)
        {
            _logger = logger;
            _configuration = configuration;
            _factory = factory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Thread.Sleep(5000);

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                var httpHandler = new HttpClientHandler();
                httpHandler.ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

                using var channel = GrpcChannel.ForAddress(_configuration["WorkerService:ServerUrl"], new GrpcChannelOptions { HttpHandler = httpHandler });
                var client = new ProductProtoService.ProductProtoServiceClient(channel);

                var addProductAsync = await client.AddProductAsync(_factory.Generate().Result);
                Console.WriteLine("AddProductAsync" + addProductAsync.ToString());

                await Task.Delay(Convert.ToInt32(_configuration["WorkerService:TaskInterval"]), stoppingToken);
            }
        }
    }
}
