using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        private readonly IDocumentIndexer _documentIndexer;

        public SearchIndexController(IFeedIndexer feedIndexer, IDocumentIndexer documentIndexer)
        {
            _feedIndexer = feedIndexer;
            _documentIndexer = documentIndexer;
        }

        [HttpGet]
        public async Task<ActionResult> Get()
        {
            //Todo index new feed https://nedlasting.geonorge.no/geonorge/ATOM/INSPIRE/Geonorge_INSPIRE_ServiceFeed.xml and ngu
            await _feedIndexer.Index(new AtomFeed() { Url = "https://nedlasting.geonorge.no/geonorge/Tjenestefeed.xml" });

            return RedirectToAction("get","search");
        }

        [HttpGet("createindex")]
        public async Task<ActionResult> CreateIndex()
        {
            await _documentIndexer.CreateIndex();

            return Ok();
        }

        [HttpGet("createsample")]
        public async Task<ActionResult> CreateSample()
        {
            await _documentIndexer.CreateSample();

            return Ok();
        }
    }
}
