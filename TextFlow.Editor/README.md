# TextFlow Editor for Avalonia

The TextFlow.Editor project complements the TextFlow document renderer with a reusable rich text editing control for Avalonia applications. It ships alongside dedicated tests and a desktop showcase so the control can evolve with confidence.

## Projects

- `TextFlow.Editor` – the rich text editor control (`RichTextBox`) implemented on top of the TextFlow document model.
- `TextFlow.Editor.Tests` – xUnit coverage for document behaviours and control-level interactions.
- `TextFlow.Editor.Sample` – an Avalonia desktop application that demonstrates styling, theming, indentation helpers, and snapshot inspection.

## Build & test

```bash
# From the repository root
dotnet test FlowDocument.sln
```

## Run the sample

```bash
# From the repository root
dotnet run --project FlowDocument/TextFlow.Editor.Sample
```

The sample window loads with pre-populated rich text demonstrating styled spans. A Word-style toolbar lets you toggle bold/italic/underline, switch fonts and sizes, change colours, align paragraphs, adjust indentation, manage bullet/numbered lists, and access undo/redo plus clipboard shortcuts.
