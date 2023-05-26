using System.Windows;
using System.Windows.Controls;

namespace PWB_CCLibrary.UserControls;

/// <summary>
/// Interaction logic for ucWindowButtons.xaml
/// </summary>
public partial class ucWindowButtons : UserControl {
    Window? owner;
    public ucWindowButtons() {
        InitializeComponent();
        Loaded += UcWindowButtons_Loaded;
    }

    private void UcWindowButtons_Loaded( object sender, RoutedEventArgs e ) {
        owner = Window.GetWindow( this );
    }

    private void btnClose_Click( object sender, RoutedEventArgs e ) {
        owner!.Close();
    }

    private void btnMaximize_Click( object sender, RoutedEventArgs e ) {
        var ws = owner.WindowState;
        owner!.WindowState = ws == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    private void btnMinimize_Click( object sender, RoutedEventArgs e ) {
        owner!.WindowState = WindowState.Minimized;
    }
}
