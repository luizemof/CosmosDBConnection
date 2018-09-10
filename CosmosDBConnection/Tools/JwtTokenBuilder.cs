using CosmosDBConnection.Constants;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CosmosDBConnection.Tools
{
	internal class JwtTokenBuilder
	{
		private SecurityKey securityKey = null;
		private string subject = "";
		private string issuer = "";
		private string audience = "";
		private string nameId = "";
		private Dictionary<string, string> claims = new Dictionary<string, string>();
		private int expiryInMinutes = 5;

		public JwtTokenBuilder AddSecurityKey(SecurityKey securityKey)
		{
			this.securityKey = securityKey;
			return this;
		}

		public JwtTokenBuilder AddSubject(string subject)
		{
			this.subject = subject;
			return this;
		}

		public JwtTokenBuilder AddIssuer(string issuer)
		{
			this.issuer = issuer;
			return this;
		}

		public JwtTokenBuilder AddAudience(string audience)
		{
			this.audience = audience;
			return this;
		}

		public JwtTokenBuilder AddNameId(string nameId)
		{
			this.nameId = nameId;
			return this;
		}

		public JwtTokenBuilder AddClaim(string type, string value)
		{
			this.claims.Add(type, value);
			return this;
		}

		public JwtTokenBuilder AddClaims(Dictionary<string, string> claims)
		{
			Dictionary<string, string> _claims = this.claims.Union(claims).ToDictionary(k => k.Key, v => v.Value);
			this.claims = _claims;
			return this;
		}

		public JwtTokenBuilder AddExpiry(int expiryInMinutes)
		{
			this.expiryInMinutes = expiryInMinutes;
			return this;
		}

		public string Build()
		{
			var claims = new List<Claim>
			{
			  new Claim(JwtRegisteredClaimNames.Sub, subject),
			  new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
			  new Claim("NameId", this.nameId)
			}
			.Union(this.claims.Select(item => new Claim(item.Key, item.Value)));

			var token = new JwtSecurityToken(
							  issuer: this.issuer,
							  audience: this.audience,
							  claims: claims,
							  expires: DateTime.UtcNow.AddMinutes(expiryInMinutes),
							  signingCredentials: new SigningCredentials(
														this.securityKey,
														SecurityAlgorithms.Sha512));

			return new JwtSecurityTokenHandler().WriteToken(token);
		}

	}

	internal static class JwtSecurityKey
	{
		public static SymmetricSecurityKey Create(string secret) =>
			new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

	}

	internal static class JwtTokenValidator
	{
		public static bool Validate(string token, out Dictionary<string, object> user)
		{
			TokenValidationParameters validationParameters =
			new TokenValidationParameters
			{
				ValidIssuer = Environment.GetEnvironmentVariable(Config.TOKEN_ISSUE),
				ValidAudiences = new[] { Environment.GetEnvironmentVariable(Config.TOKEN_ISSUE) },
				IssuerSigningKey = JwtSecurityKey.Create(Environment.GetEnvironmentVariable(Config.TOKEN_KEY))
			};
			var _user = new JwtSecurityTokenHandler().ValidateToken(token, validationParameters, out SecurityToken validatedToken);

			if (_user.Identity.IsAuthenticated)
			{
				user = JsonConvert.DeserializeObject<Dictionary<string, object>>(_user.FindFirst("User").Value);
				return true;
			}
			else
			{
				user = new Dictionary<string, object>();
				return false;
			}
		}
	}
}
