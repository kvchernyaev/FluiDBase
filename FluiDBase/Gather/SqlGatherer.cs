using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FluiDBase.Gather
{
public    class SqlGatherer : IGatherer
    {

        static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();



        public SqlGatherer()
        {
        }



        public bool DoesMatch(string fileType, string fileContents)
        {
            return fileType.Equals(".sql", StringComparison.InvariantCultureIgnoreCase) &&
                Regex.IsMatch(fileContents, @"^\s*--\s*fluidbase", RegexOptions.IgnoreCase);
        }




        /// <exception cref="ProcessException"></exception>
        public void GatherFromFile(string fileContents, Dictionary<string, string> properties, FileDescriptor fileDescriptor, List<ChangeSet> changesets)
        {
            if (!Regex.IsMatch(fileContents, @"^\s*--\s*fluidbase", RegexOptions.IgnoreCase))
                throw new Exception($"{nameof(SqlGatherer)} - file without [-- fluidbase] prefix");

            string[] changesetsString = Regex.Split(fileContents, @"^\s*--\s*changeset", RegexOptions.Multiline | RegexOptions.IgnoreCase);

            foreach (string changesetString in changesetsString.Skip(1))
            {
                SplitByFirstNewline(changesetString, out string headerLine, out string body);
                headerLine = headerLine.Trim();
                body = body.Trim();

                Logger.Trace("sql changeset header line: {headerline}", headerLine);

                string author;
                string id;
                string runAlwaysString = "false", runOnChangeString = "false";


                try
                {
                    var p = new ArgsParser(':');
                    List<KeyValuePair<string, string>> args = p.Parse(headerLine);

                    author = args[0].Key;
                    id = args[0].Value;

                    foreach (var kvp in args.Skip(1))
                    {
                        if (kvp.Key == "runAlways")
                        {
                            runAlwaysString = string.IsNullOrWhiteSpace(kvp.Value)? "false" : kvp.Value;
                        }
                        else if (kvp.Key == "runOnChange")
                        {
                            runOnChangeString = string.IsNullOrWhiteSpace(kvp.Value) ? "false" : kvp.Value;
                        }
                        else
                            throw new ProcessException("attribute [{0}] is not supported", kvp.Key);
                    }
                }
                catch (ArgumentException ex)
                {
                    throw new ProcessException("{0}: changeset bad headerline: [{1}] - {2}", fileDescriptor.Path, headerLine, ex.Message);
                }
                catch (ProcessException ex)
                {
                    throw new ProcessException("{0}: changeset bad headerline: [{1}] - {2}", fileDescriptor.Path, headerLine, ex.Message);
                }

                if (string.IsNullOrWhiteSpace(id))
                    throw new ProcessException("{0}: changeset with empty id (headerline: [{1}])", fileDescriptor.Path, headerLine);

                try
                {
                    var changeset = ChangeSet.CheckAndCreate(id, fileDescriptor,
                        author,
                        runAlwaysString, runOnChangeString,
                        changesets);

                    changesets.Add(changeset);
                    Logger.Trace("changeset [{id}] in [{file}] is added", changeset.Id, changeset.FileRelPath);
                }
                catch (ArgumentException ex)
                {
                    throw new ProcessException($"{fileDescriptor.Path}: {ex.Message}");
                }
            }
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
