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
        void GatherFromFile(string fileContents, Dictionary<string, string> properties, FileDescriptor fileDescriptor, List<ChangeSet> changesets);
    }
}
