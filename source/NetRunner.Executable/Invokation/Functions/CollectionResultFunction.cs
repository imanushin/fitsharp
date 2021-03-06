﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using NetRunner.Executable.Common;
using NetRunner.Executable.Invokation.Documentation;
using NetRunner.Executable.Invokation.Remoting;
using NetRunner.Executable.RawData;
using NetRunner.ExternalLibrary.Properties;
using NetRunner.TestExecutionProxy;

namespace NetRunner.Executable.Invokation.Functions
{
    internal sealed class CollectionResultFunction : BaseComplexArgumentedFunction
    {
        public CollectionResultFunction(
            HtmlRow columnsRow,
            IEnumerable<HtmlRow> rows,
            FunctionHeader function,
            TestFunctionReference functionToExecute) :
            base(columnsRow, rows, function, functionToExecute)
        {
        }

        protected override FunctionExecutionResult ProcessResult(GeneralIsolatedReference mainFunctionResult)
        {
            var status = new SequenceExecutionStatus();

            var collectionResult = mainFunctionResult.AsIEnumerable();

            collectionResult = collectionResult.IsNull ?
                ReflectionLoader.CreateOnTestDomain((IEnumerable)ReadOnlyList<object>.Empty) :
                collectionResult;

            var orderedResult = collectionResult.ToArray();

            if (orderedResult.Any())
            {
                AddPropertiesHelp(orderedResult[0], status);
            }

            for (int rowIndex = 0; rowIndex < orderedResult.Length && rowIndex < Rows.Count; rowIndex++)
            {
                var resultObject = orderedResult[rowIndex];
                var type = resultObject.GetType();
                var typeData = type.GetData();

                var currentRow = Rows[rowIndex];

                var properties = ColumnProperties(typeData);

                if (properties.Any(p => p == null))
                {
                    MarkMissingProperties(currentRow, properties, status, type);

                    continue;
                }

                ProcessSetProperties(properties, currentRow, status, resultObject);
                ProcessGetProperties(properties, currentRow, status, resultObject);
            }

            for (int rowIndex = orderedResult.Length; rowIndex < Rows.Count; rowIndex++)
            {
                var currentRow = Rows[rowIndex];

                status.Changes.Add(new MarkRowAsMissing(currentRow.RowReference));

                status.AllIsOk = false;
            }

            for (int rowIndex = Rows.Count; rowIndex < orderedResult.Length; rowIndex++)
            {
                var resultObject = orderedResult[rowIndex];

                var cells = CleanedColumnNames.Select(name => ReadProperty(name, resultObject) + "<br/> <i class=\"code\">surplus</i>").ToReadOnlyList();

                status.Changes.Add(new AppendRowWithCells(HtmlParser.FailCssClass, cells));

                status.AllIsOk = false;
            }

            MarkRootFunction(status, Function.RowReference);

            return FormatResult(status);
        }

        private ReadOnlyList<PropertyReference> ColumnProperties(TypeData typeData)
        {
            return CleanedColumnNames.Select(typeData.GetProperty).ToReadOnlyList();
        }

        private void AddPropertiesHelp(GeneralIsolatedReference firstRow, SequenceExecutionStatus status)
        {
            var type = firstRow.GetType();

            var newChanges =
                ColumnProperties(type.GetData())
                .Select((p, i) => new
            {
                Property = p,
                Index = i
            }).Where(e => e.Property != null)
            .Select(e => new AddCellPropertyHelp(ColumnsRow.Cells[e.Index], e.Property.GetData())).ToReadOnlyList();

            status.Changes.AddRange(newChanges);
        }

