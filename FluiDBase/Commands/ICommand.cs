using System;
using System.Collections.Generic;
using System.Text;

namespace FluiDBase
{
    public interface ICommand
    {
        string Name { get; }
        Filter CreateFilter(string[] allowedContexts, bool emptyContextAllowed);
        void Execute(CommandLineArgs args);
    }
}
