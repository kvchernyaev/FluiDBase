using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FluiDBase.Gather
{
    public class SqlGatherer : IGatherer
    {
        static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        readonly Filter _filter;
        ArgsParser _sqlHeaderLineParser = new ArgsParser(':');


        public SqlGatherer(Filter filter)
        {
            _filter = filter;
        }



        public bool DoesMatch(string fileType, string fileContents)
        {
            return fileType.Equals(SqlFileType, StringComparison.InvariantCultureIgnoreCase) &&
                Regex.IsMatch(fileContents, @"^\s*--\s*fluidbase", RegexOptions.IgnoreCase);
        }


        const string SqlFileType = ".sql";
        static readonly string[] _PossibleFileTypes = new[] { SqlFileType };
        public string[] PossibleFileTypes => _PossibleFileTypes;


        /// <exception cref="ProcessException"></exception>
        public void GatherFromFile(string fileContents, Dictionary<string, string> properties, FileDescriptor fileDescriptor, List<ChangeSet> changesets, string[] contextsFromParents, Dictionary<string, string> argsFromParent)
        {
            if (!Regex.IsMatch(fileContents, @"^\s*--\s*fluidbase", RegexOptions.IgnoreCase))
                throw new Exception($"{nameof(SqlGatherer)} - file without [-- fluidbase] prefix"); // must not be

            string[] changesetStrings = SplitFileForChangesets(fileContents);
            foreach (string changesetString in changesetStrings)
            {
                SplitByFirstNewline(changesetString, out string headerLine, out string body);
                headerLine = headerLine.Trim();
                body = body.Trim();

                Logger.Trace("sql changeset header line: {headerline}", headerLine);

                try
                {
                    List<KeyValuePair<string, string>> args = ParseHeader(headerLine);
                    if (args.Count == 0)
                        throw new ProcessException($"{fileDescriptor.Path}: empty headerline - must be at least [author:id]");
                    string author = args[0].Key;
                    string id = args[0].Value;

                    if (string.IsNullOrWhiteSpace(id))
                        throw new ProcessException($"{fileDescriptor.Path}: changeset with empty id (headerline: [{headerLine}])");

                    var argsDict = new Dictionary<string, string>(args.Skip(1)/*skip author:id*/);
                    string context;
                    argsDict.TryGetValue("context", out context);

                    if (_filter.Exclude(context, useForEmpty: true))
                    {
                        Logger.Trace("changeset [{id}] in [{file}] is EXCLUDED", id, fileDescriptor.PathFromBase);
                        continue;
                    }

                    var changeset = ChangeSet.ValidateAndCreate(id, fileDescriptor,
                        author,
                        argsDict,
                        changesets);
                    changeset.Contexts = contextsFromParents.Concat(context);

                    // todo fileContents: use properties, check preconditions, calc hashsum

                    changesets.Add(changeset);
                    Logger.Info("changeset [{id}] in [{file}] is added", changeset.Id, changeset.FileRelPath);
                }
                catch (ArgumentException ex)
                {
                    throw new ProcessException($"{fileDescriptor.Path}: {ex.Message}");
                }
            }
        }


        /// <exception cref="ArgumentException"></exception>
        List<KeyValuePair<string, string>> ParseHeader(string headerLine)
        {
            try
            {
                return _sqlHeaderLineParser.Parse(headerLine);
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException($"changeset bad headerline: [{headerLine}] - {ex.Message}");
            }
        }


        static string[] SplitFileForChangesets(string fileContents)
        {
            return Regex.Split(fileContents, @"^\s*--\s*changeset", RegexOptions.Multiline | RegexOptions.IgnoreCase)
                .Skip(1)
                .ToArray();
        }


        void SplitByFirstNewline(string s, out string first, out string second)
        {
            int i = s.IndexOf('\n');
            if (i < 0)
            {
                first = s;
                second = "";
                return;
            }
            first = s.Substring(0, i);
            second = i < s.Length - 1 ? s.Substring(i + 1) : "";
        }


    }
}
