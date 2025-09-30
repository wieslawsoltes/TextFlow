# TextFlow Editor Roadmap

This roadmap captures the current state of the project and outlines the next set of milestones for the TextFlow rich text editor for Avalonia.

## Current Progress

- **Core library (`TextFlow.Editor`)**
  - Span-based `RichTextDocument` with styling APIs and undo/redo snapshots.
  - `RichTextPresenter` rendering engine using `TextLayout` with caret/selection visuals.
  - `RichTextBox` control supporting keyboard navigation, clipboard, formatting shortcuts, and programmatic styling hooks.
- **Sample application (`TextFlow.Editor.Sample`)**
   - Word-style toolbar for undo/redo, clipboard, bold/italic/underline, font family/size, foreground colours, paragraph alignment, indentation, and bullet/numbered lists.
  - Pre-populated demo document showcasing styling features.
- **Quality**
  - Automated unit tests for the document and control behaviours (`TextFlow.Editor.Tests`).
  - CI-ready commands (`dotnet test`, `dotnet run`) verified locally.

## Near-Term Goals (Sprint-Ready)

1. **Paragraph & Layout Formatting**
   - Introduce a block/inline model (FlowDocument-style API) that supports paragraphs, alignment, indentation, and lists.
   - Bridge the new model with the existing span engine and presenter.
2. **Persistence & Interop**
   - Add save/load flows in the sample app (JSON for rich text, plain text export).
   - Provide converters for HTML/Markdown snippets.
3. **Toolbar Enhancements**
   - Background colour picker and paragraph spacing/line height controls.
   - Keyboard shortcuts wired for every toolbar action.
4. **Testing & Tooling**
   - Extend unit and integration tests to cover formatting APIs and toolbar-driven scenarios.
   - Add benchmarking hooks for large documents.

## Stretch Initiatives

- **Collaboration** – investigate real-time editing via operational transforms or CRDTs.
- **Spell Checking** – integrate dictionaries and inline suggestions.
- **Find/Replace Pane** – provide model operations and sample UX.
- **Accessibility** – ensure screen-reader support and high-contrast themes.

## Tracking Notes

- Review this roadmap at the start of each sprint and move completed items into the progress section.
- Record architectural decisions (ADR-style) when introducing major features like the FlowDocument API or persistence formats.
- Keep the sample app aligned with new capabilities so it remains a live showcase.
