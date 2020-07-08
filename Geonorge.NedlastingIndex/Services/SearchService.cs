using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Geonorge.NedlastingIndex.Models;
using Microsoft.AspNetCore.Server.Kestrel.Core;
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
            //Todo handle input parameters

            string metadataUuid = "4b6da2fb-67f9-4cab-ad18-ae064eb135e1";
            string title = "bygningspunkt";

            QueryContainer uuidFilter = null;
            QueryContainer titleFilter = null;

            if (!string .IsNullOrEmpty(metadataUuid))
                uuidFilter = new QueryContainerDescriptor<Dataset>()
                .Terms(c => c.Field(p => p.MetadataUuid).Terms(metadataUuid));

            if (!string.IsNullOrEmpty(title))
                titleFilter = new QueryContainerDescriptor<Dataset>()
                .Terms(c => c.Field(p => p.Title).Terms(title));

            var searchResponse = _client.Search<Dataset>(s => s
                .Query(q => +uuidFilter && titleFilter && q
                    .Nested(n => n
                        .InnerHits()
                        .Path(b => b.Files)
                        .Query(nq =>
                            nq.Match(m0 => m0.Field(f0 => f0.Files.First().CoverageType).Query("fylke")) &&
                            nq.Match(m1 => m1.Field(f1 => f1.Files.First().Area).Query("11")) &&
                            nq.Match(m2 => m2.Field(f2 => f2.Files.First().Projection).Query("25832")) &&
                            nq.Match(m3 => m3.Field(f3 => f3.Files.First().Format).Query("FGDB"))
                            )
                    )));

            //Get only files matching
            foreach (var hit in searchResponse.Hits)
            {
                var file = hit.InnerHits["file"].Documents<File>();
            }

            return searchResponse.Documents.ToList();
        }
    }
}
