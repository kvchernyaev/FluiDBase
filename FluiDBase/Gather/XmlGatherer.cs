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
        public void GatherFromFile(string fileContents, IDictionary<string, string> properties, FileDescriptor fileDescriptor, List<ChangeSet> changesets, string[] contextsFromParents, Dictionary<string, string> args)
        {
            if (_defaultFilesPattern == null)
                throw new Exception(nameof(XmlGatherer) + " is not initialized");

            XDocument xmlDoc = XDocument.Parse(fileContents);

            foreach (XElement elem in xmlDoc.Root.Elements())
            {
                string context = AttrVal(elem, "context").TrimOrNullIfEmpty();
                string[] contexts = contextsFromParents.Concat(context);

                // endpoint - so filter it now
                if (IsTag(elem, "changeSet"))
                {
                    if (!_filter.Exclude(context, useForEmpty: true))
                        ProcessChangeSetTag(elem, fileDescriptor, properties, changesets, contexts);
                    else
                        Logger.Trace("changeset [{id}] in [{file}] is EXCLUDED by context", AttrVal(elem, "id"), fileDescriptor.PathFromBase);
                }
                else if (IsTag(elem, "property"))
                {
                    if (!_filter.Exclude(context, useForEmpty: true))
                        ProcessPropertyTag(elem, fileDescriptor, properties);
                    else
                        Logger.Trace("property [{id}] in [{file}] is EXCLUDED by context", AttrVal(elem, "name"), fileDescriptor.PathFromBase);
                }
                //
                // middle point - filter only if context is exist
                else if (IsTag(elem, "include"))
                {
                    if (!_filter.Exclude(context, useForEmpty: false))
                        ProcessIncludeTag(elem, fileDescriptor, properties, changesets, contexts);
                    else
                        Logger.Trace("include [{id}] in [{file}] is EXCLUDED by context", AttrVal(elem, "file"), fileDescriptor.PathFromBase);
                }
                else if (IsTag(elem, "includeAll"))
                {
                    if (!_filter.Exclude(context, useForEmpty: false))
                        ProcessIncludeAllTag(elem, fileDescriptor, properties, changesets, contexts);
                    else
                        Logger.Trace("includeAll [{id}] in [{file}] is EXCLUDED by context", AttrVal(elem, "dir"), fileDescriptor.PathFromBase);
                }
                else
                    throw new ProcessException($"{fileDescriptor.Path}: tag {Name(elem)} is not supported");
            }
        }
               

        #region process tag
        void ProcessChangeSetTag(XElement elem, FileDescriptor fileDescriptor, IEnumerable<KeyValuePair<string, string>> properties, List<ChangeSet> changesets, string[] contexts)
        {
            //  failOnError
            try
            {
                Dictionary<string, string> args = elem.Attributes().ToDictionary(a => a.Name.LocalName, a => a.Value);

                var changeset = ChangeSet.ValidateAndCreate(AttrVal(elem, "id"), fileDescriptor,
                    AttrVal(elem, "author"),
                    args,
                    changesets
                );
                changeset.Contexts = contexts;

                changeset.SetBody(elem.Value.Trim(), properties);

                changesets.Add(changeset);
                Logger.Info("changeset [{id}] in [{file}] is added", changeset.Id, changeset.FileRelPath);
            }
            catch (ArgumentException ex)
            {
                throw new ProcessException($"{fileDescriptor.Path}: {ex.Message}");
            }

            // todo sub: comment, preConditions, sql (and split it by [go]), properties
        }


        void ProcessPropertyTag(XElement elem, FileDescriptor fileDescriptor, IDictionary<string, string> properties)
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


        void ProcessIncludeTag(XElement elem, FileDescriptor fileDescriptor, IDictionary<string, string> properties, List<ChangeSet> changesets, string[] contexts)
        {
            string childFileAbsPath = GetCombinedPathExistent(elem, "file", fileDescriptor, isDir: false);

            Dictionary<string, string> args = elem.Attributes().Where(a => a.Name.LocalName != "file")
                .ToDictionary(a => a.Name.LocalName, a => a.Value);

            FileDescriptor childFileDescriptor = new FileDescriptor(childFileAbsPath, fileDescriptor);
            _commonGatherer.ProcessFile(childFileDescriptor, new Dictionary<string, string>(properties), changesets, contexts, args);
        }


        void ProcessIncludeAllTag(XElement elem, FileDescriptor fileDescriptor, IDictionary<string, string> properties, List<ChangeSet> changesets, string[] contexts)
        {
            string childDirAbsPath = GetCombinedPathExistent(elem, "dir", fileDescriptor, isDir: true);

            Dictionary<string, string> args = elem.Attributes().Where(a => a.Name.LocalName != "dir")
                .ToDictionary(a => a.Name.LocalName, a => a.Value);

            string filesPattern = AttrVal(elem, "filesPattern") ?? _defaultFilesPattern;
            string[] childFileAbsPaths = _fileReader.GetFiles(childDirAbsPath, filesPattern);
            // todo exclude pattern for <includeAll> ?

            foreach (FileDescriptor childFileDescriptor in childFileAbsPaths
                .Select(childFileAbsPath => new FileDescriptor(childFileAbsPath, fileDescriptor))
                )
                _commonGatherer.ProcessFile(childFileDescriptor, new Dictionary<string, string>(properties), changesets, contexts, args);
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
