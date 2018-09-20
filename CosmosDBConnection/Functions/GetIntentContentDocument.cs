using System;
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
	public static class GetIntentContentDocument
	{
		[FunctionName("GetIntentContentDocument")]
		public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
		{
			log.Info("GetIntentAnswer message received");
			try
			{
				string intentName = req.GetQueryNameValuePairs()
					.FirstOrDefault(q => string.Compare(q.Key, "intent", true) == 0)
					.Value;

				string profile = req.GetQueryNameValuePairs()
					.FirstOrDefault(q => string.Compare(q.Key, "profile", true) == 0)
					.Value;

				string entity = req.GetQueryNameValuePairs()
					.FirstOrDefault(q => string.Compare(q.Key, "entity", true) == 0)
					.Value;

				string whereCondition = "WHERE d.type = 'IntentContent'";
				if (!string.IsNullOrWhiteSpace(intentName))
					whereCondition = $"{whereCondition} AND d.intent = '{intentName}'";

				if (!string.IsNullOrWhiteSpace(entity))
					whereCondition = $"{whereCondition} AND e['value'] = '{entity}'";

				if (!string.IsNullOrWhiteSpace(profile))
					whereCondition = $"{whereCondition} AND p.id = '{profile}'";

				CosmoOperation cosmoOperation = await CosmosDBOperations.QueryDBAsync(new CosmoOperation()
				{
					Collection = Environment.GetEnvironmentVariable(Config.COSMOS_COLLECTION),
					Database = Environment.GetEnvironmentVariable(Config.COSMOS_DATABASE),
					Payload = $"SELECT d as document FROM Data d JOIN e IN d.entities JOIN p IN e.profiles {whereCondition}"
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
