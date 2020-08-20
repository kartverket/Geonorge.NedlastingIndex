using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Geonorge.NedlastingIndex.Models
{
    public class SearchResult
    {
        public List<Facet> Facets { get; set; }
        public List<Dataset> Datasets { get; set; }

        
    }

    public class Facet
    {
        public string FacetField { get; set; }
        public List<FacetValue> FacetResults { get; set; }

        public class FacetValue
        {
            public string Name { get; set; }
            public int Count { get; set; }

            public FacetValue(string key, int count)
            {
                Name = key;
                Count = count;
            }

            public FacetValue()
            {

            }
        }


        public Facet()
        {
        }

        public Facet(string key)
        {
            FacetField = key;
            FacetResults = new List<FacetValue>();
        }
    }
}
