using System.Windows;
using System.Windows.Documents;

namespace TextFlow.Document.Wpf.Sample
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Programmatic example: append a paragraph at runtime to demonstrate code usage
            var p = new Paragraph(new Run("Programmatically added paragraph: this shows how to create runs and paragraphs in code."));
            p.Margin = new Thickness(0, 12, 0, 0);

            // Find the FlowDocument by name and add the paragraph
            if (this.FindName("SampleDocument") is FlowDocument doc)
            {
                doc.Blocks.Add(p);
            }
        }
    }
}