        private static void ProcessGetProperties(ReadOnlyList<PropertyReference> properties, HtmlRow currentRow, SequenceExecutionStatus tableChanges, GeneralIsolatedReference resultObject)
        {
            for (int i = 0; i < properties.Count; i++)
            {
                var property = properties[i];

                if (!property.GetData().HasGet)
                {
                    continue;
                }

                var cellInfo = new CellParsingInfo(
                    currentRow.Cells[i],
                    property.GetData().PropertyType,
                    property.GetData().ArgumentPrepareMode,
                    property.GetData().TrimInputCharacters);

                var value = ParametersConverter.ConvertParameter(cellInfo, "Unable to parse value");

                tableChanges.MergeWith(value.Changes);

                if (!value.Changes.AllWasOk)
                {
                    continue;
                }

                var getValueResult = property.GetValue(resultObject);

                tableChanges.MergeWith(getValueResult, currentRow.Cells[i], "Unable to get value");

                if (getValueResult.HasError)
                {
                    continue;
                }

                var conversionSucceeded = tableChanges.CompareAndMerge(value.Result, getValueResult.Result, currentRow.Cells[i]);

                var cellChange = conversionSucceeded
                     ? new CssClassCellChange(currentRow.Cells[i], HtmlParser.PassCssClass)
                     : new ShowActualValueCellChange(currentRow.Cells[i], getValueResult.Result.ToString());

                tableChanges.Changes.Add(cellChange);
                tableChanges.AllIsOk &= conversionSucceeded;
            }
        }

        private static void ProcessSetProperties(ReadOnlyList<PropertyReference> properties, HtmlRow currentRow, SequenceExecutionStatus tableChanges, GeneralIsolatedReference resultObject)
        {
            for (int i = 0; i < properties.Count; i++)
            {
                var property = properties[i];

                if (property.GetData().HasGet)
                {
                    continue;
                }

                var currentCell = currentRow.Cells[i];

                var cellInfo = new CellParsingInfo(
                    currentCell,
                    property.GetData().PropertyType,
                    property.GetData().ArgumentPrepareMode,
                    property.GetData().TrimInputCharacters);

                var value = ParametersConverter.ConvertParameter(cellInfo, "Unable to parse value");

                tableChanges.MergeWith(value.Changes);

                if (!value.Changes.AllWasOk)
                {
                    continue;
                }

                var setValueResult = property.SetValue(resultObject, value.Result);

                tableChanges.MergeWith(setValueResult, currentCell, "Unable to set value");
            }
        }

        private void MarkMissingProperties(HtmlRow currentRow, ReadOnlyList<PropertyReference> properties, SequenceExecutionStatus status, TypeReference returnType)
        {
            status.AllIsOk = false;
            status.WereExceptions = true;

            for (int i = 0; i < properties.Count; i++)
            {
                if (properties[i] != null)
                {
                    continue;
                }

                var targetCell = currentRow.Cells[i];
                var propertyName = CleanedColumnNames[i];

                const string propertyNotFoundFormat = "Type '{0}' does not contain property '{1}'. Available properties: {2}";
                string header = string.Format("Property {0} was not found", propertyName);
                string info = string.Format(propertyNotFoundFormat, returnType, propertyName, string.Join(", ", returnType.GetData().Properties.Select(p => p.GetData().Name)));

                status.Changes.Add(new AddCellExpandableInfo(targetCell, header, info));
                status.Changes.Add(new CssClassCellChange(targetCell, HtmlParser.ErrorCssClass));
            }
        }

        private static string ReadProperty(string propertyName, GeneralIsolatedReference resultObject)
        {
            var type = resultObject.GetType();
            var typeData = type.GetData();

            var property = typeData.GetProperty(propertyName);

            if (property == null)
            {
                return string.Format("Unable to find property '{0}'", propertyName);
            }

            if (!property.GetData().HasGet)
            {
                return string.Empty;
            }

            var propertyReadValue = property.GetValue(resultObject);

            if (propertyReadValue.HasError)
            {
                return string.Format("Unable to read property '{0}' because of: {1}", propertyName, propertyReadValue.ExceptionToString);
            }

            var resultValue = propertyReadValue.Result;

            return (resultValue.IsNull ? string.Empty : resultValue.ToString()).ToString();
        }
    }
}
