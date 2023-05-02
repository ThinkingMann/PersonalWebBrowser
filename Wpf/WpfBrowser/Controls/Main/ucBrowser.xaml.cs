using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using PWB_UserControls.Delegates;

namespace WpfBrowser.Controls.Main;

/// <summary>
/// Interaction logic for ucBrowser.xaml
/// </summary>
public partial class ucBrowser : UserControl {

    #region Dependency Properties (Url)
    public string Url {
        get { return (string)GetValue( UrlProperty ); }
        set {
            if (value.Equals( Url )) return;
            SetValue( UrlProperty, value );
            abTargetAddress.TargetAddress = value;
        }
    }

    // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty UrlProperty =
                           DependencyProperty.Register("Url", typeof(string), typeof(ucBrowser), new PropertyMetadata(""));
    #endregion

    public ucBrowser() {
        InitializeComponent();
        this.Loaded += UcBrowser_Loaded;
    }

    private void UcBrowser_Loaded( object sender, RoutedEventArgs e ) {
        if (!string.IsNullOrEmpty( Url )) {
            cwbBrowser.Address = abTargetAddress.TargetAddress = Url;
        }
    }

    private void tblkAddress_KeyUp( object sender, KeyEventArgs e ) {
        if (e.Key == Key.Enter) {
            Url = cwbBrowser.Address = abTargetAddress.TargetAddress;
        }
    }

    private void cwbBrowser_Initialized( object sender, System.EventArgs e ) {
        if (Url is not null)
            abTargetAddress.TargetAddress = cwbBrowser.Address = Url;
    }

    private void abTargetAddress_TargetAddressChanged( object sender, AddressChangedEventArgs e ) {
        if (!string.IsNullOrWhiteSpace( abTargetAddress.TargetAddress ) && !Url.Equals( abTargetAddress.TargetAddress ))
            Url = cwbBrowser.Address = abTargetAddress.TargetAddress;
    }
}
