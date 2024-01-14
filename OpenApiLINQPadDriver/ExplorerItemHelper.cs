using System;
using System.Collections.Generic;
using LINQPad.Extensibility.DataContext;

namespace OpenApiLINQPadDriver;
internal static class ExplorerItemHelper
{
    public static ExplorerItem CreateForTimeMeasurement()
        => new("Execution Times", ExplorerItemKind.Schema, ExplorerIcon.Box)
        {
            Children = []
        };

    public static ExplorerItem CreateForCompilationErrors(IReadOnlyCollection<string> errors)
        => CreateForCompilation(errors, "Compile errors");

    public static ExplorerItem CreateForCompilationWarnings(IReadOnlyCollection<string> warnings)
        => CreateForCompilation(warnings, "Compile warnings");

    private static ExplorerItem CreateForCompilation(IReadOnlyCollection<string> messages, string text)
    {
        var errorExplorerItem = new ExplorerItem(text, ExplorerItemKind.Schema, ExplorerIcon.LinkedDatabase)
        {
            Children = new List<ExplorerItem>(messages.Count)
        };

        foreach (var message in messages)
        {
            errorExplorerItem.Children.Add(new ExplorerItem(message, ExplorerItemKind.Schema, ExplorerIcon.Inherited)
            {
                DragText = message,
                ToolTipText = message
            });
        }

        return errorExplorerItem;
    }

    public static ExplorerItem CreateForElapsedTime(string name, TimeSpan elapsed)
        => new(name + " " + elapsed, ExplorerItemKind.Property, ExplorerIcon.Blank);

    public static List<ExplorerItem> CreateForGeneratedCode(string codeGeneratedByNSwag, string clientSourceCode)
        =>
        [
            new("NSwag generated source code", ExplorerItemKind.Schema, ExplorerIcon.Schema)
            {
                ToolTipText = "Drag and drop context generated source code to text window",
                DragText = codeGeneratedByNSwag
            },
            new("Client source code", ExplorerItemKind.Schema, ExplorerIcon.Schema)
            {
                ToolTipText = "Drag and drop context source code to text window client",
                DragText = clientSourceCode
            }
        ];
}
