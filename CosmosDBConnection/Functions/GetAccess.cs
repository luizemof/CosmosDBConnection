using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace CosmosDBConnection.Functions
{
	public static class GetAccess
	{
		[FunctionName("GetAccess")]
		public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
		{
			log.Info("C# HTTP trigger function processed a request.");

			return await Task.Factory.StartNew(() => req.CreateResponse(HttpStatusCode.OK, "GetAccess OK"));
		}
	}
}
