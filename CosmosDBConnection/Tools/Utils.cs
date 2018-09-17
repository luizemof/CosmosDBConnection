using CosmosDBConnection.Constants;
using CosmosDBConnection.Tools.Jwt;
using System;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;

namespace CosmosDBConnection.Tools
{
	internal static class Utils
	{
		internal static string CreatePartitionKey(string prefix, string id)
		{
			string _partitionKey = string.Empty;
			prefix = prefix.Replace(" ", "");
			int numberOfPartitions = Convert.ToInt32(Environment.GetEnvironmentVariable(Config.COSMOS_PARTITIONS));
			using (MD5 _md5 = MD5.Create())
			{
				var hashedValue = _md5.ComputeHash(Encoding.UTF8.GetBytes(id));
				var asInt = BitConverter.ToInt32(hashedValue, 0);
				asInt = asInt == int.MinValue ? asInt + 1 : asInt;
				_partitionKey = $"{prefix}{Math.Abs(asInt) % numberOfPartitions}";
			}
			return _partitionKey;
		}

		internal static void ValidateAuthorizationHeader(AuthenticationHeaderValue authenticationHeaderValue)
		{
			string token = string.IsNullOrWhiteSpace(authenticationHeaderValue?.Scheme) ? string.Empty : authenticationHeaderValue.Parameter;
			if (string.IsNullOrWhiteSpace(token) || !JwtValidator.Validate(token))
				throw new UnauthorizedAccessException();
		}
	}
}
