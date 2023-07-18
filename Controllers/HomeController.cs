using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Http;
using System.IO;
using Eng_to_Chi.Models;
using TranslationSample;

namespace MyApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class TranslationController : Controller
    {
        public IActionResult Index()
        {
            var model = new TranslationViewModel
            {
                Test = "This is a test value"
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DownloadTranslatedText(IFormFile fileInput)
        {
            using (var reader = new StreamReader(fileInput.OpenReadStream()))
            {
                string text = reader.ReadToEnd();
                string translatedText = await TranslationService.TranslateText(text);
                byte[] fileContents = Encoding.UTF8.GetBytes(translatedText);
                return File(fileContents, "text/plain", "translated_text.txt");
            }
        }
    }

    public class TranslationViewModel
    {
        public string Test { get; set; }
    }
}

namespace TranslationSample
{
    public static class TranslationService
    {
        private const string SubscriptionKey = "API_KEY_HERE";
        private const string Endpoint = "https://api.cognitive.microsofttranslator.com/";
        private const string Route = "/translate?api-version=3.0&to=chinese";

        public static async Task<string> TranslateText(string FileContents)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", SubscriptionKey);

            var requestBody = new StringContent("[{\"Text\": \"" + FileContents + "\"}]", Encoding.UTF8, "application/json");

            var response = await client.PostAsync(Endpoint + Route, requestBody);
            var result = await response.Content.ReadAsStringAsync();
            var jsonResult = JArray.Parse(result);

            return jsonResult[0]["translations"][0]["text"].ToString();
        }
    }
}