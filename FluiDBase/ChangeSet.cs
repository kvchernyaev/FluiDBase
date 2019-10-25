using System;
using System.Collections.Generic;
using System.Linq;
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
            Dictionary<string, string> args,
            List<ChangeSet> changesets)
        {
            // validate
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("changeset with empty [@id] attribute");
            if (changesets.Exists(c => c.FileRelPath == fileDescriptor.PathFromBase && c.Id == id))
                throw new ArgumentException($"changset [{id}] is duplicated");
            //if (string.IsNullOrWhiteSpace(author))
            //    throw new ArgumentException($"changeset [{id}] with empty [@author] attribute");

            string[] forbidden = args.Keys.Except(allowedArgs).ToArray();
            if(forbidden.Length > 0)
                throw new ArgumentException($"changset [{id}]: attribute [{string.Join(", ", forbidden)}] is not supported");

            // parse
            bool runAlways, runOnChange;
            try
            {
                runAlways = TryGet(args, "runAlways", defaultIfNull: false);
                runOnChange = TryGet(args, "runOnChange", defaultIfNull: false);
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException($"changeset [{id}]: {ex.Message}");
            }

            // create
            var changeset = new ChangeSet(id, fileDescriptor.PathFromBase)
            {
                Author = author,
                RunAlways = runAlways,
                RunOnChange = runOnChange,
            };

            return changeset;
        }


        static readonly string[] allowedArgs = new[] { "runAlways", "runOnChange", "context", "id", "author" };
        

        /// <exception cref="ArgumentException">"attribute [@{name}] value ({s}) is wrong"</exception>
        static T TryGet<T>(IDictionary<string, string> d, string name, T defaultIfNull)
        {
            try
            {
                string s = TryGet(d, name);
                T rv = Parse(s, defaultIfNull);
                return rv;
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException($"attribute [@{name}]: {ex.Message}");
            }
        }


        static string TryGet(IDictionary<string, string> d, string name)
            => d != null && d.TryGetValue(name, out string value) ? value : null;


        /// <exception cref="ArgumentException">"value ({s}) is wrong"</exception>
        static T Parse<T>(string s, T defaultIfNull)
        {
            if (s == null)
                return defaultIfNull;
            try
            {
                T rv = (T)Convert.ChangeType(s, typeof(T));
                return rv;
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException($"value ({s}) is wrong");
            }
            catch (FormatException)
            {
                throw new ArgumentException($"value ({s}) is wrong");
            }
            catch (OverflowException)
            {
                throw new ArgumentException($"value ({s}) is wrong");
            }
        }
    }
}
