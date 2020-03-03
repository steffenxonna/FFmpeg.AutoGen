﻿// Copyright 2020 Craytive Technologies BV. All rights reserved. Company proprietary and confidential.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using FFmpeg.AutoGen.CppSharpUnsafeGenerator.Definitions;

namespace FFmpeg.AutoGen.CppSharpUnsafeGenerator
{
    internal class ExistingInlineFunctionsHelper
    {
        private static readonly Regex FunctionNameRegex = new Regex(
            @"\s+public static .+ (?<name>[\S]+)\(.*?\)", RegexOptions.Compiled | RegexOptions.Multiline);

        private static readonly Regex FunctionHashRegex = new Regex(
            @"\s+// original body hash: (?<hash>\S+)", RegexOptions.Compiled | RegexOptions.Multiline);

        public static InlineFunctionDefinition[] LoadInlineFunctions(string path)
        {
            if (!File.Exists(path)) return Array.Empty<InlineFunctionDefinition>();

            var text = File.ReadAllText(path);
            var functions = new List<InlineFunctionDefinition>();

            var nameMatches = FunctionNameRegex.Matches(text);
            var hashMatches = FunctionHashRegex.Matches(text);

            Debug.Assert(nameMatches.Count == hashMatches.Count);

            for (var i = 0; i < nameMatches.Count; i++)
            {
                var nameMatch = nameMatches[i];
                var hashMatch = hashMatches[i];

                var name = nameMatch.Groups["name"].Value;
                var hash = hashMatch.Groups["hash"].Value;
                var bodyIndex = nameMatch.Index + nameMatch.Length;
                var body = text.Substring(bodyIndex, hashMatch.Index - bodyIndex);

                var function = new InlineFunctionDefinition
                {
                    Name = name,
                    Body = body,
                    OriginalBodyHash = hash
                };
                functions.Add(function);
            }

            return functions.ToArray();
        }
    }
}