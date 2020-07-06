using Elasticsearch.Net;
using Geonorge.NedlastingIndex.Models;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;

namespace Geonorge.NedlastingIndex.Services.Index
{
    public class ElasticDocumentIndexer : IDocumentIndexer
    {

        private readonly ElasticClient _client;

        public ElasticDocumentIndexer(AppSettings appSettings)
        {
            var settings = new ConnectionSettings(new Uri(appSettings.ElasticSearchHostname)).DefaultIndex(appSettings.ElasticSearchIndexName);

            _client = new ElasticClient(settings);
        }

        public async Task Index(Document document)
        {
            Log.Debug("Indexing document with title: " + document.Title);

            var asyncIndexResponse = await _client.IndexDocumentAsync(document);
            await _client.IndexDocumentAsync(document);

            Log.Debug("Response from indexing request: " + asyncIndexResponse.Result.GetStringValue());
        }
    }
}