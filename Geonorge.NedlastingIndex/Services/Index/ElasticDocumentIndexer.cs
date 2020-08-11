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

        public async Task Index(Dataset document)
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


        public async Task CreateSample()
        {
            Log.Debug("Create sample document");

            var dataset = new Dataset
            {
                MetadataUuid = "4b6da2fb-67f9-4cab-ad18-ae064eb135e1",
                Title = "Matrikkelen - Bygningspunkt"
            };

            List<File> files = new List<File>();

            files.Add(new File 
            { 
                CoverageType = "fylke",
                Area = "11",
                Projection = "25832",
                Format = "FGDB",
                Url = "https://nedlasting.geonorge.no/geonorge/Basisdata/Fylker/FGDB/Basisdata_11_Rogaland_25832_Fylker_FGDB.zip"
            });

            files.Add(new File
            {
                CoverageType = "fylke",
                Area = "15",
                Projection = "25832",
                Format = "FGDB",
                Url = "https://nedlasting.geonorge.no/geonorge/Basisdata/Fylker/FGDB/Basisdata_15_More_og_Romsdal_25832_Fylker_FGDB.zip"
            });

            files.Add(new File
            {
                CoverageType = "landsdekkende",
                Area = "0000",
                Projection = "25833",
                Format = "GML",
                Url = "https://nedlasting.geonorge.no/geonorge/Basisdata/Fylker/GML/Basisdata_0000_Norge_25833_Fylker_GML.zip"
            });

            dataset.Files = files;

            var asyncIndexResponse = await _client.IndexDocumentAsync(dataset);
            Log.Debug("Response from create sample request: " + asyncIndexResponse.Result.GetStringValue());
        }
    }
}