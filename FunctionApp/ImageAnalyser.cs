using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace FunctionApp
{
    public static class ImageAnalyser
    {
        [FunctionName("ImageAnalyser")]
        public static async Task Run([BlobTrigger("post-images/{name}", Connection = "blob_conn")]Stream myBlob, string name, [SignalR(HubName = "chat")] IAsyncCollector<SignalRMessage> signalRMessages,
        ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");


            Analysis imageAnalysis = await AnalyzeImage_ByAPIAsync(myBlob, log);

            log.LogInformation("\nTags:\n" + String.Join('\n', imageAnalysis.tags.Select(x => x.name)));

            log.LogInformation("\nFaces:\n" + String.Join('\n', imageAnalysis.faces.Select(x => "*" + x.gender + " of age " + x.age + " years")));

            log.LogInformation("\nDescription:\n" + String.Join('\n', imageAnalysis.description.captions.Select(x => "* " + x.text)));

            await signalRMessages.AddAsync(
             new SignalRMessage
             {
                 Target = "SendAll",
                 Arguments = new[] {
                 String.Join("<br>", imageAnalysis.tags.Select(x => x.name)),
                 String.Join("<br>", imageAnalysis.faces.Select(x => x.gender + " of age " + x.age + " years")),
                 String.Join("<br>", imageAnalysis.description.captions.Select(x => x.text))
                 }
             });
        }

        private static async Task<Analysis> AnalyzeImage_ByAPIAsync(Stream image, ILogger log)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var content = new StreamContent(image);
                    var url =
                   "https://eastus.api.cognitive.microsoft.com/vision/v1.0/analyze?visualFeatures=Faces,Description,Tags&language=en";
                    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key",
        "b894f5db6793443f89a84631294b714a");
                    content.Headers.ContentType = new
                   MediaTypeHeaderValue("application/octet-stream");
                    var httpResponse = await client.PostAsync(url, content);

                    string analysisText = string.Empty;

                    if (httpResponse.StatusCode == HttpStatusCode.OK)
                    {
                        analysisText = await httpResponse.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<Analysis>(analysisText);
                    }
                }
            }
            catch (Exception exception)
            {
                log.LogError("", exception);
            }
            return null;
        }
    }
}
