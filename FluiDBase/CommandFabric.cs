using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluiDBase
{
    public class CommandFabric
    {
        readonly List<ICommand> Commands = new List<ICommand>();
        readonly IEqualityComparer<string> nameComparer = StringComparer.InvariantCultureIgnoreCase;

        public CommandFabric(IEnumerable<ICommand> commands)
        {
            Commands.AddRange(commands);
            Test();
        }


        public CommandFabric(params ICommand[] commands)
        {
            Commands.AddRange(commands);
            Test();
        }


        void Test()
        {
            IEnumerable<IGrouping<string, ICommand>> g = this.Commands.GroupBy(c => c.Name, nameComparer);
            IGrouping<string, ICommand> duplicated = g.FirstOrDefault(x => x.Count() > 1);
            if (duplicated != null)
                throw new ProcessException($"Command {duplicated.Key} is duplicated ({string.Join(", ", duplicated.Select(x => x.GetType().FullName))})");
        }


        public ICommand GetCommand(CommandLineArgs args)
        {
            ICommand command = this.Commands.FirstOrDefault(c => nameComparer.Equals(c.Name, args.Command));
            return command;
        }

    }
}
