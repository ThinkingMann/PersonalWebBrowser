using System.Windows;
using System.Windows.Controls;

namespace PWB_CCLibrary.UserControls;

/// <summary>
/// Interaction logic for ucWindowButtons.xaml
/// </summary>
public partial class ucWindowButtons : UserControl {
    public ucWindowButtons() {
        InitializeComponent();
    }

    private void btnClose_Click( object sender, RoutedEventArgs e ) {
        Window.GetWindow( this ).Close();
    }

    private void btnMaximize_Click( object sender, RoutedEventArgs e ) {
        Window.GetWindow( this ).WindowState = WindowState.Maximized;
    }

    private void btnMinimize_Click( object sender, RoutedEventArgs e ) {
        Window.GetWindow( this ).WindowState = WindowState.Minimized;
    }
}
