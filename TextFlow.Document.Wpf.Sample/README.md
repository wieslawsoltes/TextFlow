TextFlow.Document.Wpf.Sample
=======================

This is a small WPF sample project showing comprehensive usage of WPF's built-in FlowDocument features: paragraphs, runs, inline formatting, lists, tables, figures, and BlockUIContainer.

How to run
----------

1. Open the solution `FlowDocument.sln` in Visual Studio 2022/2023 or Rider that supports .NET 7/8/9 WPF projects.
2. Add the `TextFlow.Document.Wpf.Sample/TextFlow.Document.Wpf.Sample.csproj` project to the solution if it's not already included.
3. Set it as the startup project and run.

Notes on compatibility with `TextFlow.Document`
-------------------------------------------------

- This sample uses WPF's built-in `System.Windows.Documents.*` types. The repository also contains an Avalonia-based lightweight `FlowDocument` implementation under `TextFlow.Document`.
- You're encouraged to try copying content from this sample's XAML or code into code that uses the Avalonia `FlowDocument` types. Expect API differences:
  - Namespace: WPF uses `System.Windows.Documents`; Avalonia types live in `TextFlow.Document.Documents`.
  - Class names like `FlowDocument`, `Paragraph`, `Run`, `List`, `Table`, `Figure` are present in both, but some properties (e.g., PagePadding, FontFamily types, TextAlignment enum) may differ or be typed differently.
  - UI hosting: WPF uses `FlowDocumentScrollViewer` and `BlockUIContainer`. Avalonia has its own viewer/control (see `TextFlow.Document/Controls/FlowDocumentView.cs`).

Try it
------

- Copy a small paragraph or Table XAML into an Avalonia `FlowDocument` instance and adjust namespaces and property names.
- For programmatic compatibility, try copying `new Paragraph(new Run("..."))` style code but replace `System.Windows.Documents` types with `TextFlow.Document.Documents` equivalents and adapt property types.

If you want, I can add a second sample window that constructs an Avalonia-style document side-by-side for direct comparison. Tell me if you want that and whether to prefer XAML or code examples.
