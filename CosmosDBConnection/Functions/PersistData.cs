using CosmosDBConnection.CosmosDB;
using CosmosDBConnection.WebAPIContracts;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace CosmosDBConnection.Functions
{
	public static class PersistData
	{
		[FunctionName("PersistData")]
		public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
		{
			log.Info("C# HTTP trigger function processed a request.");
			string json = await req.Content.ReadAsStringAsync();
			PersistInfo persistData = JsonConvert.DeserializeObject<PersistInfo>(json);
			CosmoOperation cosmoOperation = await CosmosDBOperations.UpsertDocumentAsync(new CosmoOperation()
			{
				Collection = string.Empty,
				Database = string.Empty,
				Payload = string.Empty,
				Results = null
			});

			return await Task.Factory.StartNew(() => req.CreateResponse(HttpStatusCode.OK, "PersistData OK"));
		}
	}
}
