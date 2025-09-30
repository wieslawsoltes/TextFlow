using System;
using System.Text;

namespace TextFlow.Document.Documents;

/// <summary>
/// Represents a contiguous run of text.
/// </summary>
public class Run : Inline
{
    private string _text;

    public Run()
    {
        _text = string.Empty;
    }

    public Run(string text)
    {
        _text = text ?? string.Empty;
    }

    public string Text
    {
        get => _text;
        set
        {
            var normalised = value ?? string.Empty;
            if (string.Equals(_text, normalised, StringComparison.Ordinal))
            {
                return;
            }

            _text = normalised;
            RaiseContentInvalidated();
        }
    }

    internal override void AppendPlainText(StringBuilder builder)
    {
        builder.Append(_text);
    }

    public override string ToString()
    {
        return _text;
    }
}
