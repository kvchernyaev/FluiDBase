using System;
using System.Linq;
using CmdArgs;

namespace FluiDBase
{
    class Program
    {
        static void Main(string[] args)
        {

            var p = new CmdArgsParser<CommandLineArgs>();
            p.AllowUnknownArguments = false;
            p.AllowAdditionalArguments = true;

            try
            {

                Res<CommandLineArgs> r = p.ParseCommandLine(args);

                Console.WriteLine("AdditionalArguments: " + string.Join(", ", r.AdditionalArguments));
                Console.WriteLine("UnknownArguments: " + string.Join("; ", r.UnknownArguments.Select(t => $"{t.Item1} {string.Join(",", t.Item2)}")));
                Console.WriteLine("loglevel " + r.Args.NLogLogLevel);

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

                // todo вынести дополнительную подготовку CommandLineArgs в сам этот класс - его вызвать в ParseCommandLine(), непонятно как там обрабатывать ошибки
                if (r.AdditionalArguments.Count == 0)
                {
                    Console.Error.WriteLine("Provide command name!");
                    return;
                }
                r.Args.Command = r.AdditionalArguments[0];
                if (r.AdditionalArguments.Count > 1)
                {
                    Console.Error.WriteLine("Unneccessary arguments: " + string.Join(", ", r.AdditionalArguments.Skip(1).Select(x => $"[{x}]")));
                    return;
                }

                Console.WriteLine("command: " + r.Args.Command);

                Console.WriteLine("context: " + string.Join(", ", r.Args.Contexts));


                Process(r);

            }
            catch (CmdException ex)
            {
                Console.Error.WriteLine(ex.Message);
            }

        }


        private static void Process(Res<CommandLineArgs> r)
        {
            // todo вынести сбор и создание команд - чтобы плагинами можно было расширять
            var commandFabric = new CommandFabric(new UpdateCommand());
            ICommand command = commandFabric.GetCommand(r.Args);
            if (command == null)
                throw new ProcessException($"Command {r.Args.Command} is unsupported");

            command.Execute(r.Args);
        }
    }
}
