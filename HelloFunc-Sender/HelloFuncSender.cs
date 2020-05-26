using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;

namespace HelloFunc_Sender
{
    public static class HelloFuncSender
    {
        [FunctionName("HelloFuncSender")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string name = req.Query["name"];
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            string azureBaseUrl = Environment.GetEnvironmentVariable("AzureFunctionURL") + name;
            string message = string.Empty;
            try
            {
                using (var client = new HttpClient())
                {
                    using (HttpResponseMessage res = await client.GetAsync(azureBaseUrl))
                    {
                        using (HttpContent content = res.Content)
                        {
                            string response = await content.ReadAsStringAsync();
                            if (data != null)
                            {
                                message = response;
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                message = ex.Message;
            }

            string responseMessage = string.IsNullOrEmpty(message)
                ? "This HTTP triggered HelloFuncSender function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered HelloFuncSender function executed successfully and called Receiver function with below message.";
            responseMessage += message;

            return new OkObjectResult(responseMessage);
        }

    }
}
