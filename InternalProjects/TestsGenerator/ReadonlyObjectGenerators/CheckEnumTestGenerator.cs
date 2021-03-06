﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TestsGenerator.ReadonlyObjectGenerators
{
    internal static class CheckEnumTestGenerator
    {
        private const string checkWrongEnumConstructorTestTemplate =
@"
        [TestMethod]
        public void {2}_CheckWrongEnumArg_{0}{4}()
        {{
{1}

            try
            {{
                new {2}({3});
            }}
            catch(ArgumentException ex)
            {{
                CheckArgumentExceptionParameter( ""{0}"", ex.ParamName );

                return;
            }}

            Assert.Fail(""Enumeration in the argument '{0}' isn't checked for the not-defined values"");
        }}
";


        internal static string CreateWrongEnumConstructor(bool skipOverloading, ParameterInfo param, string argumentsList, string initialization)
        {
            Type parameterType = param.ParameterType;

            int wrongValue = Enum.GetValues(parameterType).Cast<int>().Max() + 1;

            const string replacementTemplate = "({0}){1}";

            string replacement = string.Format(replacementTemplate, EnumTestsGenerator.GetEnumName(parameterType), wrongValue);

            Type targetType = param.Member.DeclaringType;

            string currentArgs = argumentsList.Replace(param.Name, replacement);

            var methodSuffix = string.Empty;

            if (!skipOverloading)
            {
                methodSuffix = "_" + ReadonlyObjectTestGenerator.GetIndexerValue();
            }

            return string.Format(checkWrongEnumConstructorTestTemplate, param.Name, initialization, targetType.Name, currentArgs, methodSuffix);
        }

    }
}
