using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using PWB_CCLibrary.Controls;
using PWB_CCLibrary.Interfaces;

namespace WpfBrowser.Controls.Main;

/// <summary>
/// Interaction logic for ucBrowser.xaml
/// </summary>
public partial class ucBrowser : UserControl, IBrowserPanel {
    private static int Counter = 0;


    public int Id { get; set; }

    #region Dependency Properties (Url)
    public string Url {
        get { return (string)GetValue( UrlProperty ); }
        set {
            if (value.Equals( Url )) return;
            SetValue( UrlProperty, value );
            //abTargetAddress.TargetAddress = value;
        }
    }

    public PWB_TabItem? TabItem { get; set; }

    // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty UrlProperty =
                           DependencyProperty.Register("Url", typeof(string), typeof(ucBrowser), new PropertyMetadata(""));
    #endregion

    private ucBrowser() {
        this.Id = ++Counter;
        InitializeComponent();
        this.Loaded += UcBrowser_Loaded;
    }

    public ucBrowser( PWB_TabItem tabItem ) : this() {
        TabItem = tabItem;
    }

    private void UcBrowser_Loaded( object sender, RoutedEventArgs e ) {
        //if (!string.IsNullOrEmpty( Url )) {
        //    cwbBrowser.Address = abTargetAddress.TargetAddress = Url;
        //}
    }

    private void tblkAddress_KeyUp( object sender, KeyEventArgs e ) {
        //if (e.Key == Key.Enter) {
        //    Url = cwbBrowser.Address = abTargetAddress.TargetAddress;
        //}
    }

    private void cwbBrowser_Initialized( object sender, System.EventArgs e ) {
        //if (Url is not null)
        //    abTargetAddress.TargetAddress = cwbBrowser.Address = Url;
    }


    public void Goto( string newAddress ) {
        cwbBrowser.Address = newAddress;
    }

    public void Refresh() {

    }
}
