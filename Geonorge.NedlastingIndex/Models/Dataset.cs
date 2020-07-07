using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Geonorge.NedlastingIndex.Models
{
    [ElasticsearchType(IdProperty = nameof(MetadataUuid))]
    public class Dataset
    {
        [Keyword]
        public string MetadataUuid { get; set; }
        public string Title { get; set; }

        [Nested]
        [PropertyName("file")]
        public List<File> Files { get; set; }
    }
}
