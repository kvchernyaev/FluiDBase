using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace FluiDBase.Gather
{
    public class SqlRawGatherer : IGatherer
    {
        static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        readonly Filter _filter;


        public SqlRawGatherer(Filter filter)
        {
            _filter = filter;
        }


        public bool DoesMatch(string fileType, string fileContents)
        {
            return fileType.Equals(SqlFileType, StringComparison.InvariantCultureIgnoreCase)
                && !Regex.IsMatch(fileContents, @"^\s*--\s*fluidbase", RegexOptions.IgnoreCase);
        }


        const string SqlFileType = ".sql";
        static readonly string[] _PossibleFileTypes = new[] { SqlFileType };
        public string[] PossibleFileTypes => _PossibleFileTypes;


        /// <exception cref="ProcessException"></exception>
        public void GatherFromFile(string fileContents, IDictionary<string, string> properties, FileDescriptor fileDescriptor, List<ChangeSet> changesets, string[] contextsFromParents, Dictionary<string,string> args)
        {
            if (_filter.Exclude(context: null, useForEmpty: true))
            {
                Logger.Trace("sql file [{id}] in [{file}] is EXCLUDED by context", fileDescriptor.PathFromParent, fileDescriptor.Parent.PathFromBase);
                return;
            }

            try
            {
                var changeset = ChangeSet.ValidateAndCreate(
                    id: fileDescriptor.PathFromParent,
                    fileDescriptor: fileDescriptor.Parent,
                    author: args.TryGetValue("author", out string author) ? author : null,
                    args,
                    changesets
                );
                changeset.Contexts = contextsFromParents;

                changeset.SetBody(fileContents, properties);

                changesets.Add(changeset);
                Logger.Info("changeset [{id}] in [{file}] is added", changeset.Id, changeset.FileRelPath);
            }
            catch (ArgumentException ex)
            {
                throw new ProcessException($"{fileDescriptor.Path}: {ex.Message}");
            }
        }
    }
}
