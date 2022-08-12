using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestApiInstallerDomain
{
    public class CommandLineParser
    {
        public static Options OptionsData;
        public static string ResultMessage;

        public static void Read(string[] args)
        {
            var parser = new Parser(with => with.EnableDashDash = true);
            var result = Parser.Default.ParseArguments<Options>(args);
            result.WithParsed(options => { PopulateOptions(options); });

            if (result.Tag == ParserResultType.NotParsed)
            {
                result.WithNotParsed<Options>((errs) => CommandLineParser.HandleParserError(errs));
            }
        }

        private static void PopulateOptions(Options options)
        {
            OptionsData = options;
        }

        public static void HandleParserError(IEnumerable<Error> errors)
        {
            var sb = new StringBuilder();

            foreach (Error err in errors)
            {
                switch(err.Tag)
                {
                    case ErrorType.MissingRequiredOptionError:
                        sb.Append($" {err.Tag.ToString()} : {((CommandLine.MissingRequiredOptionError)err).NameInfo.NameText} ");
                        break;

                    case ErrorType.MissingValueOptionError:
                        sb.Append($" {err.Tag.ToString()} : {((CommandLine.MissingValueOptionError)err).NameInfo.NameText} ");
                        break;

                    case ErrorType.UnknownOptionError:
                        sb.Append($" {err.Tag.ToString()} : {((CommandLine.UnknownOptionError)err).Token} ");
                        break;
                }                
            }

            CommandLineParser.ResultMessage = sb.ToString();
        }
    }
}
