using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluiDBase.Gather;

namespace FluiDBase
{
    public class GathererFabric
    {
        readonly List<IGatherer> Gatherers = new List<IGatherer>();

        public GathererFabric(IEnumerable<IGatherer> gatherers) => Gatherers.AddRange(gatherers);


        public GathererFabric(params IGatherer[] gatherers) => Gatherers.AddRange(gatherers);


        public void Add(IEnumerable<IGatherer> gatherers) => Gatherers.AddRange(gatherers);


        public void Add(params IGatherer[] gatherers) => Gatherers.AddRange(gatherers);


        public void Replace(params IGatherer[] gatherers)
        {
            Gatherers.Clear();
            Gatherers.AddRange(gatherers);
        }


        public IGatherer GetGatherer(string fileType, string fileContents) 
            => Gatherers.FirstOrDefault(g => g.DoesMatch(fileType, fileContents));
    }
}
