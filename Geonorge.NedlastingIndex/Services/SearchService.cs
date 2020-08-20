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

        public SearchResult Search(SearchParameters searchParameters)
        {
            int size = 10000;
            List<Dataset> datasets = new List<Dataset>();
            string text = searchParameters.text;
            string[] coverageTypes = searchParameters.coveragetypes;
            string[] areas = searchParameters.areas;
            string[] projections = searchParameters.projections;
            string[] formats = searchParameters.formats;

            var filters = new List<Func<QueryContainerDescriptor<Dataset>, QueryContainer>>();

            if (coverageTypes != null && coverageTypes.Length > 0)
            {
                filters.Add(nq => nq.Terms(m0 => m0.Field(f0 => f0.Files.First().CoverageType).Terms(coverageTypes)));
            }

            if (areas != null && areas.Length > 0)
            {
                filters.Add(nq => nq.Terms(m1 => m1.Field(f1 => f1.Files.First().Area).Terms(areas)));
            }

            if (projections != null && projections.Length > 0)
            {
                filters.Add(nq => nq.Terms(m2 => m2.Field(f2 => f2.Files.First().Projection).Terms(projections)));
            }

            if (formats != null && formats.Length > 0)
            {
                filters.Add(nq => nq.Terms(m3 => m3.Field(f3 => f3.Files.First().Format).Terms(formats)));
            }

            if (!string.IsNullOrEmpty(text))
                text = "*" + text + "*";

            var searchResponse = _client.Search<Dataset>(s => s
                .Size(size)
                .Query(q => q
                .Wildcard(m => m
                    .Field(t => t.Title)
                    .Value(text)
                )
                && q.Nested(n => n
                        .InnerHits()
                        .Path(b => b.Files)
                        .Query(nq => nq.Bool(bq => bq.Filter(filters))

                                )
                            )
                    )

                .Aggregations(a => a
                    .Nested("facets", n => n
                    .Path(p => p.Files)
                    .Aggregations(a => a
                        .Terms("coverageType", t => t.Field(s => s.Files.First().CoverageType).Size(size))
                        .Terms("area", t => t.Field(s => s.Files.First().Area).Size(size))
                        .Terms("projection", t => t.Field(s => s.Files.First().Projection).Size(size))
                        .Terms("format", t => t.Field(s => s.Files.First().Format).Size(size))
                        )
                    )
                    //.Terms("capabilitiesMetadata", t => t.Field(s => s.MetadataUuid).Size(size)
                    //.Aggregations(b => b
                    //.Nested("files", nn => nn
                    //.Path(pp => pp.Files).Aggregations(aa => aa
                    //.Terms("coverageType", t => t.Field(s => s.Files.First().CoverageType).Size(size))
                    //.Terms("area", t => t.Field(s => s.Files.First().Area).Size(size))
                    //.Terms("projection", t => t.Field(s => s.Files.First().Projection).Size(size))
                    //.Terms("format", t => t.Field(s => s.Files.First().Format).Size(size))
                    //))))
                )
            );
            
            SingleBucketAggregate facets = (SingleBucketAggregate)searchResponse.Aggregations["facets"];

            List<Facet> facetResult = new List<Facet>();

            for (int f = 0; f < facets.Keys.Count(); f++)
            {
                string key = facets.Keys.ElementAt(f);

                Facet facet = new Facet(key);

                BucketAggregate bucketAggregate = (BucketAggregate)facets.Values.ElementAt(f);
                var items= bucketAggregate.Items;
                foreach(KeyedBucket<object> bucked in items) 
                {
                    var facetCount = (int)bucked.DocCount;
                    var facetValue = bucked.Key.ToString();

                    facet.FacetResults.Add(new Facet.FacetValue(facetValue, facetCount));
                }

                facetResult.Add(facet);
            }

            SearchResult searchResult = new SearchResult();

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

            searchResult.Datasets = datasets;
            searchResult.Facets = facetResult;

            return searchResult;
        }

    }
}
