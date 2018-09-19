using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosDBConnection.WebAPIContracts
{
	internal class Profiles
	{
		public string id { get; set; }
		public string type { get { return "Profiles"; } }
		public string partitionKey { get; set; }
		public List<Profile> profiles { get; set; }
	}

	internal class Profile
	{
		public string code { get; set; }
		public string name { get; set; }
		public string description { get; set; }
	}
}
