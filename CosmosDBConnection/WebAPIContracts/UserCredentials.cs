using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosDBConnection.WebAPIContracts
{
	public class UserCredentials
	{
		[JsonProperty("login")]
		public string Login { get; set; }

		[JsonProperty("password")]
		public string Password { get; set; }
	}
}
