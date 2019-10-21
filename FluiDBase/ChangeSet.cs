using System;
using System.Collections.Generic;
using System.Text;

namespace FluiDBase
{
    public class ChangeSet
    {
        public string Id;
        public string FileRelPath;

        public string Author;
        public bool RunAlways;
        public bool RunOnChange;

        /// <summary>
        /// May be a chain - if include|includeAll has [@context] attr
        /// </summary>
        public string[] Contexts;

        public ChangeSet(string id, string fileRelPath)
        {
            Id = id;
            FileRelPath = fileRelPath;
        }


        /// <exception cref="ArgumentException"></exception>
        public static ChangeSet ValidateAndCreate(string id, FileDescriptor fileDescriptor, string author, 
            string runAlwaysString, string runOnChangeString, List<ChangeSet> changesets)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("changeset with empty [@id] attribute");
            if (changesets.Exists(c => c.FileRelPath == fileDescriptor.PathFromBase && c.Id == id))
                throw new ArgumentException($"changset [{id}] is duplicated");
            //if (string.IsNullOrWhiteSpace(author))
            //    throw new ProcessException("{0}: changeset [{1}] with empty [@author] attribute", currentFilePath, id);

            bool runAlways;
            if (runAlwaysString == null)
                runAlways = false;
            else if (!bool.TryParse(runAlwaysString, out runAlways))
                throw new ArgumentException($"changeset [{id}] - wrong attribute [@runAlways] value ({runAlwaysString})");

            bool runOnChange;
            if (runOnChangeString == null)
                runOnChange = false;
            else
                if (!bool.TryParse(runOnChangeString, out runOnChange))
                throw new ArgumentException($"changeset [{id}] - wrong attribute [@runOnChange] value ({runOnChangeString})");

            var changeset = new ChangeSet(id, fileDescriptor.PathFromBase)
            {
                Author = author,
                RunAlways = runAlways,
                RunOnChange = runOnChange,
            };

            return changeset;
        }
    }
}
