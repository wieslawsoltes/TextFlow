# TextFlow

TextFlow packages a FlowDocument renderer and authoring surface for Avalonia. The solution now combines the original FlowDocument port with the rich text editor controls under a single roof so both projects can evolve together.

## Projects

- `TextFlow.Document` – miniature FlowDocument object model (`FlowDocument`, `Paragraph`, `Run`, inline spans, tables, figures) plus the `FlowDocumentView` renderer.
- `TextFlow.Document.Tests` – unit coverage for the document model (block/inline ownership, tables, anchored blocks, mutation notifications).
- `TextFlow.Document.Sample` – Avalonia desktop sample that previews multiple document presets and live updates using `FlowDocumentView`.
- `TextFlow.Document.Wpf.Sample` – WPF comparison app that mirrors the sample content (builds cross-platform with `EnableWindowsTargeting=true`, runs on Windows).
- `TextFlow.Editor` – Avalonia rich text editor control (`RichTextBox`) backed by the TextFlow document model.
- `TextFlow.Editor.Tests` – unit tests for editor document behaviours and control integration.
- `TextFlow.Editor.Sample` – desktop showcase for the editor with typography themes, indentation helpers, and live styling tweaks.

## Building and running

```bash
cd FlowDocument
# Compile everything (Document, Editor, samples, tests)
dotnet build FlowDocument.sln

# Launch the document preview sample
dotnet run --project TextFlow.Document.Sample

# Launch the editor showcase
dotnet run --project TextFlow.Editor.Sample

# Execute the test suites
dotnet test FlowDocument.sln
```

The WPF comparison sample (`TextFlow.Document.Wpf.Sample`) targets `net9.0-windows`. Building on non-Windows machines is supported, but running the executable still requires Windows.

## Contributing

Issues, roadmap updates, and pull requests are welcome. The solution keeps the document renderer and editor side-by-side so improvements in one project can immediately benefit the other.
