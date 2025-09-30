using System.Runtime.CompilerServices;
using Avalonia.Metadata;

[assembly: InternalsVisibleTo("TextFlow.Editor")]
[assembly: InternalsVisibleTo("TextFlow.Document.Tests")]

// Map the Avalonia XAML XML namespace to this assembly's CLR namespaces so consumers
// can reference controls and document types using the Avalonia XAML namespace.
[assembly: XmlnsDefinition("https://github.com/avaloniaui", "TextFlow.Document.Controls")]
[assembly: XmlnsDefinition("https://github.com/avaloniaui", "TextFlow.Document.Documents")]

// Recommended prefix for the mapped Avalonia namespace when used in XAML (e.g. xmlns:fdc="https://github.com/avaloniaui").
[assembly: XmlnsPrefix("https://github.com/avaloniaui", "fdc")]
