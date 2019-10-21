using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluiDBase.Commands
{
    /// <summary>
    /// Вывести список всех используемых в ченжсетах контекстов
    /// </summary>
    public class AllContextsCommand : ICommand
    {
        public string Name => "allcontexts";

        static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();


        readonly CommonGatherer _commonGatherer;
        readonly FileReader _fileReader;


        public AllContextsCommand(CommonGatherer commonGatherer, FileReader fileReader)
        {
            _commonGatherer = commonGatherer;
            _fileReader = fileReader;
        }


        public Filter CreateFilter(string[] allowedContexts, bool emptyContextAllowed)
        {
            return new FilterNoneIfNoContext(allowedContexts, emptyContextAllowed);
        }


        public void Execute(CommandLineArgs args)
        {
            FileDescriptor fileDescriptorFirst = new FileDescriptor(args.ChangeLogFile.FullName, _fileReader);
            List<ChangeSet> changesets = _commonGatherer.ProcessFile(fileDescriptorFirst);

            foreach (string cs in changesets
                .Select(c => c.Contexts)
                .Where(x => x != null && x.Length > 0)
                .Select(x => string.Join(" & ", x))
                .Distinct()
                )
                Console.WriteLine(cs);
        }


        
        public class FilterNoneIfNoContext : Filter
        {
            public FilterNoneIfNoContext(string[] allowedContexts, bool emptyContextAllowed)
                : base(allowedContexts, emptyContextAllowed)
            {
            }


            protected override bool ExcludeByContext(string context, bool useForEmpty)
            {
                if (_allowedContexts == null || _allowedContexts.Length == 0)
                    return false;

                return base.ExcludeByContext(context, useForEmpty);
            }
        }

    }
}
