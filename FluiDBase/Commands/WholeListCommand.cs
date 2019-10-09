using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluiDBase.Commands
{
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


        public void Execute(CommandLineArgs args)
        {
            FileDescriptor fileDescriptorFirst = new FileDescriptor(args.ChangeLogFile.FullName, _fileReader);
            List<ChangeSet> changesets = _commonGatherer.ProcessFile(fileDescriptorFirst);

            for (int i = 0; i < changesets.Count; i++)
            {
                ChangeSet c = changesets[i];
                string addon = string.Join(", ", new[] {
                    c.RunAlways ? "runAlways" : null,
                    c.RunOnChange ? "runOnChange" : null }
                .Where(x => x != null));
                Console.WriteLine($"{i} [{c.FileRelPath}] : [{c.Id}] {(string.IsNullOrWhiteSpace(addon) ? "" : $"({addon})")}");
            }
        }
    }
}
