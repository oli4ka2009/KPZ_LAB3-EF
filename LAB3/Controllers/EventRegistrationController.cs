using LAB3.Helpers;
using LAB3.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Azure.Storage.Queues;
using System.Text.Json;
using Azure.Messaging.ServiceBus;

namespace LAB3.Controllers
{
    public class EventRegistrationController : Controller
    {
        private readonly RegistrationContext _registrationContext;
        private readonly ApplicationDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EventRegistrationController> _logger;

        public EventRegistrationController(IOptions<CosmosSettings> cosmosSettings, ApplicationDbContext dbContext, IConfiguration configuration, ILogger<EventRegistrationController> logger)
        {
            _registrationContext = new RegistrationContext(cosmosSettings);
            _dbContext = dbContext;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Register(int clientId, string eventKey)
        {
            var client = await _dbContext.Clients.SingleOrDefaultAsync(c => c.Id == clientId);
            if (client == null)
            {
                return NotFound();
            }

            await _registrationContext.ConfigureConnectionAsync();

            var registration = new EventRegistration
            {
                EventKey = eventKey,
                FirstName = client.FirstName,
                LastName = client.LastName,
                Email = client.Email,

                ClientId = clientId.ToString()
            };

            return View(registration);
        }

        [HttpPost]
        public async Task<IActionResult> Register(EventRegistration registration)
        {
            if (ModelState.IsValid)
            {
                await _registrationContext.ConfigureConnectionAsync();
                await _registrationContext.SaveEventRegistrationAsync(registration);

                var documentUrl = await CallGenerateDocumentFunctionAsync(registration);

                TempData["DocumentUrl"] = documentUrl;

                //try
                //{
                //    string connectionString = _configuration.GetConnectionString("AzureStorage");
                //    if (string.IsNullOrEmpty(connectionString))
                //    {
                //        _logger.LogError("Azure Storage Connection String 'AzureWebJobsStorage' is not configured.");
                //    }
                //    else
                //    {
                //        string queueName = "registration-requests";

                //        QueueClient queueClient = new QueueClient(connectionString, queueName);
                //        await queueClient.CreateIfNotExistsAsync();

                //        var messagePayload = registration;

                //        registration.id = "";

                //        string messageJson = System.Text.Json.JsonSerializer.Serialize(messagePayload);
                //        string messageBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(messageJson));

                //        await queueClient.SendMessageAsync(messageBase64);

                //        _logger.LogInformation($"Sent message to Storage Queue");
                //    }
                //}
                //catch (Exception ex)
                //{
                //    _logger.LogError(ex, "Error sending message to Storage Queue");
                //}

                try
                {
                    string connectionString = _configuration.GetConnectionString("ServiceBusConnectionString");
                    string queueName = "servicebus-registration-requests"; 

                    if (string.IsNullOrEmpty(connectionString))
                    {
                        _logger.LogError("Azure Service Bus Connection String 'ServiceBusConnectionString' is not configured.");
                    }
                    else
                    {
                        await using var client = new ServiceBusClient(connectionString);
                        ServiceBusSender sender = client.CreateSender(queueName);

                        var messagePayloadObject = registration;

                        registration.id = "";

                        string messageJson = System.Text.Json.JsonSerializer.Serialize(messagePayloadObject);

                        ServiceBusMessage message = new ServiceBusMessage(messageJson);

                        await sender.SendMessageAsync(message);

                        _logger.LogInformation($"Sent message to Service Bus Queue for Registration");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending message to Service Bus Queue");
                }

                return RedirectToAction("Confirmation");
            }
            return View(registration);
        }

        public IActionResult Confirmation()
        {
            ViewBag.DocumentUrl = TempData["DocumentUrl"] as string;
            return View();
        }

        private async Task<string> CallGenerateDocumentFunctionAsync(EventRegistration registration)
        {
            var functionUrl = "http://localhost:7072/api/GenerateDocument";

            var payload = new
            {
                eventKey = registration.EventKey,
                firstName = registration.FirstName,
                lastName = registration.LastName,
                email = registration.Email,
                clientId = registration.ClientId
            };

            var jsonPayload = JsonConvert.SerializeObject(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.PostAsync(functionUrl, content);
                response.EnsureSuccessStatusCode();

                var documentUrl = await response.Content.ReadAsStringAsync();
                return documentUrl;
            }
        }
    }
}
