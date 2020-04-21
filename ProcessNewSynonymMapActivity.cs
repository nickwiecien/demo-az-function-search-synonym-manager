using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
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
        public static async Task Run([BlobTrigger("synonym-map/{name}", Connection = "BlobStorageConnectionString")]Stream myBlob, string name, ILogger log, ExecutionContext context)
        {
            log.LogInformation($"Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            // Read file
            TextReader streamReader = new StreamReader(myBlob);
            var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture);
            var records = csvReader.GetRecords<Record>();
            
            // Build request
            var sb = new StringBuilder();
            foreach (var record in records)
            {
                sb.Append(record.Name);
                sb.AppendLine(record.Synonyms.Replace("|",","));
            }

            var synonymMap = new SynonymMap()
            {
                Name = "ProductNameMap",
                Synonyms = sb.ToString()
            };

            // Update Azure Search
            string searchServiceName = config["SearchServiceName"];
            string adminApiKey = config["SearchServiceAdminApiKey"];
            SearchServiceClient serviceClient = new SearchServiceClient(searchServiceName, new SearchCredentials(adminApiKey));
            await serviceClient.SynonymMaps.CreateOrUpdateAsync(synonymMap);
        }
    }
}
