using System;
using System.Globalization;
using System.IO;
using System.Linq;
using CmdArgs;

namespace FluiDBase
{
    public class CommandLineArgs : ICheckAndPrepare<CommandLineArgs>
    {
        [ValuedArgument("logLevel", Description = "Used if --verbose option is provided.", AllowedValues = new[] { "Trace", "Debug", "Info", "Warn", "Error" }, 
            DefaultValue = "Warn", UseDefWhenNoArg = true)]
        public string LogLevel;
        public NLog.LogLevel NLogLogLevel => NLog.LogLevel.FromString(this.LogLevel);

        [ValuedArgument("labels")] public string Labels;
        [ValuedArgument("contexts")] public string[] Contexts;


        [FileArgument("defaultsFile", MustExists = true)]
        public FileInfo DefaultsFile;


        [FileArgument("changeLogFile", MustExists = true, Mandatory = true)]
        public FileInfo ChangeLogFile;

        [UnstrictlyConfArgument('D', "DEFINE")]
        public UnstrictlyConf Define;


        [ValuedArgument('c', "ConnectionString")]
        public string ConnectionString;

        [ValuedArgument('s', "server")] public string SqlServer;
        [ValuedArgument('u', "username")] public string SqlUser;
        [ValuedArgument('p', "password")] public string SqlUserPassword;

        /// <summary>
        /// It is from AdditionalArguments[0]
        /// </summary>
        public string Command;


        public void CheckAndPrepare(Res<CommandLineArgs> parsed)
        {
            if (parsed.AdditionalArguments.Count == 0)
                throw new CmdException("Provide command name!");
            if (parsed.AdditionalArguments.Count > 1)
                throw new CmdException("Unsupported arguments: " + parsed.AdditionalArguments.Skip(1).Select(x => $"[{x}]"));

            Command = parsed.AdditionalArguments[0];
        }
    }
}