using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Geonorge.NedlastingIndex.Controllers;
using Geonorge.NedlastingIndex.Models;
using Microsoft.AspNetCore.Mvc.Formatters;
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

            settings.EnableDebugMode(); settings.DisableDirectStreaming();
            settings.PrettyJson();


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
                filters.Add(nq => nq.Terms(m0 => m0.Field("file.coverageType").Terms(coverageTypes)));
            }

            if (areas != null && areas.Length > 0)
            {
                filters.Add(nq => nq.Terms(m1 => m1.Field("file.area").Terms(areas)));
            }

            if (projections != null && projections.Length > 0)
            {
                filters.Add(nq => nq.Terms(m2 => m2.Field("file.projection").Terms(projections)));
            }

            if (formats != null && formats.Length > 0)
            {
                filters.Add(nq => nq.Terms(m3 => m3.Field("file.format").Terms(formats)));
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
                        .InnerHits(ih => ih.From(0).Size(size))
                        .Path(b => b.Files)
                        .Query(nq => nq.Bool(bq => bq.Filter(filters))

                                )
                            )
                    )

                .Aggregations(a => a
                    .Nested("facets", n => n
                    .Path(p => p.Files)
                    .Aggregations(a => a
                        .Terms("coverageType", t => t.Field("file.coverageType").Size(size))
                        .Terms("area", t => t.Field("file.area").Size(size))
                        .Terms("projection", t => t.Field("file.projection").Size(size))
                        .Terms("format", t => t.Field("file.format").Size(size))
                        )
                    )
                )
            );

            List<Facet> facetResult = new List<Facet>();

            if (areas == null && coverageTypes == null && projections == null && formats == null) 
            { 
            
                SingleBucketAggregate facets = (SingleBucketAggregate)searchResponse.Aggregations["facets"];


                for (int f = 0; f < facets.Keys.Count(); f++)
                {
                    string key = facets.Keys.ElementAt(f);


                    Facet facet = new Facet(key);

                    BucketAggregate bucketAggregate = facets.Values.ElementAt(f) as BucketAggregate;
                    if (bucketAggregate != null) 
                    {
                        var items= bucketAggregate.Items;
                        foreach(KeyedBucket<object> bucked in items) 
                        {
                            var facetCount = (int)bucked.DocCount;
                            var facetValue = bucked.Key.ToString();

                            facet.FacetResults.Add(new Facet.FacetValue(facetValue, facetCount));
                        }

                        facetResult.Add(facet);
                    }
                }
            }

            SearchResult searchResult = new SearchResult();

            foreach (var hit in searchResponse.Hits)
            {
                Dataset dataset = new Dataset();
                dataset.Files = new List<File>();
                dataset.MetadataUuid = hit.Source.MetadataUuid;
                dataset.Title = hit.Source.Title;

                if (hit.InnerHits.Count > 0) {
                    if (facetResult.Count == 0) 
                    {
                        facetResult.Add(new Facet("area"));
                        facetResult.Add(new Facet("coverageType"));
                        facetResult.Add(new Facet("format"));
                        facetResult.Add(new Facet("projection"));
                    }

                    var innerhits = hit.InnerHits["file"].Documents<File>();

                    foreach (var innerhit in innerhits) 
                    {
                        var coverageType = innerhit.CoverageType;
                        var facetCoverage = facetResult[1].FacetResults.Where(f => f.Name == coverageType).FirstOrDefault();
                        if (facetCoverage == null)
                            facetResult[1].FacetResults.Add(new Facet.FacetValue { Name = coverageType, Count = 1 });
                        else
                            facetResult[1].FacetResults.Where(p => p.Name == coverageType).Select(u => { u.Count = u.Count + 1; return u; }).ToList();


                        var area = innerhit.Area;
                        var facetArea = facetResult[0].FacetResults.Where(f => f.Name == area).FirstOrDefault();
                        if (facetArea == null)
                            facetResult[0].FacetResults.Add(new Facet.FacetValue { Name = area, Count = 1 });
                        else
                            facetResult[0].FacetResults.Where(p => p.Name == area).Select(u => { u.Count = u.Count + 1; return u; }).ToList();

                        var format = innerhit.Format;
                        var facetFormat = facetResult[2].FacetResults.Where(f => f.Name == format).FirstOrDefault();
                        if (facetFormat == null)
                            facetResult[2].FacetResults.Add(new Facet.FacetValue { Name = format, Count = 1 });
                        else
                            facetResult[2].FacetResults.Where(p => p.Name == format).Select(u => { u.Count = u.Count + 1; return u; }).ToList();

                        var projection = innerhit.Projection;
                        var facetProjection = facetResult[3].FacetResults.Where(f => f.Name == projection).FirstOrDefault();
                        if (facetProjection == null)
                            facetResult[3].FacetResults.Add(new Facet.FacetValue { Name = projection, Count = 1 });
                        else
                            facetResult[3].FacetResults.Where(p => p.Name == projection).Select(u => { u.Count = u.Count + 1; return u; }).ToList();


                    }

                    dataset.Files.AddRange(hit.InnerHits["file"].Documents<File>());
                }
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
