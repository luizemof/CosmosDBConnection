using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CosmosDBConnection.Constants;
using CosmosDBConnection.CosmosDB;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace CosmosDBConnection.Functions
{
	public static class DeleteDocument
	{
		[FunctionName("DeleteDocument")]
		public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
		{
			log.Info("InsertIntentContent message received");
			try
			{
				string body = await req.Content.ReadAsStringAsync();

				if (string.IsNullOrWhiteSpace(body))
					throw new Exception("Missing body");

				Dictionary<string, string> data = JsonConvert.DeserializeObject<Dictionary<string, string>>(body);
				if (!data.TryGetValue("id", out string id) || string.IsNullOrWhiteSpace(id))
					throw new Exception("Missing Id");

				if (!data.TryGetValue("partitionKey", out string partitionKey) || string.IsNullOrWhiteSpace(partitionKey))
					throw new Exception("Missing partitionKey");

				log.Info($"Deleting document with Id: {id}\tparitionKey: {partitionKey}");
				CosmoOperation cosmoOperation = await CosmosDBOperations.DeleteDocumentAsync(new CosmoOperation()
				{
					Collection = Environment.GetEnvironmentVariable(Config.COSMOS_COLLECTION),
					Database = Environment.GetEnvironmentVariable(Config.COSMOS_DATABASE),
					Payload = new { id, partitionKey }
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
