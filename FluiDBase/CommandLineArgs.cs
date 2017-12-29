using CommandLineParser.Arguments;


namespace FluiDBase
{
    public class CommandLineArgs
    {
        [SwitchArgument('s', "show", true, Description = "Set whether show or not")]
        public bool show;

        private bool hide;
        [SwitchArgument('h', "hide", false, Description = "Set whether hid or not")]
        public bool Hide
        {
            get { return hide; }
            set { hide = value; }
        }

        [ValueArgument(typeof(decimal), 'v', "version", Description = "Set desired version")]
        public decimal version;

        [ValueArgument(typeof(string), 'l', "level", Description = "Set the level")]
        public string level;

        [BoundedValueArgument(typeof(int), 'o', "optimization", 
            MinValue = 0, MaxValue = 3, Description = "Level of optimization")]
        public int optimization;

        [EnumeratedValueArgument(typeof(string),'c', "color", AllowedValues = "red;green;blue")]
        public string color;
    }
}