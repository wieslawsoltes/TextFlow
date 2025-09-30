# TextFlow Document Roadmap

A living tracker for TextFlow.Document parity work.

## ✅ Delivered

- Initial `FlowDocument` object model with `Paragraph`, `Run`, and inline formatting (`Span`, `Bold`, `Italic`, `Underline`).
- `FlowDocumentView` renderer supporting paragraphs with nested inline styling.
- Sample application showcasing typography, inline spans, and live document mutations.
- Block collections generalized to support nested containers (`Section`).
- List infrastructure (`List`, `ListItem`, `ListItemCollection`, `ListMarkerStyle`) with rendering sample.
- Renderer support for `Section` and `List` blocks (markers, indentation, `StartIndex`).
- Table surface (`Table`, `TableColumn`, `TableRowGroup`, `TableRow`, `TableCell`) with renderer integration and sample parity matrix.
- Guardrails for list infrastructure with unit tests covering mutation invalidation.
- Anchored block support (`Figure`, `Floater`) with renderer integration and sample coverage.

## 🚧 In Progress / Next Up

## 🛣️ Upcoming Milestones

- **BlockUIContainer:** Allow embedding Avalonia controls when feasible.
- **Enhanced text features:** Line height inheritance, text trimming configuration, and advanced decorations.
- **Printing / serialization:** Export helpers (plain text, XAML fragment) to aid interoperability.
- **Sample enhancements:** Additional pages demonstrating lists, tables, figures, and interactive editing UI.

## 📌 Notes

- Roadmap items focus on WPF parity; scope may adjust as Avalonia-specific needs arise.
- Contributions and feedback welcome—update this file as milestones shift or complete.

## 📊 API Parity Checklist

Status legend: `[x]` implemented, `[ ]` planned/todo, `✗` not currently feasible.

### Foundations

| WPF API | Avalonia Equivalent | Status | Notes |
| --- | --- | --- | --- |
| `FlowDocument` | `TextFlow.Document.Documents.FlowDocument` | [x] | Root document with block collection and change notifications. |
| `TextElement` | `TextFlow.Document.Documents.TextElement` | [x] | Base styling properties mapped to Avalonia equivalents. |
| `Block` | `TextFlow.Document.Documents.Block` | [x] | Abstract base for block-level content. |
| `Inline` | `TextFlow.Document.Documents.Inline` | [x] | Abstract base for inline elements. |
| `BlockCollection` | `TextFlow.Document.Documents.BlockCollection` | [x] | Manages block ownership and invalidation. |
| `InlineCollection` | `TextFlow.Document.Documents.InlineCollection` | [x] | Maintains inline ownership and change propagation. |

### Block elements

| WPF API | Avalonia Equivalent | Status | Notes |
| --- | --- | --- | --- |
| `Paragraph` | `TextFlow.Document.Documents.Paragraph` | [x] | Supports inline children and invalidation wiring. |
| `Section` | `TextFlow.Document.Documents.Section` | [x] | Nested block containers implemented. |
| `List` | `TextFlow.Document.Documents.List` | [x] | Implements marker style, start index, and block children. |
| `ListItem` | `TextFlow.Document.Documents.ListItem` | [x] | Hosts paragraphs/blocks within lists. |
| `ListItemCollection` | `TextFlow.Document.Documents.ListItemCollection` | [x] | Handles ownership and change notification. |
| `ListMarkerStyle` | `TextFlow.Document.Documents.ListMarkerStyle` | [x] | Enum mirrors WPF marker options in scope. |
| `Table` | `TextFlow.Document.Documents.Table` | [x] | Core table surface with column/row groups. |
| `TableColumn` | `TextFlow.Document.Documents.TableColumn` | [x] | Column definition, width, and parent linking. |
| `TableColumnCollection` | `TextFlow.Document.Documents.TableColumnCollection` | [x] | Ownership enforcement with change notifications. |
| `TableRowGroup` | `TextFlow.Document.Documents.TableRowGroup` | [x] | Groups rows with host integration. |
| `TableRowGroupCollection` | `TextFlow.Document.Documents.TableRowGroupCollection` | [x] | Manages row-group ownership. |
| `TableRow` | `TextFlow.Document.Documents.TableRow` | [x] | Hosts cell collection and parent notifications. |
| `TableRowCollection` | `TextFlow.Document.Documents.TableRowCollection` | [x] | Handles parent binding/ownership. |
| `TableCell` | `TextFlow.Document.Documents.TableCell` | [x] | Supports block content within cells. |
| `TableCellCollection` | `TextFlow.Document.Documents.TableCellCollection` | [x] | Manages cell ownership and invalidation. |
| `BlockUIContainer` | _Not available_ | [ ] | Requires Avalonia control hosting inside documents. |
| `Figure` | `TextFlow.Document.Documents.Figure` | [x] | Anchored block support with configurable width, padding, and anchors. |
| `Floater` | `TextFlow.Document.Documents.Floater` | [x] | Anchored block aligned left/right/center with optional sizing. |

### Inline elements

| WPF API | Avalonia Equivalent | Status | Notes |
| --- | --- | --- | --- |
| `Run` | `TextFlow.Document.Documents.Run` | [x] | Text payload with change notification implemented. |
| `Span` | `TextFlow.Document.Documents.Span` | [x] | Inline container with child management. |
| `Bold` | `TextFlow.Document.Documents.Bold` | [x] | Applies weight via `Span` derivative. |
| `Italic` | `TextFlow.Document.Documents.Italic` | [x] | Applies style via `Span` derivative. |
| `Underline` | `TextFlow.Document.Documents.Underline` | [x] | Applies decoration via `Span` derivative. |
| `Hyperlink` | _Not available_ | [ ] | Requires navigation commands and input gesture support. |
| `LineBreak` | _Not available_ | [ ] | Needs inline element or run splitting strategy. |
| `InlineUIContainer` | _Not available_ | [ ] | Depends on embedding Avalonia controls inline. |

### Editing & services

| WPF API | Avalonia Equivalent | Status | Notes |
| --- | --- | --- | --- |
| `TextPointer` | _Not available_ | [ ] | Text container/addressing model not yet implemented. |
| `TextRange` | _Not available_ | [ ] | Requires pointer model and content serialization. |
| `TextSelection` | _Not available_ | [ ] | Depends on editor surface and pointer infrastructure. |
| `IDocumentPaginatorSource` | _Not available_ | [ ] | Printing/pagination support to be designed. |
| `DocumentPaginator` | _Not available_ | [ ] | Dependent on pagination pipeline and layout service. |
