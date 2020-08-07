using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Geonorge.NedlastingIndex.Models;
using Geonorge.NedlastingIndex.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Geonorge.NedlastingIndex.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        ISearchService _searchService;
        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }

        [HttpPost]
        public List<Dataset> Get([FromBody] SearchParameters searchParameters) 
        {
            return _searchService.Search(searchParameters);
        }

        public List<Dataset> Get()
        {
            return _searchService.Search(new SearchParameters());
        }
    }
}
