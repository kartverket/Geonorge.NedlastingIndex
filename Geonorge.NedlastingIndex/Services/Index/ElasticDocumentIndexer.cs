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
        private readonly AppSettings _appSettings;

        public ElasticDocumentIndexer(AppSettings appSettings)
        {
            _appSettings = appSettings;
            var settings = new ConnectionSettings(new Uri(_appSettings.ElasticSearchHostname)).DefaultIndex(_appSettings.ElasticSearchIndexName);

            _client = new ElasticClient(settings);
        }

        public async Task Index(Document document)
        {
            Log.Debug("Indexing document with title: " + document.Title);

            var asyncIndexResponse = await _client.IndexDocumentAsync(document);
            await _client.IndexDocumentAsync(document);

            Log.Debug("Response from indexing request: " + asyncIndexResponse.Result.GetStringValue());
        }

        public async Task CreateIndex()
        {
            Log.Debug("Delete index " + _appSettings.ElasticSearchIndexName);
            await _client.Indices.DeleteAsync(_appSettings.ElasticSearchIndexName);

            Log.Debug("Create index " + _appSettings.ElasticSearchIndexName);

            var createIndexResponse = _client.Indices.Create(_appSettings.ElasticSearchIndexName, c => c
                    .Map<Dataset>(m => m
                        .AutoMap()
                        .Properties(ps => ps
                            .Nested<File>(n => n
                                .Name(nn => nn.Files)
                                .AutoMap()
                            )
                        )
                    )
                );

            Log.Debug("Response from createIndex: " + createIndexResponse.DebugInformation);
        }
    }
}