using System.Collections.Generic;

namespace Geonorge.NedlastingIndex.Controllers
{
    public class SearchParameters
    {
        public string text { get; set; }
        public string[] coveragetypes { get; set; }
        public string[] areas { get; set; }
        public string[] projections { get; set; }
        public string[] formats { get; set; }
    }
}