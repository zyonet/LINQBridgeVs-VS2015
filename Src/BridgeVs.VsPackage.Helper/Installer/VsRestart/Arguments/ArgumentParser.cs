﻿using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace BridgeVs.VsPackage.Helper.Installer.VsRestart.Arguments
{
    internal class ArgumentParser
    {
        private readonly string _arguments;

        public ArgumentParser(string arguments)
        {
            _arguments = arguments;
        }

        public ArgumentTokenCollection GetArguments()
        {
            ArgumentTokenCollection result = new ArgumentTokenCollection();
            foreach (IArgumentToken argument in GetArgumentTokens())
            {
                result.Add(argument);
            }

            return result;
        }

        private IEnumerable<IArgumentToken> GetArgumentTokens()
        {
            if (string.IsNullOrEmpty(_arguments))
                yield break;

            RegexOptions options = RegexOptions.None;
            Regex regex = new Regex("(?<match>[^\\s\"]+)|(?<match>\"[^\"]*\")", options);

            List<string> arguments = (regex.Matches(_arguments).Cast<Match>().Where(m => m.Groups["match"].Success).Select(m => m.Groups["match"].Value)).ToList();

            foreach (string argument in arguments)
            {
                if (argument.ToLower().Contains(".sln"))
                    yield return new SolutionArgumentToken(argument);
                else if (argument.ToLower().Contains(".*proj"))
                    yield return new ProjectArgumentToken(argument);
                else yield return new GenericArgumentToken(argument);
            }
        }
    }
}