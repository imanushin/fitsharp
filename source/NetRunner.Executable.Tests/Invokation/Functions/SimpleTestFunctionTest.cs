
using HtmlAgilityPack;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetRunner.Executable.Common;
using NetRunner.Executable.Invokation;
using NetRunner.Executable.Invokation.Functions;
using NetRunner.Executable.Invokation.Keywords;
using NetRunner.Executable.RawData;
using NetRunner.Executable.Tests;
using NetRunner.Executable.Tests.Invokation;
using NetRunner.Executable.Tests.Invokation.Functions;
using NetRunner.Executable.Tests.Invokation.Keywords;
using NetRunner.Executable.Tests.RawData;
using NetRunner.ExternalLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NetRunner.Executable.Tests.Invokation.Functions
{
    partial class SimpleTestFunctionTest
    {
        private static IEnumerable<SimpleTestFunction> GetInstancesOfCurrentType()
        {
            foreach (var functionHeader in FunctionHeaderTest.objects.Objects)
            {
                foreach (TestFunctionReference testFunctionReference in TestFunctionReferenceTest.objects.Objects)
                {
                    foreach (HtmlRow row in HtmlRowTest.objects.Objects)
                    {
                        yield return new SimpleTestFunction(functionHeader, testFunctionReference, row);
                    }
                }
            }
        }
    }
}

