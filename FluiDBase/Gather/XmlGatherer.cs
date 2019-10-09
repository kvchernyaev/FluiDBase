using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace FluiDBase.Gather
{
    public class XmlGatherer : IGatherer
    {
        static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        readonly FileReader _fileReader;
        readonly CommonGatherer _commonGatherer;


        public XmlGatherer(FileReader fileReader, CommonGatherer commonGatherer)
        {
            _fileReader = fileReader;
            _commonGatherer = commonGatherer;
        }
        

        public bool DoesMatch(string fileType, string fileContents)
        {
            return fileType.Equals(".xml", StringComparison.InvariantCultureIgnoreCase);
        }


        /// <exception cref="ProcessException"></exception>
        public void GatherFromFile(string fileContents, Dictionary<string, string> properties, FileDescriptor fileDescriptor, List<ChangeSet> changesets)
        {
            XDocument xmlDoc = XDocument.Parse(fileContents);

            foreach (XElement elem in xmlDoc.Root.Elements())
            {
                // todo context/label filter

                if (elem.Name.LocalName.Equals("property", StringComparison.InvariantCultureIgnoreCase))
                {
                    string propName = elem.Attribute("name").Value;
                    string propValue = elem.Attribute("value").Value;

                    if (properties.ContainsKey(propName))
                        Logger.Trace("property {propName} updated with value {propValue}", propName, propValue);
                    else
                        Logger.Trace("property {propName} with value {propValue} added", propName, propValue);
                    properties[propName] = propValue;
                    continue;
                }
                else if (elem.Name.LocalName.Equals("include", StringComparison.InvariantCultureIgnoreCase))
                {
                    string childFileRelPath = elem.Attribute("file")?.Value;
                    if (string.IsNullOrWhiteSpace(childFileRelPath))
                        throw new ProcessException("{0}: <include> with empty [@file] attribute", fileDescriptor.Path);

                    string childFileAbsPath = _fileReader.CombinePath(fileDescriptor.Dir, childFileRelPath);

                    if (!_fileReader.FileExists(childFileAbsPath))
                        throw new ProcessException("{0}: file [{1}] does not exist", fileDescriptor.Path, childFileRelPath);

                    FileDescriptor childFileDescriptor = new FileDescriptor(childFileAbsPath, fileDescriptor);
                    _commonGatherer.ProcessFile(childFileDescriptor, new Dictionary<string, string>(properties), changesets);
                }
                else if (elem.Name.LocalName.Equals("includeAll", StringComparison.InvariantCultureIgnoreCase))
                {
                    string childDirRelPath = elem.Attribute("dir")?.Value;
                    if (string.IsNullOrWhiteSpace(childDirRelPath))
                        throw new ProcessException("{0}: <includeAll> with empty [@dir] attribute", fileDescriptor.Path);

                    string childDirAbsPath = _fileReader.CombinePath(fileDescriptor.Dir, childDirRelPath);

                    if (!_fileReader.DirectoryExists(childDirAbsPath))
                        throw new ProcessException("{0}: directory [{1}] does not exist", fileDescriptor.Path, childDirRelPath);

                    string filesPattern = elem.Attribute("filesPattern")?.Value ?? "*.xml|*.sql";
                    string[] childFileAbsPaths = _fileReader.GetFiles(childDirAbsPath, filesPattern);

                    foreach (FileDescriptor childFileDescriptor in childFileAbsPaths.Select(x => new FileDescriptor(x, fileDescriptor)))
                        _commonGatherer.ProcessFile(childFileDescriptor, new Dictionary<string, string>(properties), changesets);
                }
                else if (elem.Name.LocalName.Equals("changeSet", StringComparison.InvariantCultureIgnoreCase))
                {
                    //  failOnError
                    try
                    {
                        var changeset = ChangeSet.CheckAndCreate(elem.Attribute("id")?.Value, fileDescriptor,
                            elem.Attribute("author")?.Value,
                            elem.Attribute("runAlways")?.Value, elem.Attribute("runOnChange")?.Value,
                            changesets);

                        changesets.Add(changeset);
                        Logger.Trace("changeset [{id}] in [{file}] is added", changeset.Id, changeset.FileRelPath);
                    }
                    catch (ArgumentException ex)
                    {
                        throw new ProcessException($"{fileDescriptor.Path}: {ex.Message}");
                    }

                    // todo sub: comment preConditions sql (and split it by [go])
                }
            }
        }
    }
}
