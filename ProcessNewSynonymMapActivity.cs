using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace demo_az_function_search_synonym_manager
{
    public static class ProcessNewSynonymMapActivity
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        [FunctionName("ProcessNewSynonymMapActivity")]
        public static async Task Run([BlobTrigger("samples-workitems/{name}", Connection = "BlobStorageConnectionString")]Stream myBlob, string name, ILogger log, ExecutionContext context)
        {
            log.LogInformation($"Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            // TODO - Read file

            // TODO - Build request

            // Update Azure Search
            var request = "";
            var url = config["AzureSearchServiceUrl"];
            var response = await _httpClient.PostAsync(url, new StringContent(request, Encoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException("An error occured invoking measurement service.");
            }
        }
    }
}
