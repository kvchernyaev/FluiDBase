using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using UtfUnknown;
using System.Linq;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace FluiDBase
{
    public class UpdateCommand : ICommand
    {
        public string Name => "update";

        static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();


        readonly CommonGatherer _commonGatherer;
        readonly FileReader _fileReader;


        public UpdateCommand(CommonGatherer commonGatherer, FileReader fileReader)
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

            foreach (var c in changesets)
                Console.WriteLine($"{c.FileRelPath} {c.Id}");
        }


        /*
        преобразовать с DEFINE + properties
        посчитать чексуммы
        проверить накатанность

        накатить + пометить
         */

    }
}
