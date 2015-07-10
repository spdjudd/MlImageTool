using Fclp;
using System;

namespace MlImageTool
{
    class CommandLineArgs
    {
        public Mode Mode { get; set; }
        public string TargetDirectory { get; set; }
        public string CategoriesFile { get; set; }
        public int SearchCount { get; set; }
        public int ImageSize { get; set; }
        public int TestNumber { get; set; }

        public CommandLineArgs()
        {
            SearchCount = 100;
            ImageSize = 50;
            TestNumber = 30;
        }

        public bool IsValid()
        {
            return Mode != Mode.Search || !string.IsNullOrEmpty(CategoriesFile);
        }

        public static CommandLineArgs Parse(string[] args)
        {
            var parser = new FluentCommandLineParser<CommandLineArgs>();
            parser.Setup(arg => arg.Mode)
                .As('m', "mode")
                .WithDescription("Mode {search|crop|flip|resize|split}")
                .Required();
            parser.Setup(arg => arg.TargetDirectory)
                .As('t', "target")
                .WithDescription("Target directory for images")
                .Required();
            parser.Setup(arg => arg.CategoriesFile)
                .As('c', "categories")
                .WithDescription("Path to the CSV file of categories and search terms - required for search mode");
            parser.Setup(arg => arg.SearchCount)
                .As('r', "results")
                .WithDescription("Number of search results per category - default 100");
            parser.Setup(arg => arg.ImageSize)
                .As('s', "size")
                .WithDescription("Image dimensions for resizing (square) - default 50");
            parser.Setup(arg => arg.TestNumber)
                .As('n', "testnumber")
                .WithDescription("Number of image groups to use for testing (vs training) - default 30");
            parser.SetupHelp("?", "help")
                .Callback(text => Console.WriteLine(text));
            var parseResult = parser.Parse(args);
            if (parseResult.HasErrors || !parser.Object.IsValid())
            {
                Console.WriteLine("Usage:");
                parser.HelpOption.ShowHelp(parser.Options);
                return null;
            }
            return parser.Object;
        }
    }

    enum Mode
    {
        Search,
        Crop,
        Flip,
        Resize,
        Split
    }
}
