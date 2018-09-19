using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosDBConnection.WebAPIContracts
{
	internal class IntentContent
	{
		public string id { get; set; }

		public List<Intents> intents { get; set; }

		public Content content { get; set; }

		public string type { get; set; }

		public string partitionKey { get; set; }
	}

	internal class Intents
	{
		public string name { get; set; }
		public string profile { get; set; }
	}

	internal class Content
	{
		public string type { get; set; }
		public string value { get; set; }
	}
}