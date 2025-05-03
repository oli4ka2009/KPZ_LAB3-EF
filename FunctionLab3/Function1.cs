using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace FunctionsLab3
{
    public class RegistrationMessage
    {
        public string eventKey { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string clientId { get; set; }
        public string id { get; set; }
    }

    public class ProcessStorageQueueRegistration
    {
        private readonly ILogger<ProcessStorageQueueRegistration> _logger;

        public ProcessStorageQueueRegistration(ILogger<ProcessStorageQueueRegistration> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Function("ProcessStorageQueueRegistration")]
        public async Task Run(
            [QueueTrigger("registration-requests", Connection = "AzureWebJobsStorage")] string myQueueItemJson,
            FunctionContext context)
        {
            try
            {
                _logger.LogInformation($"C# Storage Queue trigger function started processing message (JSON): {myQueueItemJson}");

                var messagePayload = JsonSerializer.Deserialize<RegistrationMessage>(myQueueItemJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                await Task.Delay(10000);

                if (messagePayload != null)
                {
                    _logger.LogInformation($"Successfully deserialized. Processing eventKey: {messagePayload.eventKey}, Email: {messagePayload.email}");

                    _logger.LogInformation($"Simulating processing for event {messagePayload.eventKey}, client {messagePayload.clientId}...");
                    _logger.LogInformation($"'Sign-in sheet' generation simulated for {messagePayload.firstName} {messagePayload.lastName}.");

                    _logger.LogInformation($"Processing completed for eventKey: {messagePayload.eventKey}");
                }
                else
                {
                    _logger.LogWarning("Could not deserialize queue message content.");
                }
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, $"JSON Deserialization error for message: {myQueueItemJson}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error processing message: {myQueueItemJson}");
                throw;
            }
        }
    }

    public class ProcessServiceBusQueueRegistration // Можна перейменувати клас для ясності
    {
        private readonly ILogger<ProcessServiceBusQueueRegistration> _logger;

        public ProcessServiceBusQueueRegistration(ILogger<ProcessServiceBusQueueRegistration> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Function("ProcessServiceBusQueueRegistration")]
        public void Run(
            [ServiceBusTrigger("servicebus-registration-requests", Connection = "ServiceBusConnectionString")] string mySbMsg,
            FunctionContext context)
        {
            try
            {
                _logger.LogInformation($"C# Service Bus queue trigger function started processing message: {mySbMsg}");

                var messagePayload = JsonSerializer.Deserialize<RegistrationMessage>(mySbMsg);

                if (messagePayload != null)
                {
                    _logger.LogInformation($"Successfully deserialized. Processing eventKey: {messagePayload.eventKey}, Email: {messagePayload.email}");

                    _logger.LogInformation($"Simulating processing for registration {messagePayload.eventKey}, client {messagePayload.clientId}...");
                    _logger.LogInformation($"'Sign-in sheet' generation simulated for {messagePayload.firstName} {messagePayload.lastName}.");

                    _logger.LogInformation($"Processing completed for Registration ID: {messagePayload.eventKey}");
                }
                else
                {
                    _logger.LogWarning("Could not deserialize Service Bus message content.");
                }
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, $"JSON Deserialization error for Service Bus message: {mySbMsg}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error processing Service Bus message: {mySbMsg}");
                throw;
            }
        }
    }
}