using System;
using System.Collections.Generic;
using CommandLineParser.Arguments;
using CommandLineParser.Exceptions;


namespace FluiDBase
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            // https://github.com/j-maly/CommandLineParser
            CommandLineParser.CommandLineParser parser = 
                new CommandLineParser.CommandLineParser();
            
            
            SwitchArgument showArgument = new SwitchArgument(
                's', "show", "Set whether show or not", true);
            ValueArgument<decimal> version = new ValueArgument<decimal> (
                'e', "version", "Set desired version");
            EnumeratedValueArgument<string> color = new EnumeratedValueArgument<string> (
                'c', "color", new string[] { "red", "green", "blue" });

            parser.Arguments.Add(showArgument);
            parser.Arguments.Add(version);
            parser.Arguments.Add(color);
            
            try 
            {
                parser.ParseCommandLine(args); 
                parser.ShowParsedArguments();
            }
            catch (CommandLineException e)
            {
                Console.WriteLine("CommandLineException");
                Console.WriteLine(e.Message);
            }
        }
    }
}