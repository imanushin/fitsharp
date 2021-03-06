﻿using System;
using System.Globalization;
using System.Linq;
using HtmlAgilityPack;
using NetRunner.Executable.Common;
using NetRunner.Executable.RawData;
using NetRunner.ExternalLibrary.Properties;

namespace NetRunner.Executable.Invokation.Functions
{
    internal abstract class AbstractTableChange : BaseReadOnlyObject
    {
        public abstract void PatchHtmlTable(HtmlNode table);

        protected static void AddExpandableRow(HtmlNode table, HtmlRowReference previousRowReference, string header, string text, string nodeClass = null)
        {
            var document = table.OwnerDocument;

            var node = document.CreateElement(HtmlParser.TableRowNodeName);

            AddClassAttribute(nodeClass, node);

            var cellContainer = document.CreateElement(HtmlParser.TableCellNodeName);

            cellContainer.Attributes.Add("colspan", "999");

            AddExpandableDivToCell(header, text, cellContainer);

            node.AppendChild(cellContainer);

            var previousRow = previousRowReference.GetRow(table);

            var previousRowIndex = table.ChildNodes.GetNodeIndex(previousRow);

            table.ChildNodes.Insert(previousRowIndex + 1, node);
        }

        protected static void AddExpandableDivToCell(string header, string text, HtmlNode cellContainer)
        {
            //ToDo: check if such div has already existed

            var document = cellContainer.OwnerDocument;

            var expandableDiv = document.CreateElement("div");

            AddClassAttribute("collapsible closed", expandableDiv);

            var titleNode = document.CreateElement("p");
            titleNode.InnerHtml = header;
            AddClassAttribute("title", titleNode);
            expandableDiv.AppendChild(titleNode);

            var textNode = document.CreateElement("div");
            textNode.InnerHtml = text;
            expandableDiv.AppendChild(textNode);

            cellContainer.AppendChild(expandableDiv);
        }

        private static void AddClassAttribute(string nodeClass, HtmlNode node)
        {
            if (!string.IsNullOrWhiteSpace(nodeClass))
            {
                node.Attributes.Add(HtmlParser.ClassAttributeName, nodeClass);
            }
        }
    }
}
