using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CosmosDBConnection.Constants;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace CosmosDBConnection.Functions
{
	public static class GetAnswer
	{
		[FunctionName("GetAnswer")]
		public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
		{
			log.Info("GetAnswer message received");

			try
			{
				string query = req.GetQueryNameValuePairs()
					.FirstOrDefault(q => string.Compare(q.Key, "query", true) == 0)
					.Value;

				if (string.IsNullOrWhiteSpace(query))
					throw new Exception("Missing query");

				string intentName = await GetLuisIntent(query);
				return req.CreateResponse(HttpStatusCode.OK, await _GetAnswer(intentName, req));
			}
			catch (Exception ex)
			{
				log.Error("Error: ", ex);
				return req.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
			}
		}

		private static async Task<object> _GetAnswer(string intentName, HttpRequestMessage req)
		{
			using (HttpClient client = new HttpClient())
			{
				UriBuilder uriBuilder = new UriBuilder
				{
					Host = req.RequestUri.Host,
					Scheme = req.RequestUri.Scheme,
					Port = req.RequestUri.Port,
					Path = "api/GetIntentAnswer",
					Query = $"intent={intentName}"
				};
				HttpResponseMessage result = await client.GetAsync(uriBuilder.ToString());
				if (result.StatusCode != HttpStatusCode.OK)
					throw new Exception("Error");

				return JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(await result.Content.ReadAsStringAsync());
			}
		}

		private static async Task<string> GetLuisIntent(string query)
		{
			string url = $"https://{Environment.GetEnvironmentVariable(Config.LUIS_DOMAIN)}/luis/v2.0/apps/{Environment.GetEnvironmentVariable(Config.LUIS_MODEL_ID)}?subscription-key={Environment.GetEnvironmentVariable(Config.LUIS_SUBSCRIPTION_KEY)}&verbose=true&timezoneOffset=0&q={query}";
			string intentName = string.Empty;
			using (HttpClient client = new HttpClient())
			{
				HttpResponseMessage response = await client.GetAsync(url);
				string content = await response.Content.ReadAsStringAsync();
				if (!string.IsNullOrWhiteSpace(content))
				{
					dynamic result = JsonConvert.DeserializeObject(content);
					intentName = result.topScoringIntent.intent;
				}
			}

			return intentName;
		}
	}
}
