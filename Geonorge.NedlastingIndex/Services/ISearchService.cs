using Geonorge.NedlastingIndex.Models;
using Microsoft.AspNetCore.Mvc;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Geonorge.NedlastingIndex.Services
{
    public interface ISearchService
    {
        List<Dataset> Search();
    }
}
