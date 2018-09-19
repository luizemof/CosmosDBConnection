using CosmosDBConnection.Constants;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CosmosDBConnection.CosmosDB
{
	internal struct CosmoOperation
	{
		public string Database;
		public string Collection;
		public object Payload;
		public dynamic Results;
	}

	internal static partial class CosmosDBOperations
	{
		#region Lazy Client

		private static Lazy<DocumentClient> lazyDBClient = new Lazy<DocumentClient>(() =>
		{
			Uri endpoint = new Uri(Environment.GetEnvironmentVariable(Config.COSMOS_URI));
			string authKey = Environment.GetEnvironmentVariable(Config.COSMOS_KEY);
			return new DocumentClient(endpoint, authKey);
		});
		private static DocumentClient CosmosDBClient => lazyDBClient.Value;
		#endregion

		public static async Task<CosmoOperation> QueryDBAsync(CosmoOperation operation)
		{
			try
			{
				string db = operation.Database;
				string col = operation.Collection;

				IDocumentQuery<dynamic> query = CosmosDBClient
					.CreateDocumentQuery(
						UriFactory.CreateDocumentCollectionUri(operation.Database, operation.Collection),
						operation.Payload.ToString(),
						new FeedOptions { EnableCrossPartitionQuery = true }
					).AsDocumentQuery();

				operation.Results = await GetAllResultsAsync(query);
			}
			catch (Exception ex)
			{
				operation.Results = ex;
			}

			return operation;
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

		public static async Task<CosmoOperation> UpsertDocumentAsync(CosmoOperation operation)
		{
			try
			{
				string db = operation.Database;
				string col = operation.Collection;

				operation.Results = await CosmosDBClient.UpsertDocumentAsync(UriFactory.CreateDocumentCollectionUri(db, col), operation.Payload);
				operation.Results = ((ResourceResponse<Document>)operation.Results).Resource;
			}
			catch (Exception ex)
			{
				operation.Results = ex;
			}

			return operation;
		}

		public static async Task<CosmoOperation> DeleteDocumentAsync(CosmoOperation operation)
		{
			try
			{
				string db = operation.Database;
				string col = operation.Collection;

				operation.Results = await CosmosDBClient.DeleteDocumentAsync(
					UriFactory.CreateDocumentUri(db, col, ((dynamic)operation.Payload).id),
					new RequestOptions() { PartitionKey = new PartitionKey(((dynamic)operation.Payload).partitionKey) });

				operation.Results = ((ResourceResponse<Document>)operation.Results).Resource;
			}
			catch (Exception ex)
			{
				operation.Results = ex;
			}

			return operation;
		}
	}
}
