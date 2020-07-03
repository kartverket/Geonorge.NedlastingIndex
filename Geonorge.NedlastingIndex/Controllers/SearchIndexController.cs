using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Geonorge.NedlastingIndex.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SearchIndexController : ControllerBase
    {
        private readonly ILogger<SearchIndexController> _logger;

        public SearchIndexController(ILogger<SearchIndexController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public string Get()
        {
            return "searchindex";
        }
    }
}
