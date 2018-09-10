using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosDBConnection.Constants
{
	internal static class Config
	{
		public const string COSMOS_COLLECTION = "COSMOS_DATA_COLLECTION";
		public const string COSMOS_DATABASE = "COSMOS_DATA_DATABASE";
		public const string COSMOS_URI = "COSMOS_URI";
		public const string COSMOS_KEY = "COSMOS_KEY";

		public const string TOKEN_ISSUE = "TOKEN_ISSUER";
		public const string TOKEN_KEY = "TOKEN_KEY";
		public const string TOKEN_EXPIRATION = "TOKEN_EXPIRATION";

		public const string LUIS_DOMAIN = "LUIS_DOMAIN";
		public const string LUIS_MODEL_ID = "LUIS_MODEL_ID";
		public const string LUIS_SUBSCRIPTION_KEY = "LUIS_SUBSCRIPTION_KEY";
	}
}
