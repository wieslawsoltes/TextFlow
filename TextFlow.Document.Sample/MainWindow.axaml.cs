using Avalonia.Controls;
using TextFlow.Document.Sample.ViewModels;

namespace TextFlow.Document.Sample;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }
}