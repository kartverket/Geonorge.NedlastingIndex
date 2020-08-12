using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Geonorge.NedlastingIndex.Controllers;
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

        public List<Dataset> Search(SearchParameters searchParameters)
        {

            List<Dataset> datasets = new List<Dataset>();
            string text = searchParameters.text;
            string coverageType = searchParameters.coveragetype;
            string area = searchParameters.area;
            string projection = searchParameters.projection;
            string format = searchParameters.format;

            var filters = new List<Func<QueryContainerDescriptor<Dataset>, QueryContainer>>();

            if (!string.IsNullOrEmpty(coverageType))
            {
                filters.Add(nq => nq.Match(m0 => m0.Field(f0 => f0.Files.First().CoverageType).Query(coverageType)));
            }

            if (!string.IsNullOrEmpty(area))
            {
                filters.Add(nq => nq.Match(m1 => m1.Field(f1 => f1.Files.First().Area).Query(area)));
            }

            if (!string.IsNullOrEmpty(projection))
            {
                filters.Add(nq => nq.Match(m2 => m2.Field(f2 => f2.Files.First().Projection).Query(projection)));
            }

            if (!string.IsNullOrEmpty(format))
            {
                filters.Add(nq => nq.Match(m3 => m3.Field(f3 => f3.Files.First().Format).Query(format)));
            }


            var searchResponse = _client.Search<Dataset>(s => s
                .Query(q => q
                .Match(m => m
                    .Field(t => t.Title)
                    .Query(text)
                )
                && q.Nested(n => n
                        .InnerHits()
                        .Path(b => b.Files)
                        .Query(nq => nq.Bool(bq => bq.Filter(filters))

                                )
                            )
                    )
            );
            //Get only files matching
            foreach (var hit in searchResponse.Hits)
            {
                Dataset dataset = new Dataset();
                dataset.Files = new List<File>();
                dataset.MetadataUuid = hit.Source.MetadataUuid;
                dataset.Title = hit.Source.Title;

                if (hit.InnerHits.Count > 0)
                    dataset.Files.AddRange(hit.InnerHits["file"].Documents<File>());
                else
                    dataset.Files.AddRange(hit.Source.Files);

                datasets.Add(dataset);
            }

            return datasets;
        }

    }
}
