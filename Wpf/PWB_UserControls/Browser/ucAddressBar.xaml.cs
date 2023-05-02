using System;
using System.Windows;
using System.Windows.Controls;

using PWB_UserControls.Delegates;

namespace PWB_UserControls.Browser;

/// <summary>
/// Interaction logic for ucAddressBar.xaml
/// </summary>
public partial class ucAddressBar : UserControl {

    public event AddressChangedEventHandler? TargetAddressChanged;

    #region Dependency Properties (SecureImageSource, ReloadImageSource, TargetAddress)
    public string SecureImageSource {
        get => (string)GetValue( SecureImageSourceProperty );
        set {
            SetValue( SecureImageSourceProperty, value );
            btnSecure.ImageSource = SecureImageSource;
        }
    }

    // Using a DependencyProperty as the backing store for SecureImageSource.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty SecureImageSourceProperty =
                           DependencyProperty.Register("SecureImageSource", typeof(string), typeof(ucAddressBar), new PropertyMetadata(""));

    public string ReloadImageSource {
        get => (string)GetValue( ReloadImageSourceProperty );
        set {
            SetValue( ReloadImageSourceProperty, value );
            btnReload.ImageSource = ReloadImageSource;
        }
    }

    // Using a DependencyProperty as the backing store for ReloadImageSource.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ReloadImageSourceProperty =
                           DependencyProperty.Register("ReloadImageSource", typeof(string), typeof(ucAddressBar), new PropertyMetadata(""));

    public string TargetAddress {
        get => (string)GetValue( TargetAddressProperty );
        set {
            SetValue( TargetAddressProperty, value );
            var oa = tbAddress.Text;
            tbAddress.Text = value;
            TargetAddressChanged?.Invoke( this, new AddressChangedEventArgs() { OldAddress = oa, NewAddress = TargetAddress } );
        }
    }

    // Using a DependencyProperty as the backing store for TargetAddress.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty TargetAddressProperty =
                           DependencyProperty.Register("TargetAddress", typeof(string), typeof(ucAddressBar), new PropertyMetadata(""));
    #endregion

    #region Class ctor..
    public ucAddressBar() {
        InitializeComponent();

        this.Loaded += UcAddressBar_Loaded;
    }
    #endregion

    private void UcAddressBar_Loaded( object sender, RoutedEventArgs e ) {
        if (!String.IsNullOrEmpty( TargetAddress )) {
            tbAddress.Text = TargetAddress;
            TargetAddressChanged?.Invoke( this, new AddressChangedEventArgs() {
                OldAddress = string.Empty,
                NewAddress = TargetAddress
            } );
        }
    }

    #region tbAddress.KeyUp event handler method
    private void tbAddress_KeyUp( object sender, System.Windows.Input.KeyEventArgs e ) {
        if (e.Key == System.Windows.Input.Key.Enter) {
            var oa = TargetAddress;
            TargetAddress = tbAddress.Text;
            TargetAddressChanged?.Invoke( this, new AddressChangedEventArgs() {
                OldAddress = oa,
                NewAddress = TargetAddress
            } );
        }
    }
    #endregion
}
