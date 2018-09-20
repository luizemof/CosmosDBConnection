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
		private class LuisModel
		{
			public LuisModel(string profile)
			{
				Profile = profile;
			}

			public string Profile { get; private set; }
			public string Domain { get; set; }
			public string ModelId { get; set; }
			public string Subscription { get; set; }
		}

		private static readonly Dictionary<string, LuisModel> DicLuisModels = new Dictionary<string, LuisModel>()
		{
			{
				"CF",
				new LuisModel("CF")
				{
					Domain = Environment.GetEnvironmentVariable(Config.LUIS_DOMAIN_CF),
					ModelId = Environment.GetEnvironmentVariable(Config.LUIS_MODEL_ID_CF),
					Subscription = Environment.GetEnvironmentVariable(Config.LUIS_SUBSCRIPTION_KEY_CF)
				}
			},
			{
				"CFD",
				new LuisModel("CFD")
				{
					Domain = Environment.GetEnvironmentVariable(Config.LUIS_DOMAIN_CFD),
					ModelId = Environment.GetEnvironmentVariable(Config.LUIS_MODEL_ID_CFD),
					Subscription = Environment.GetEnvironmentVariable(Config.LUIS_SUBSCRIPTION_KEY_CFD)
				}
			},
			{
				"CN", new LuisModel("CN")
				{
					Domain = Environment.GetEnvironmentVariable(Config.LUIS_DOMAIN_CN),
					ModelId = Environment.GetEnvironmentVariable(Config.LUIS_MODEL_ID_CN),
					Subscription = Environment.GetEnvironmentVariable(Config.LUIS_SUBSCRIPTION_KEY_CN)
				}
			},
			{
				"CND",
				new LuisModel("CND")
				{
					Domain = Environment.GetEnvironmentVariable(Config.LUIS_DOMAIN_CND),
					ModelId = Environment.GetEnvironmentVariable(Config.LUIS_MODEL_ID_CND),
					Subscription = Environment.GetEnvironmentVariable(Config.LUIS_SUBSCRIPTION_KEY_CND)
				}
			},
			{
				"LIDER",
				new LuisModel("LIDER")
				{
					Domain = Environment.GetEnvironmentVariable(Config.LUIS_DOMAIN_LIDER),
					ModelId = Environment.GetEnvironmentVariable(Config.LUIS_MODEL_ID_LIDER),
					Subscription = Environment.GetEnvironmentVariable(Config.LUIS_SUBSCRIPTION_KEY_LIDER)
				}
			},
		};

		[FunctionName("GetAnswer")]
		public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
		{
			log.Info("GetAnswer message received");

			try
			{
				string query = req.GetQueryNameValuePairs()
					.FirstOrDefault(q => string.Compare(q.Key, "query", true) == 0)
					.Value;

				string profile = req.GetQueryNameValuePairs()
					.FirstOrDefault(q => string.Compare(q.Key, "profile", true) == 0)
					.Value;

				if (string.IsNullOrWhiteSpace(query))
					throw new Exception("Missing query");

				if (string.IsNullOrWhiteSpace(profile))
					throw new Exception("Missing profile");

				if (!DicLuisModels.TryGetValue(profile.ToUpper(), out LuisModel luisModel))
					throw new Exception("Invalid profile");

				string intentName = await GetLuisIntent(query, luisModel);
				return req.CreateResponse(HttpStatusCode.OK, await _GetAnswer(intentName, luisModel.Profile, req));
			}
			catch (Exception ex)
			{
				log.Error("Error: ", ex);
				return req.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
			}
		}

		private static async Task<string> _GetAnswer(string intentName, string profile, HttpRequestMessage req)
		{
			using (HttpClient client = new HttpClient())
			{
				UriBuilder uriBuilder = new UriBuilder
				{
					Host = req.RequestUri.Host,
					Scheme = req.RequestUri.Scheme,
					Port = req.RequestUri.Port,
					Path = "api/GetIntentContentDocument",
					Query = $"intent={intentName}"
				};
				HttpResponseMessage result = await client.GetAsync(uriBuilder.ToString());
				if (result.StatusCode != HttpStatusCode.OK)
					throw new Exception("Error");

				Dictionary<string, object> obj = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(await result.Content.ReadAsStringAsync()).FirstOrDefault();
				if (obj.TryGetValue("document", out dynamic doc) && doc.entities != null)
				{
					foreach (var entity in doc.entities)
					{
						if (entity.profiles != null)
						{
							foreach (var itemProfile in entity.profiles)
							{
								if (itemProfile.id == profile)
									return itemProfile.content.value;
							}
						}
					}
				}
				return string.Empty;
			}
		}

		private static async Task<string> GetLuisIntent(string query, LuisModel luisModel)
		{
			string url = $"https://{luisModel.Domain}/luis/v2.0/apps/{luisModel.ModelId}?subscription-key={luisModel.Subscription}&verbose=true&timezoneOffset=0&q={query}";
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
