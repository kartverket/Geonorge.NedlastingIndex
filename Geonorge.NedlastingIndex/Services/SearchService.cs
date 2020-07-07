using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Geonorge.NedlastingIndex.Models;
using Nest;

namespace Geonorge.NedlastingIndex.Services
{
    public class SearchService : ISearchService
    {
        private readonly ElasticClient _client;
        public SearchService(AppSettings appSettings)
        {
            var settings = new ConnectionSettings(new Uri(appSettings.ElasticSearchHostname)).DefaultIndex(appSettings.ElasticSearchIndexName);

            _client = new ElasticClient(settings);
        }

        public List<Dataset> Search() 
        { 
            //Todo build dynamic query
            var searchResponse = _client.Search<Dataset>(s => s
            .From(0)
            .Size(10)
            );

         return searchResponse.Documents.ToList();
        }
    }
}
