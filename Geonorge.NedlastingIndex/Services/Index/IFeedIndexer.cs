﻿using Geonorge.NedlastingIndex.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Geonorge.NedlastingIndex.Services.Index
{
    public interface IFeedIndexer
    {
        Task Index(AtomFeed feed);
    }
}
