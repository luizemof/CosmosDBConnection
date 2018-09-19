using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CosmosDBConnection.Constants;
using CosmosDBConnection.CosmosDB;
using CosmosDBConnection.Tools;
using CosmosDBConnection.WebAPIContracts;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace CosmosDBConnection.Functions
{
	public static class InsertProfiles
	{
		[FunctionName("InsertProfiles")]
		public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
		{
			log.Info("InsertIntentContent message received");
			try
			{
				//Utils.ValidateAuthorizationHeader(req.Headers?.Authorization);

				string json = await req.Content.ReadAsStringAsync();
				if (string.IsNullOrWhiteSpace(json))
					throw new Exception("Missing body");

				Profiles profile = JsonConvert.DeserializeObject<Profiles>(json);
				if (string.IsNullOrWhiteSpace(profile.id))
				{
					log.Info("Creating profiles Id");
					profile.id = Guid.NewGuid().ToString();
				}

				if (string.IsNullOrWhiteSpace(profile.partitionKey))
				{
					log.Info("Creating profiles Partition key");
					profile.partitionKey = Utils.CreatePartitionKey(profile.type.Replace(" ", ""), profile.id);
				}

				CosmoOperation cosmoOperation = await CosmosDBOperations.UpsertDocumentAsync(new CosmoOperation()
				{
					Collection = Environment.GetEnvironmentVariable(Config.COSMOS_COLLECTION),
					Database = Environment.GetEnvironmentVariable(Config.COSMOS_DATABASE),
					Payload = profile
				});

				return req.CreateResponse(HttpStatusCode.OK, cosmoOperation.Results as object);
			}
			catch (UnauthorizedAccessException ex)
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
