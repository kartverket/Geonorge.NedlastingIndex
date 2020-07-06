using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Geonorge.NedlastingIndex.Models;
using Geonorge.NedlastingIndex.Services.Index;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Serilog;

namespace Geonorge.NedlastingIndex.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SearchIndexController : ControllerBase
    {
        private readonly IFeedIndexer _feedIndexer;

        public SearchIndexController(IFeedIndexer feedIndexer)
        {
            _feedIndexer = feedIndexer;
        }

        [HttpGet]
        public async Task<ActionResult> Get()
        {
            await _feedIndexer.Index(new AtomFeed() { Url = "https://nedlasting.geonorge.no/geonorge/Tjenestefeed.xml" });

            return RedirectToAction("get","search");
        }
    }
}
