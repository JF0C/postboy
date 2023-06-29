using Mono.Options;
using Const = Postboy.Constants.Constants;

namespace Postboy.Helpers
{
    public static class ArgsParserExtension
    {
        public static WebApplicationBuilder ParseArgs(this WebApplicationBuilder builder, string[] args)
        {
            List<KeyValuePair<string, string?>>? kvs = new List<KeyValuePair<string, string?>>();
            var options = new OptionSet
            {
                { $"u|{Const.UserAppDir}=", "The user's \"AppData\\Roaming\" directory", v => 
                kvs.Add(new KeyValuePair<string, string?>(Const.UserAppDir, v)) }
            };
            options.Parse(args);
            builder.Configuration.AddInMemoryCollection(kvs);
            var appdir = builder.Configuration[Const.UserAppDir];
            if (appdir == null)
            {
                Console.WriteLine($"using default AppData folder for appstate.json");
            }
            else
            {
                Console.WriteLine($"using folder \"{appdir}\" for appstate.json");
            }
            return builder;
        }
    }
}
