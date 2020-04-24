using ITLab.Salary.Database;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ITLab.Salary.Services.Events.Remote
{
    public class RemoteApiEventsService : IEventsService
    {

        public const string HttpClientName = nameof(RemoteApiEventsService) + nameof(HttpClientName);
        private readonly HttpClient client;
        private readonly ILogger logger;
        private readonly EventSalaryContext eventSalaryContext;

        public RemoteApiEventsService(
            ILogger<RemoteApiEventsService> logger,
            IHttpClientFactory httpClientFactory,
            EventSalaryContext eventSalaryContext)
        {
            client = httpClientFactory.CreateClient(HttpClientName);
            this.logger = logger;
            this.eventSalaryContext = eventSalaryContext;
        }

        public async Task<List<Guid>> GetEventIdsInRange(DateTime? begin, DateTime? end)
        {
            var uri = "ids?";
            if (begin != null)
            {
                uri += $"begin={begin}";
            }
            if (end != null)
            {
                uri += $"end={end}";
            }
            try
            {

                var response = await client.GetAsync(uri);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<Guid>>(content);
                }
                else
                {
                    throw new Exception("non 200 status code from events api");
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Can't get event ids");
                var selfReferenced = new SelfReferencedEventsService(eventSalaryContext);
                return await selfReferenced.GetEventIdsInRange(begin, end);
            }
        }
    }
}
