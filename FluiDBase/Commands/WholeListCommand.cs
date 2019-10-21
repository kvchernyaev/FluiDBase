using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluiDBase.Commands
{
    /// <summary>
    /// Вывести список всех ченжсетов (для данного контекста), то есть без проверки примененности и preconditions
    /// </summary>
    public class WholeListCommand : ICommand
    {
        public string Name => "wholeList";

        static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();


        readonly CommonGatherer _commonGatherer;
        readonly FileReader _fileReader;


        public WholeListCommand(CommonGatherer commonGatherer, FileReader fileReader)
        {
            _commonGatherer = commonGatherer;
            _fileReader = fileReader;
        }


        public Filter CreateFilter(string[] allowedContexts, bool emptyContextAllowed)
        {
            return new Filter(allowedContexts, emptyContextAllowed);
        }


        public void Execute(CommandLineArgs args)
        {
            FileDescriptor fileDescriptorFirst = new FileDescriptor(args.ChangeLogFile.FullName, _fileReader);
            List<ChangeSet> changesets = _commonGatherer.ProcessFile(fileDescriptorFirst);

            for (int i = 0; i < changesets.Count; i++)
                Console.WriteLine($"{i} {AsString(changesets[i])}");
        }


        string AsString(ChangeSet c)
        {
            string addon = string.Join(", ", new[] {
                    c.RunAlways ? "runAlways" : null,
                    c.RunOnChange ? "runOnChange" : null,
                    c.Contexts == null || c.Contexts.Length == 0 ? null : string.Join(" & ", c.Contexts),
            }
            .Where(x => x != null));
            return $"[{c.FileRelPath}] : [{c.Id}] {(string.IsNullOrWhiteSpace(addon) ? "" : $"({addon})")}";
        }
    }
}
