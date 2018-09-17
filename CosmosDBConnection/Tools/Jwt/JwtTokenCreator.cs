using CosmosDBConnection.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosDBConnection.Tools.Jwt
{
	internal static class JwtTokenCreator
	{
		internal static string CreateJwtToken(string subject, string nameid, Dictionary<string, string> claims)
		{
			string token = new JwtTokenBuilder()
				.AddSubject(subject)
				.AddIssuer(Environment.GetEnvironmentVariable(Config.TOKEN_ISSUE))
				.AddAudience(Environment.GetEnvironmentVariable(Config.TOKEN_ISSUE))
				.AddNameId(nameid)
				.AddClaims(claims)
				.AddExpiry(Convert.ToInt32(Environment.GetEnvironmentVariable(Config.TOKEN_EXPIRATION)))
				.Build();
			return token;
		}
	}
}
