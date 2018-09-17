using CosmosDBConnection.Constants;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosDBConnection.Tools.Jwt
{
	internal class JwtValidator
	{
		public static bool Validate(string token)
		{
			TokenValidationParameters validationParameters =
			new TokenValidationParameters
			{
				ValidIssuer = Environment.GetEnvironmentVariable(Config.TOKEN_ISSUE),
				ValidAudiences = new[] { Environment.GetEnvironmentVariable(Config.TOKEN_ISSUE) },
				IssuerSigningKey = JwtConstants.SigningConfigurations.Key
			};
			var _user = new JwtSecurityTokenHandler().ValidateToken(token, validationParameters, out SecurityToken validatedToken);

			return _user.Identity.IsAuthenticated;
		}
	}
}
