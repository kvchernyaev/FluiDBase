using System;
using System.Collections.Generic;
using System.Text;

namespace FluiDBase
{
    public class UpdateCommand : ICommand
    {
        public string Name => "update";

        public void Execute(CommandLineArgs args)
        {
            
        }
    }
}
