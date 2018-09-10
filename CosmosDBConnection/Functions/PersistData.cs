using CosmosDBConnection.CosmosDB;
using CosmosDBConnection.WebAPIContracts;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using CosmosDBConnection.Constants;
using System;

namespace CosmosDBConnection.Functions
{
	public static class PersistData
	{
		[FunctionName("PersistData")]
		public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
		{
			try
			{
				log.Info("PersistData message received");
				string json = await req.Content.ReadAsStringAsync();

				if (string.IsNullOrWhiteSpace(json))
					throw new Exception("Missing body");

				CosmoOperation cosmoOperation = await CosmosDBOperations.UpsertDocumentAsync(new CosmoOperation()
				{
					Collection = Environment.GetEnvironmentVariable(Config.COSMOS_COLLECTION),
					Database = Environment.GetEnvironmentVariable(Config.COSMOS_DATABASE),
					Payload = json,
				});

				return req.CreateResponse(HttpStatusCode.OK, cosmoOperation.Results as object);
			}
			catch (Exception ex)
			{
				log.Error("Error: ", ex);
				return req.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
			}
		}
	}
}
