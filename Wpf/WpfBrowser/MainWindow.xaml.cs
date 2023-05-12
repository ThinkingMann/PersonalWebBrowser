using System.Windows;

namespace WpfBrowser;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window {
    public MainWindow() {
        InitializeComponent();
    }

    private void abbWindowMinimize_Click( object sender, RoutedEventArgs e ) {
        this.WindowState = WindowState.Minimized;
    }

    private void abbWindowMaximize_Click( object sender, RoutedEventArgs e ) {
        this.WindowState = WindowState.Maximized;
    }
}
