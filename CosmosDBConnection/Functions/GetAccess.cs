using CosmosDBConnection.Constants;
using CosmosDBConnection.CosmosDB;
using CosmosDBConnection.Tools;
using CosmosDBConnection.WebAPIContracts;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace CosmosDBConnection.Functions
{
	public static class GetAccess
	{
		[FunctionName("GetAccess")]
		public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
		{
			log.Info("GetAccess message received");

			try
			{
				log.Info("Reading body message");

				UserCredentials userCredentials = JsonConvert.DeserializeObject<UserCredentials>(await req.Content.ReadAsStringAsync());
				if (userCredentials == null || string.IsNullOrWhiteSpace(userCredentials.Login) || string.IsNullOrWhiteSpace(userCredentials.Password))
					return req.CreateResponse(HttpStatusCode.Unauthorized, "Missing credentials");

				log.Info("Getting User");
				CosmoOperation cosmoOperation = await CosmosDBOperations.QueryDBAsync(new CosmoOperation()
				{
					Collection = Environment.GetEnvironmentVariable(Config.COSMOS_COLLECTION),
					Database = Environment.GetEnvironmentVariable(Config.COSMOS_DATABASE),
					Payload = $"SELECT * FROM c WHERE c.type = 'User' AND c.login = '{userCredentials.Login}' AND c.password = '{userCredentials.Password}'"
				});

				if (cosmoOperation.Results == null || cosmoOperation.Results.Length == 0)
					return req.CreateResponse(HttpStatusCode.Unauthorized, "Invalid credentials");

				if (cosmoOperation.Results.Length > 1)
					throw new Exception("More than one user has found");

				Dictionary<string, object> user = JsonConvert.DeserializeObject<Dictionary<string, object>>(cosmoOperation.Results[0].ToString());
				string token = JwtTokenCreator.CreateJwtToken(
					(string)user["name"],
					(string)user["id"],
					new Dictionary<string, string>() { { "user", JsonConvert.SerializeObject(user) } });

				return await Task.Factory.StartNew(() => req.CreateResponse(HttpStatusCode.OK, token));
			}
			catch (Exception ex)
			{
				log.Error("Erro: ", ex);
				return req.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
			}
		}
	}
}
