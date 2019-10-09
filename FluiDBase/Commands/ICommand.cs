using System;
using System.Collections.Generic;
using System.Text;

namespace FluiDBase
{
    public interface ICommand
    {
        void Execute(CommandLineArgs args);
        string Name { get; }
    }
}
