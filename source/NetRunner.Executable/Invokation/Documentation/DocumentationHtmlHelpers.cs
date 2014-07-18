﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetRunner.TestExecutionProxy;

namespace NetRunner.Executable.Invokation.Documentation
{
    internal static class DocumentationHtmlHelpers
    {
        public const string AttributeName = "helpid";

        private static readonly Dictionary<string, string> functionKeyMap = new Dictionary<string, string>();

        private static readonly StringBuilder resultHtmls = new StringBuilder();

        private static int indexer = 1;

        private static readonly object syncRoot = new object();
        private const string tooltipFormat = "<div class='tooltiptext' id='{0}'>{1}</div>";

        private const string header =
            @"<script src=""http://cdn.jsdelivr.net/qtip2/2.2.0/jquery.qtip.min.js"" type=""text/javascript""></script>
   <link rel=""stylesheet"" type=""text/css"" href=""http://cdn.jsdelivr.net/qtip2/2.2.0/jquery.qtip.min.css"" />

<style>
.tooltiptext{
    display: none;
}
</style>";

        private const string footer =
            @"
  <script>
// Create the tooltips only when document ready
$(document).ready(function()
{
     $('[helpId]').each(function() {
        var el = document.getElementById( $( this ).attr('helpid') );

        if( el === null )
            return true;
        
         $(this).qtip({
             content: {
                 text: el.innerHTML
             }
         });
     });
});
  </script>
";

        public static string GetAllTypesHintElements()
        {
            var types = DocumentationStore.GetAllTypesHelp();

            var result = new StringBuilder();

            foreach (var nameToText in types)
            {
                var rawText = nameToText.Value;

                result.AppendFormat(tooltipFormat, GetTypeId(nameToText.Key), rawText);
            }

            return result.ToString();
        }

        public static string GetHintAttributeValue(TestFunctionReference function)
        {
            var identity = function.Identity;

            lock (syncRoot)
            {
                string internalId;

                if (!functionKeyMap.TryGetValue(identity, out internalId))
                {
                    internalId = string.Format("function_{0}", Interlocked.Increment(ref indexer));

                    string documentation = DocumentationStore.GetFor(function);

                    if (!string.IsNullOrWhiteSpace(documentation))
                    {
                        resultHtmls.AppendFormat(tooltipFormat, internalId, documentation);
                    }
                }

                return internalId;
            }
        }

        public static string GetHintAttributeValue(TypeReference type)
        {
            return GetTypeId(type.FullName);
        }

        private static string GetTypeId(string typeFullName)
        {
            return string.Format("type_{0}", typeFullName.Replace('.', '_'));
        }

        public static string HtmlHeader
        {
            get
            {
                return header;
            }
        }

        public static string HtmlFooter
        {
            get
            {
                lock (syncRoot)
                {
                    resultHtmls.Append(footer);

                    var result = resultHtmls.ToString();

                    resultHtmls.Clear();

                    return result;
                }
            }
        }
    }
}
