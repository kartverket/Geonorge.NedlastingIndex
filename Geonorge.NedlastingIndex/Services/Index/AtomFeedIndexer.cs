using CodeHollow.FeedReader;
using Geonorge.FeedReader;
using Geonorge.NedlastingIndex.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Geonorge.NedlastingIndex.Services.Index
{
    public class AtomFeedIndexer : IFeedIndexer
    {
        private readonly IDocumentIndexer _documentIndexer;

        public AtomFeedIndexer(IDocumentIndexer documentIndexer)
        {
            _documentIndexer = documentIndexer;
        }

        public async Task Index(AtomFeed feed)
        {
            Feed parsedRootFeed = await CodeHollow.FeedReader.FeedReader.ReadAsync(feed.Url);

            foreach (var item in parsedRootFeed.Items)
            {
                await _documentIndexer.Index(new Document() // Todo index as Dataset
                {
                    Title = item.Title,
                    Description = item.Description,
                    Epsg = item.GetGeonorgeFeedItem().Epsg?.Value
                });
            }

        }
    }
}