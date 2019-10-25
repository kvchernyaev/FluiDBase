using System;
using System.Collections.Generic;
using System.Text;

namespace FluiDBase.Gather
{
    public interface IGatherer
    {
        bool DoesMatch(string fileType, string fileContents);
        string[] PossibleFileTypes { get; }

        /// <exception cref="ProcessException"></exception>
        void GatherFromFile(string fileContents, IDictionary<string, string> properties, FileDescriptor fileDescriptor, List<ChangeSet> changesets, string[] contextsFromParents, Dictionary<string, string> args);
   }
}
