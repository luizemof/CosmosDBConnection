using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CosmosDBConnection.Constants;
using CosmosDBConnection.CosmosDB;
using CosmosDBConnection.Tools;
using CosmosDBConnection.Tools.Jwt;
using CosmosDBConnection.WebAPIContracts;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace CosmosDBConnection.Functions
{
	public static class InsertIntentContent
	{
		[FunctionName("InsertIntentContent")]
		public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
		{
			log.Info("InsertIntentContent message received");

			try
			{
				//Utils.ValidateAuthorizationHeader(req.Headers?.Authorization);

				string json = await req.Content.ReadAsStringAsync();
				if (string.IsNullOrWhiteSpace(json))
					throw new Exception("Missing body");

				IntentContent intentContent = JsonConvert.DeserializeObject<IntentContent>(json);

				log.Info("Creating intentContent Id");
				if (string.IsNullOrWhiteSpace(intentContent.id))
					intentContent.id = Guid.NewGuid().ToString();

				log.Info("Creating intentContent Partition key");
				if (string.IsNullOrWhiteSpace(intentContent.partitionKey))
					intentContent.partitionKey = Utils.CreatePartitionKey(intentContent.type.Replace(" ", ""), intentContent.id);

				CosmoOperation cosmoOperation = await CosmosDBOperations.UpsertDocumentAsync(new CosmoOperation()
				{
					Collection = Environment.GetEnvironmentVariable(Config.COSMOS_COLLECTION),
					Database = Environment.GetEnvironmentVariable(Config.COSMOS_DATABASE),
					Payload = intentContent
				});

				return req.CreateResponse(HttpStatusCode.OK, cosmoOperation.Results as object);
			}
			catch(UnauthorizedAccessException ex)
			{
				log.Error("Unauthorized");
				return req.CreateResponse(HttpStatusCode.Unauthorized, ex.Message);
			}
			catch (Exception ex)
			{
				log.Error($"Error: {ex.Message} ", ex);
				return req.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
			}
		}
	}
}
