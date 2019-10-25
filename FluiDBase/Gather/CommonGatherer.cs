using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;
using FluiDBase.Gather;

namespace FluiDBase
{
    public class CommonGatherer
    {
        static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        readonly FileReader _fileReader;
        readonly GathererFabric _gathererFabric;


        public CommonGatherer(FileReader fileReader, GathererFabric gathererFabric)
        {
            _fileReader = fileReader;
            _gathererFabric = gathererFabric;
        }


        public List<ChangeSet> ProcessFile(FileDescriptor fileDescriptor)
        {
            List<ChangeSet> changesets = new List<ChangeSet>();
            ProcessFile(fileDescriptor, properties: new Dictionary<string, string>(), changesets, contexts: null, args: null);
            return changesets;
        }


        public void ProcessFile(FileDescriptor fileDescriptor, Dictionary<string, string> properties, List<ChangeSet> changesets, string[] contexts, Dictionary<string, string> args)
        {
            string fileContents = _fileReader.ReadFile(fileDescriptor.Path);

            IGatherer gatherer = _gathererFabric.GetGatherer(fileDescriptor.Type, fileContents);
            if(gatherer == null)
                throw new ProcessException("file type [{0}] is not supported (file [{1}] from [{2}])", fileDescriptor.Type, fileDescriptor.Path, fileDescriptor.Parent.Path);

            gatherer.GatherFromFile(fileContents, properties, fileDescriptor, changesets, contexts, args);
        }
    }
}
