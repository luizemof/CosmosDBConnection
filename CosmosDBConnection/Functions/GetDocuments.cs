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

namespace CosmosDBConnection.Functions
{
	public static class GetDocuments
	{
		[FunctionName("GetDocuments")]
		public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
		{
			log.Info("GetDocuments requested");

			try
			{
				string type = req.GetQueryNameValuePairs()
					.FirstOrDefault(q => string.Compare(q.Key, "type", true) == 0)
					.Value;

				string whereClause = !string.IsNullOrWhiteSpace(type) ? $"WHERE c.type = '{type}'" : string.Empty;
				CosmoOperation cosmoOperation = await CosmosDBOperations.QueryDBAsync(new CosmoOperation()
				{
					Collection = Environment.GetEnvironmentVariable(Config.COSMOS_COLLECTION),
					Database = Environment.GetEnvironmentVariable(Config.COSMOS_DATABASE),
					Payload = $"SELECT * FROM c {whereClause}"
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
