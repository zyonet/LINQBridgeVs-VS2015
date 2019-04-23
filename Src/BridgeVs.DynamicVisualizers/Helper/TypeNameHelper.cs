﻿#region License
// Copyright (c) 2013 - 2018 Coding Adventures
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace BridgeVs.DynamicVisualizers.Helper
{
    internal static class TypeNameHelper
    {

        private static readonly List<string> SystemNamespaces = new List<string>
                                                                    {
                                                                        "System.Collections.Generic.",
                                                                        "System.Collections.",
                                                                        "System.Data.Common.",
                                                                        "System.Data.Linq.",
                                                                        "System.Data.",
                                                                        "System.ComponentModel.",
                                                                        "System.Configuration.",
                                                                        "System.Diagnostics.",
                                                                        "System.Linq.Expression.",
                                                                        "System.Linq.",
                                                                        "System.",

                                                                    };
        public static string RemoveSystemNamespaces(string input)
        {
            SystemNamespaces.ForEach(s => input = input.IndexOf(s, StringComparison.Ordinal) >= 0 ? input.Replace(s, string.Empty) : input);

            return input;
        }
        public static string GetDisplayName(Type type, bool fullName)
        {
            if (type == null)
            {
                return string.Empty;
            }

            if (type.IsGenericParameter)
            {
                return type.Name;
            }

            if (!type.IsGenericType && !type.IsArray)
            {
                return fullName ? type.FullName : type.Name;
            }

            string anonymousTypeName = GetAnonymousTypeName(type);
           
            // replace `2 with <type1, type2="">
            Regex regex = new Regex("`[0-9]+");
            GenericsMatchEvaluator evaluator = new GenericsMatchEvaluator(type.GetGenericArguments(), fullName);

            // Remove [[fullName1, ..., fullNameX]]
            string name = fullName ? type.FullName : type.Name;
            int start = name.IndexOf("[[", StringComparison.Ordinal);
            int end = name.LastIndexOf("]]", StringComparison.Ordinal);
            if (start > 0 && end > 0)
            {
                name = name.Substring(0, start) + name.Substring(end + 2);
            }
            string retName = regex.Replace(name, evaluator.Evaluate);
            
            return anonymousTypeName.Length != 0 ? retName.Replace(anonymousTypeName,"AnonymousType") : retName;
        }

        private static string GetAnonymousTypeName(Type @type)
        {
            if (type == null) return string.Empty;

            Type[] genericTypes = type.GetGenericArguments();

            foreach (Type genericType in genericTypes)
            {
                string  anonymousTypeName = GetAnonymousTypeName(genericType);
                if (anonymousTypeName.Length > 0) return anonymousTypeName;
            }

            //leaf
            bool hasCompilerGeneratedAttribute = type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Any();
            bool nameContainsAnonymousType = type.FullName.Contains("AnonymousType");
            bool isAnonymousType = hasCompilerGeneratedAttribute && nameContainsAnonymousType;

            if (!isAnonymousType) return string.Empty;
         
            bool hasMoreGenericParams = @type.GetGenericArguments().Length != 0;
            return hasMoreGenericParams ? @type.Name.Split('`')[0] : @type.Name;
        }

        private static bool IsAnonymousType(Type type)
        {
            if (type == null) return false;

            bool hasCompilerGeneratedAttribute = type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Any();
            bool nameContainsAnonymousType = type.FullName.Contains("AnonymousType");
            bool isAnonymousType = hasCompilerGeneratedAttribute && nameContainsAnonymousType;

            if (isAnonymousType) return true;

            Type[] genericTypes = type.GetGenericArguments();

            foreach (Type genericType in genericTypes)
            {
                isAnonymousType = IsAnonymousType(genericType);
                if (isAnonymousType) break;
            }

            return isAnonymousType;
        }
        class GenericsMatchEvaluator
        {
            readonly Type[] _generics;
            int _index;
            readonly bool _fullName;

            public GenericsMatchEvaluator(Type[] generics, bool fullName)
            {
                _generics = generics;
                _index = 0;
                _fullName = fullName;
            }

            public string Evaluate(Match match)
            {
                int numberOfParameters = int.Parse(match.Value.Substring(1), CultureInfo.InvariantCulture);

                StringBuilder sb = new StringBuilder();

                // matched "`N" is replaced by "<type1, ...,="" typen="">"
                sb.Append("<");

                for (int i = 0; i < numberOfParameters; i++)
                {
                    if (i > 0)
                    {
                        sb.Append(", ");
                    }

                    sb.Append(GetDisplayName(_generics[_index++], _fullName));
                }

                sb.Append(">");

                return sb.ToString();
            }
        }
    }
}