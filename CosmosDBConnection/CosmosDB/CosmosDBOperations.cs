using CosmosDBConnection.Constants;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosDBConnection.CosmosDB
{
	internal struct CosmoOperation
	{
		public string Database;
		public string Collection;
		public string Payload;
		public dynamic Results;
	}

	internal static partial class CosmosDBOperations
	{
		#region Lazy Client

		private static Lazy<DocumentClient> lazyDBClient = new Lazy<DocumentClient>(() =>
		{
			Uri endpoint = new Uri(Environment.GetEnvironmentVariable( Config.COSMOS_URI));
			string authKey = Environment.GetEnvironmentVariable(Config.COSMOS_KEY);
			return new DocumentClient(endpoint, authKey);
		});
		private static DocumentClient CosmosDBClient => lazyDBClient.Value;
		#endregion

		public static async Task<CosmoOperation> QueryDBAsync(CosmoOperation OperationInfo)
		{
			try
			{
				string db = OperationInfo.Database;
				string col = OperationInfo.Collection;

				IDocumentQuery<dynamic> query = CosmosDBClient
					.CreateDocumentQuery(
						UriFactory.CreateDocumentCollectionUri(OperationInfo.Database, OperationInfo.Collection),
						OperationInfo.Payload,
						new FeedOptions { EnableCrossPartitionQuery = true }
					).AsDocumentQuery();

				OperationInfo.Results = await GetAllResultsAsync(query);
			}
			catch (Exception ex)
			{
				OperationInfo.Results = ex;
			}

			return OperationInfo;
		}

		private async static Task<T[]> GetAllResultsAsync<T>(IDocumentQuery<T> queryAll)
		{
			var list = new List<T>();
			while (queryAll.HasMoreResults)
			{
				FeedResponse<T> docs = await queryAll.ExecuteNextAsync<T>();
				foreach (var d in docs)
					list.Add(d);
			}
			return list.ToArray();
		}

		public static async Task<CosmoOperation> UpsertDocumentAsync(CosmoOperation OperationInfo)
		{
			try
			{
				string db = OperationInfo.Database;
				string col = OperationInfo.Collection;
				Dictionary<string, object> document = JsonConvert.DeserializeObject<Dictionary<string, object>>(OperationInfo.Payload);

				OperationInfo.Results = await CosmosDBClient.UpsertDocumentAsync(UriFactory.CreateDocumentCollectionUri(db, col), document);
				OperationInfo.Results = ((ResourceResponse<Document>)OperationInfo.Results).Resource;
			}
			catch (Exception ex)
			{
				OperationInfo.Results = ex;
			}

			return OperationInfo;
		}
	}
}
