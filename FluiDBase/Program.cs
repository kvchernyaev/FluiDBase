using System;
using System.Linq;
using System.Text;
using CmdArgs;
using FluiDBase.Commands;
using FluiDBase.Gather;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace FluiDBase
{
    class Program
    {
        static NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        static void Main(string[] argsArray)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            SetupNLog(NLog.LogLevel.Error);
            Console.ForegroundColor = ConsoleColor.Yellow;

            try
            {
                CommandLineArgs args = PrepareArgs(argsArray);
                if (args == null)
                    return;

                SetupNLog(args.NLogLogLevel);

                Logger.Trace("Command {command} for file [{filepath}]", args.Command, args.ChangeLogFile.FullName);

                Construct(args);
                ConstructCommands(args);

                Process(args);
            }
            catch (CmdException ex)
            {
                Logger.Fatal(ex.Message);
            }
            catch (ProcessException ex)
            {
                Logger.Fatal(ex.Message);
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex);
            }
        }


        static CommandLineArgs PrepareArgs(string[] argsArray)
        {
            Res<CommandLineArgs> r;
            var p = new CmdArgsParser<CommandLineArgs>();
            p.AllowUnknownArguments = false;
            p.AllowAdditionalArguments = true;
            r = p.ParseCommandLine(argsArray);

            return r.Args;
        }


        static void SetupNLog(NLog.LogLevel logLevel)
        {
            var config = new LoggingConfiguration();

            var consoleTarget = new ColoredConsoleTarget("console")
            {
                Layout = @"${level}: ${message} ${exception}"
            };
            config.AddTarget(consoleTarget);

            config.AddRule(logLevel, NLog.LogLevel.Fatal, consoleTarget);
            LogManager.Configuration = config;
        }


        private static void Process(CommandLineArgs args)
        {
            ICommand command = _commandFabric.GetCommand(args);
            if (command == null)
                throw new ProcessException($"Command {args.Command} is not supported");

            IGatherer[] gatherers = ConstuctGatherers(_fileReader, _commonGatherer, 
                command.CreateFilter(args.Contexts, emptyContextAllowed: !args.ExcludeEmptyContext));
            _gathererFabric.Replace(gatherers);

            command.Execute(args);
        }


        static FileReader _fileReader;
        static GathererFabric _gathererFabric;
        static CommonGatherer _commonGatherer;
        static CommandFabric _commandFabric;


        static void Construct(CommandLineArgs args)
        {
            _fileReader = new FileReader();
            _gathererFabric = new GathererFabric();
            _commonGatherer = new CommonGatherer(_fileReader, _gathererFabric);
        }


        static void ConstructCommands(CommandLineArgs args)
        {
            // todo вынести сбор и создание команд - чтобы плагинами можно было расширять

            _commandFabric = new CommandFabric(
                new UpdateCommand(_commonGatherer, _fileReader),
                new WholeListCommand(_commonGatherer, _fileReader),
                new WholeListSqlCommand(_commonGatherer, _fileReader),
                new AllContextsCommand(_commonGatherer, _fileReader)
            );
        }


        static IGatherer[] ConstuctGatherers(FileReader fileReader, CommonGatherer commonGatherer, Filter filter)
        {
            XmlGatherer xmlGatherer = new XmlGatherer(fileReader, commonGatherer, filter);
            var gatherers = new IGatherer[]
            {
                new SqlGatherer(filter),
                new SqlRawGatherer(filter),
                xmlGatherer,
            };
            xmlGatherer.Init(gatherers);

            return gatherers;
        }
    }
}

/*
 call core\liquibase 

--defaultsFile="core/liquibase_CIS-DB.CIS_custom.properties" ^
--labels="bus_data OR bus_longtime_data" ^
--contexts="Light,prod, sync_prod,sync" ^

update ^


changeLogFile=.\\CIS\\#db.changelog-master_custom.xml

url=jdbc:sqlserver://CIS-DB;databaseName=CIS;integratedSecurity=true;applicationName=liquibase;
#username=liquibaseuser
#password=gfhjkm
promptForNonLocalDatabase=true

logLevel=info

rem liquibase commands: http://www.liquibase.org/documentation/command_line.html
update, updateSQL, changelogSync, changelogSyncSQL, status --verbose, releaseLocks, unexpectedChangeSets --verbose

                 */
