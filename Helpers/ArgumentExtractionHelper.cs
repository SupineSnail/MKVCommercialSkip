using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulletfish.MKVCommercialSkip.Helpers
{
    public enum Argument
    {
        None,
        File,
        XmlFile,
        Log
    }

    public static class ArgumentExtractionHelper
    {
        public static Dictionary<Argument, string> ExtractArguments (string[] argv)
        {
            var returnDict = new Dictionary<Argument, string>();

            if (argv == null || argv.Length == 0)
                return returnDict;

            var lastArgument = string.Empty;
            var lastArgEnum = Argument.None;
            var expectingParam = false;
            foreach (var arg in argv)
            {
                if (expectingParam && arg.Trim().StartsWith("-"))
                    throw new InvalidOperationException($"Extracting args expecting param for last argument '{lastArgument}'. Instead got next argument '{arg}'");

                if (expectingParam)
                {
                    returnDict[lastArgEnum] = arg;
                    expectingParam = false;
                }
                else
                {
                    switch (arg.Substring(1))
                    {
                        case "xml":
                            expectingParam = true;
                            lastArgEnum = Argument.XmlFile;
                            break;
                        case "file":
                        case "f":
                            expectingParam = true;
                            lastArgEnum = Argument.File;
                            break;
                        case "log":
                        case "l":
                            expectingParam = false;
                            returnDict[Argument.Log] = string.Empty;
                            break;
                        default:
                            throw new InvalidOperationException($"Unknown argument passed in '{arg}'");
                    }

                    lastArgument = arg;
                }
            }

            if (expectingParam)
                throw new InvalidOperationException($"Extracting args expecting param for last argument {lastArgument}");

            return returnDict;
        }
    }
}
