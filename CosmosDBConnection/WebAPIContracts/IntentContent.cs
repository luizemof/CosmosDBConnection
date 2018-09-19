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

		public string intent { get; set; }

		public List<IntentEntities> entities { get; set; }

		public string type { get; set; }

		public string partitionKey { get; set; }
	}
	
	internal class IntentEntities
	{
		public string value { get; set; }
		public List<IntentEntitiesProfile> profiles { get; set; }
	}

	internal class IntentEntitiesProfile
	{
		public string id { get; set; }
		public Content content { get; set; }
	}

	internal class Content
	{
		public string type { get; set; }
		public string value { get; set; }
	}
}