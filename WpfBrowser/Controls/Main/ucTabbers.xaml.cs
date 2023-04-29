using System.Windows.Controls;

namespace WpfBrowser.Controls.Main {
    /// <summary>
    /// Interaction logic for ucTabbers.xaml
    /// </summary>
    public partial class ucTabbers : UserControl {
        public ucTabbers() {
            InitializeComponent();

            cmbTabGroups.Items.Add( "Main Group" );
            cmbTabGroups.SelectedIndex = 0;

            lstTabs.Items.Add( "Google" );
            lstTabs.SelectedIndex = 0;
        }
    }
}
