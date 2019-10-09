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



        public SqlRawGatherer()
        {
        }



        public bool DoesMatch(string fileType, string fileContents)
        {
            return fileType.Equals(".sql", StringComparison.InvariantCultureIgnoreCase)
                && !Regex.IsMatch(fileContents, @"^\s*--\s*fluidbase", RegexOptions.IgnoreCase);
        }


        /// <exception cref="ProcessException"></exception>
        public void GatherFromFile(string fileContents, Dictionary<string, string> properties, FileDescriptor fileDescriptor, List<ChangeSet> changesets)
        {
            try
            {
                var changeset = ChangeSet.CheckAndCreate(
                    id: fileDescriptor.PathFromParent,
                    fileDescriptor: fileDescriptor.Parent,
                    author: "",
                    "false", "false", changesets);
                changesets.Add(changeset);
                Logger.Trace("changeset [{id}] in [{file}] is added", changeset.Id, changeset.FileRelPath);
            }
            catch (ArgumentException ex)
            {
                throw new ProcessException($"{fileDescriptor.Path}: {ex.Message}");
            }
        }
    }
}
