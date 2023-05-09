using System.Windows.Controls;

using PWB_CCLibrary.Controls;

using PWB_UserControls.Delegates;

namespace WpfBrowser.Controls.Main;
/// <summary>
/// Interaction logic for MainContent.xaml
/// </summary>
public partial class ucMainContent : UserControl {
    public ucMainContent() {
        InitializeComponent();
    }

    private void abTargetAddress_TargetAddressChanged( object sender, AddressChangedEventArgs e ) {
        if (e.NewAddress is null) {
            return;
        }
        if (!(Tabber.SelectedItem is PWB_TabItem ti)) {
            ti = Tabber.CreateNewTab( e.NewAddress! );
            ContentCtrl.Content = ti.WebPanel;
        }
        ti.Address = e.NewAddress;
        //ContentCtrl.Focus();
    }

}
