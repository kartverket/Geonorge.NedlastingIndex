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
        XNamespace gn = "http://geonorge.no/geonorge";

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

                List<File> files = new List<File>();

                foreach (var elements in parsedDetailsFeed.Items)
                {
                    var entries = elements.SpecificItem.Element;

                    var projection = entries.Element(ns + "category").Attribute("code")?.Value;

                    var format = entries.Element(gn + "format")?.Value;

                    var links = entries.Elements(ns + "link").ToList();

                    foreach(var link in links) 
                    {
                       var coverageType = link.Attribute(gn + "coveragetype")?.Value;

                       var area = link.Attribute(gn + "coveragecode")?.Value;

                       var url = link.Attribute("href")?.Value;

                        files.Add(new File 
                        { 
                            CoverageType = coverageType,
                            Area = area,
                            Projection = projection,
                            Format = format,
                            Url = url
                        });
                    }

                }
                    

                var dataset = new Dataset()
                {
                    Title = item.Title,
                    MetadataUuid = uuid,
                    Files = files
                };
                await _documentIndexer.Index(dataset);
            }

        }
    }
}