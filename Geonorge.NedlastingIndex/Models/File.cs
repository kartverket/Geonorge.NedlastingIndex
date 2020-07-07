using Nest;
namespace Geonorge.NedlastingIndex.Models
{
    public class File
    {
        [Keyword]
        public string CoverageType { get; set; }
        [Keyword]
        public string Area { get; set; }
        [Keyword]
        public string Projection { get; set; }
        [Keyword]
        public string Format { get; set; }
        [Keyword]
        public string Url { get; set; }
    }
}