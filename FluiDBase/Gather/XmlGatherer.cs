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
        readonly Filter _filter;
        string _defaultFilesPattern;


        public XmlGatherer(FileReader fileReader, CommonGatherer commonGatherer, Filter filter)
        {
            _fileReader = fileReader;
            _commonGatherer = commonGatherer;
            _filter = filter;
        }


        public void Init(IEnumerable<IGatherer> gatherers)
        {
            _defaultFilesPattern = string.Join("|", gatherers.SelectMany(x => x.PossibleFileTypes).Distinct().Select(x => "*" + x));
        }


        public bool DoesMatch(string fileType, string fileContents)
        {
            return fileType.Equals(XmlFileType, StringComparison.InvariantCultureIgnoreCase);
        }


        const string XmlFileType = ".xml";
        static readonly string[] _PossibleFileTypes = new[] { XmlFileType };
        public string[] PossibleFileTypes => _PossibleFileTypes;


        /// <exception cref="ProcessException"></exception>
        public void GatherFromFile(string fileContents, Dictionary<string, string> properties, FileDescriptor fileDescriptor, List<ChangeSet> changesets)
        {
            if (_defaultFilesPattern == null)
                throw new Exception(nameof(XmlGatherer) + " is not initialized");

            XDocument xmlDoc = XDocument.Parse(fileContents);

            foreach (XElement elem in xmlDoc.Root.Elements())
            {
                string context = AttrVal(elem, "context");

                // endpoint - so filter it now
                if (IsTag(elem, "changeSet"))
                {
                    if (!_filter.Exclude(context, useForEmpty: true))
                        ProcessChangeSetTag(fileDescriptor, changesets, elem);
                }
                else if (IsTag(elem, "property"))
                {
                    if (!_filter.Exclude(context, useForEmpty: true))
                        ProcessPropertyTag(elem, properties, fileDescriptor);
                }
                //
                // middle point - filter only if context is exist
                else if (IsTag(elem, "include"))
                {
                    if (!_filter.Exclude(context, useForEmpty: false))
                        ProcessIncludeTag(elem, fileDescriptor, properties, changesets);
                }
                else if (IsTag(elem, "includeAll"))
                {
                    if (!_filter.Exclude(context, useForEmpty: false))
                        ProcessIncludeAllTag(elem, fileDescriptor, properties, changesets);
                }
                else
                    throw new ProcessException($"{fileDescriptor.Path}: tag {Name(elem)} is not supported");
            }
        }


        #region process tag
        void ProcessChangeSetTag(FileDescriptor fileDescriptor, List<ChangeSet> changesets, XElement elem)
        {
            //  failOnError
            try
            {
                var changeset = ChangeSet.CheckAndCreate(AttrVal(elem, "id"), fileDescriptor,
                    AttrVal(elem, "author"),
                    AttrVal(elem, "runAlways"), AttrVal(elem, "runOnChange"),
                    changesets);

                changesets.Add(changeset);
                Logger.Info("changeset [{id}] in [{file}] is added", changeset.Id, changeset.FileRelPath);
            }
            catch (ArgumentException ex)
            {
                throw new ProcessException($"{fileDescriptor.Path}: {ex.Message}");
            }

            // todo sub: comment preConditions sql (and split it by [go])
        }


        void ProcessPropertyTag(XElement elem, Dictionary<string, string> properties, FileDescriptor fileDescriptor)
        {
            string propName = AttrVal(elem, "name");
            string propValue = AttrVal(elem, "value");

            if (string.IsNullOrWhiteSpace(propName))
                throw new ProcessException("{0}: <property> with empty [@name] attribute", fileDescriptor.Path);

            if (properties.ContainsKey(propName))
                Logger.Trace("property {propName} updated with value {propValue}", propName, propValue);
            else
                Logger.Trace("property {propName} with value {propValue} added", propName, propValue);

            properties[propName] = propValue;
        }


        void ProcessIncludeTag(XElement elem, FileDescriptor fileDescriptor, Dictionary<string, string> properties, List<ChangeSet> changesets)
        {
            string childFileAbsPath = GetCombinedPathExistent(elem, "file", fileDescriptor, isDir: false);

            FileDescriptor childFileDescriptor = new FileDescriptor(childFileAbsPath, fileDescriptor);
            _commonGatherer.ProcessFile(childFileDescriptor, new Dictionary<string, string>(properties), changesets);
        }


        void ProcessIncludeAllTag(XElement elem, FileDescriptor fileDescriptor, Dictionary<string, string> properties, List<ChangeSet> changesets)
        {
            string childDirAbsPath = GetCombinedPathExistent(elem, "dir", fileDescriptor, isDir: true);

            string filesPattern = AttrVal(elem, "filesPattern") ?? _defaultFilesPattern;
            string[] childFileAbsPaths = _fileReader.GetFiles(childDirAbsPath, filesPattern);
            // todo exclude pattern for <includeAll> ?

            foreach (FileDescriptor childFileDescriptor in childFileAbsPaths
                .Select(childFileAbsPath => new FileDescriptor(childFileAbsPath, fileDescriptor))
                )
                _commonGatherer.ProcessFile(childFileDescriptor, new Dictionary<string, string>(properties), changesets);
        }
        #endregion


        string GetCombinedPathExistent(XElement elem, string attrName, FileDescriptor fileDescriptor, bool isDir)
        {
            string relPath = AttrVal(elem, attrName);
            if (string.IsNullOrWhiteSpace(relPath))
                throw new ProcessException("{0}: <{1}> with empty [@{2}] attribute", fileDescriptor.Path, Name(elem), attrName);

            string absPath = _fileReader.CombinePath(fileDescriptor.Dir, relPath);

            if (isDir ? !_fileReader.DirectoryExists(absPath) : !_fileReader.FileExists(absPath))
                throw new ProcessException("{0}: {2} [{1}] does not exist", fileDescriptor.Path, relPath, isDir ? "directory" : "file");

            return absPath;
        }


        static bool IsTag(XElement elem, string tag) => Name(elem).Equals(tag, StringComparison.InvariantCultureIgnoreCase);

        static string Name(XElement elem) => elem.Name.LocalName;
        static string AttrVal(XElement elem, string attrName) => elem.Attribute(attrName)?.Value;
    }
}
