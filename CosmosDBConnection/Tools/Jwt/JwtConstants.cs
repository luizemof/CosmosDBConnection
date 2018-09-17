using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CosmosDBConnection.Tools.Jwt
{
	internal static class JwtConstants
	{
		public static SigningConfigurations SigningConfigurations = new SigningConfigurations();
	}
}
