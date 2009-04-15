// FitNesse.NET
// Copyright � 2008 Syterra Software Inc. Includes work by Object Mentor, Inc., (c) 2002 Cunningham & Cunningham, Inc.
// This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License version 2.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.

using fitSharp.Fit.Model;
using fitSharp.Fit.Operators;
using fitSharp.Machine.Engine;
using fitSharp.Machine.Model;

namespace fit.Operators {
	public class ExecuteEmpty : ExecuteBase {
	    public override bool TryExecute(Processor<Cell> processor, ExecuteParameters parameters, ref TypedValue result) {
			if ((parameters.Verb != ExecuteParameters.Check && parameters.Verb != ExecuteParameters.Input)
                || parameters.Cell.Text.Length != 0
                || ((Parse)parameters.Cell).Parts!= null) return false;

	        switch (parameters.Verb) {
	            case ExecuteParameters.Input:
                    TypedValue actual = processor.TryInvoke(parameters.SystemUnderTest, parameters.GetMemberName(processor),
                             new TreeList<Cell>());
                    if (actual.IsValid) ShowActual(parameters, actual.Value);
	                break;
                case ExecuteParameters.Check:
			        ShowActual(parameters, parameters.GetActual(processor));
	                break;
	        }
	        return true;
	    }

	    private static void ShowActual(ExecuteParameters parameters, object actual) {
            parameters.Cell.AddToAttribute(
                CellAttributes.InformationSuffixKey,
                actual == null ? "null"
	            : actual.ToString().Length == 0 ? "blank"
	            : actual.ToString(),
                CellAttributes.SuffixFormat);
	    }
	}
}