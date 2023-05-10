using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

using PWB_CCLibrary.Delegates;

namespace WpfBrowser.Controls.Browser;
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
        }
    }

    // Using a DependencyProperty as the backing store for TargetAddress.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty TargetAddressProperty =
                       DependencyProperty.Register("TargetAddress", typeof(string), typeof(ucAddressBar), new PropertyMetadata(""));
    #endregion

    #region Class ctor..
    public ucAddressBar() {
        InitializeComponent();

        //tbAddress.Focusable = false;
        tbAddress.IsTabStop = false;

        this.Loaded += UcAddressBar_Loaded;
    }
    #endregion

    private void UcAddressBar_Loaded( object sender, RoutedEventArgs e ) {
        btnSecure.ImageSource = SecureImageSource;
        btnReload.ImageSource = ReloadImageSource;
        if (!String.IsNullOrEmpty( TargetAddress )) {
            tbAddress.Text = TargetAddress;
        }
    }

    #region tbAddress.KeyUp event handler method
    private void tbAddress_KeyUp( object sender, System.Windows.Input.KeyEventArgs e ) {
        if (e.Key == System.Windows.Input.Key.Enter) {
            var oa = TargetAddress;
            TargetAddress = tbAddress.Text;
            TargetAddressChanged?.Invoke( this, new AddressChangedEventArgs() { OldAddress = oa, NewAddress = TargetAddress } );
            ClearFocus();
        }
    }

    private void ClearFocus() {
        // Kill logical focus
        FocusManager.SetFocusedElement( FocusManager.GetFocusScope( tbAddress ), null );
        // Kill keyboard focus
        Keyboard.ClearFocus();
    }

    public void SetFocus() {
        tbAddress.Focus();
    }
    #endregion

    private void tbAddress_GotFocus( object sender, RoutedEventArgs e ) {
        DispatcherTimer dt = new DispatcherTimer();
        dt.Interval = TimeSpan.FromMilliseconds( 10 );
        dt.Tick += ( sender, e ) => {
            tbAddress.SelectAll();
            dt.Stop();
        };
        dt.Start();
    }
}