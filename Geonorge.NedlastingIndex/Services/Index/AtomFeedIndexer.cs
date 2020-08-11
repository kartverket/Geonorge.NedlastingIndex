using CodeHollow.FeedReader;
using Geonorge.FeedReader;
using Geonorge.NedlastingIndex.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml;

namespace Geonorge.NedlastingIndex.Services.Index
{
    public class AtomFeedIndexer : IFeedIndexer
    {
        private readonly IDocumentIndexer _documentIndexer;

        XNamespace ns = "http://www.w3.org/2005/Atom";

        public AtomFeedIndexer(IDocumentIndexer documentIndexer)
        {
            _documentIndexer = documentIndexer;
        }

        public async Task Index(AtomFeed feed)
        {
            Feed parsedRootFeed = await CodeHollow.FeedReader.FeedReader.ReadAsync(feed.Url);

            foreach (var item in parsedRootFeed.Items)
            {
                XElement data = item.SpecificItem.Element;
                var uuid = data.Element(ns + "uuid")?.Value;

                Feed parsedDetailsFeed = await CodeHollow.FeedReader.FeedReader.ReadAsync(item.Id);
                foreach (var elements in parsedDetailsFeed.Items)
                {
                    var entries = elements.SpecificItem.Element;
                    var links = entries.Elements(ns + "link").ToList();
                }
                    

                var dataset = new Dataset()
                {
                    Title = item.Title,
                    MetadataUuid = uuid
                    //MetadataUuid = item.GetGeonorgeFeedItem().InspireSpatialDatasetIdentifierCode,
                    //Epsg = item.GetGeonorgeFeedItem().Epsg?.Value


                };
                await _documentIndexer.Index(dataset);
            }

        }
    }
}