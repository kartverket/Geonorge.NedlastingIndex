using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Geonorge.NedlastingIndex;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SearchApp.Models;

namespace SearchApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppSettings _appSettings;

        public HomeController(ILogger<HomeController> logger, AppSettings appSettings)
        {
            _logger = logger;
            _appSettings = appSettings;
        }

        public async Task<IActionResult> IndexAsync()
        {
            var search = new { text = "" };
            var jObject = JRaw.FromObject(search);

            JObject data;
            var url = _appSettings.SearchApiUrl;
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync(url))
                {
                    using (var content = response.Content)
                    {
                        var result = await content.ReadAsStringAsync();
                        data = (JObject)JsonConvert.DeserializeObject(result);
                    }
                }
            }

            SearchModel model = new SearchModel();
            model.model = data;
            model.parameters = jObject;

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SearchAsync(string text, string[] areas, string[] projections, string[] formats, string[] coverageTypes)
        {
            JObject data;
            var url = _appSettings.SearchApiUrl;

            //var jObject = JRaw.Parse(@"{""text"": ""FKB - Buildings"", ""coveragetypes"": [""fylke""], ""areas"": [""11""],""projections"": [""25832""], ""formats"": [""SOSI"", ""GML""]}");

            var search = new { text = text, areas = !IsNullOrEmpty(areas) ? areas : null , projections = !IsNullOrEmpty(projections) ? projections : null, formats = !IsNullOrEmpty(formats) ? formats : null, coverageTypes = !IsNullOrEmpty(coverageTypes) ? coverageTypes : null };
            var jObject = JRaw.FromObject(search);

            using (var httpClient = new HttpClient())
            {
                var contents = new StringContent(jObject.ToString(), Encoding.UTF8, "application/json");
                using (var response = await httpClient.PostAsync(url, contents))
                {
                    using (var content = response.Content)
                    {
                        var result = await content.ReadAsStringAsync();
                        data = (JObject)JsonConvert.DeserializeObject(result);
                    }
                }
            }

            //JObject model = data;

            SearchModel model = new SearchModel();
            model.model = data;
            model.parameters = jObject;

            return View("Index",model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        static bool IsNullOrEmpty(string[] myStringArray)
        {
            return myStringArray == null || myStringArray.Length < 1;
        }
    }

    public class SearchModel
    {
        public JObject model { get; set; }
        public JToken parameters { get; set; }
    }
}
