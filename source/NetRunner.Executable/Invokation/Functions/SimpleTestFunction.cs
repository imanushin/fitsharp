﻿using System;
using System.Collections.Generic;
using System.Linq;
using NetRunner.Executable.Common;

namespace NetRunner.Executable.Invokation.Functions
{
    internal sealed class SimpleTestFunction : AbstractTestFunction
    {
        public SimpleTestFunction(FunctionHeader header, TestFunctionReference functionToExecute)
        {
            Validate.ArgumentIsNotNull(header, "header");

            Function = header;
            FunctionReference = functionToExecute;
        }

        public TestFunctionReference FunctionReference
        {
            get;
            private set;
        }

        public FunctionHeader Function
        {
            get;
            private set;
        }

        protected override IEnumerable<object> GetInnerObjects()
        {
            yield return Function;
        }

        public override FunctionExecutionResult Invoke(ReflectionLoader loader)
        {
            try
            {
                var result = InvokeFunction(loader, FunctionReference, Function);

                if (Equals(false, result))
                    return new FunctionExecutionResult(FunctionExecutionResult.FunctionRunResult.Fail, string.Empty);

                return new FunctionExecutionResult(FunctionExecutionResult.FunctionRunResult.Success, string.Empty);
            }
            catch (Exception ex)
            {
                TestExecutionLog.Trace("Unable to execute function {0} because of error {1}", this, ex);

                return new FunctionExecutionResult(FunctionExecutionResult.FunctionRunResult.Exception, ex.ToString());
            }
        }

        protected override string GetString()
        {
            return GetType().Name + ": " + Function;
        }
    }
}
