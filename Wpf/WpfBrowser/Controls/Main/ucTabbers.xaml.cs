using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using PWB_CCLibrary.Classes;
using PWB_CCLibrary.Controls;
using PWB_CCLibrary.Delegates;

namespace WpfBrowser.Controls.Main;
/// <summary>
/// Interaction logic for ucTabbers.xaml
/// </summary>
public partial class ucTabbers : UserControl {
    public static readonly int MaxPinnedPageCount = 7;

    public event SelectedTabItemChangedEventHandler? SelectionChanged;

    public ObservableCollection<TabGroup> TabGroups = new ObservableCollection<TabGroup>();

    #region members and properties
    private PWB_TabItem? selectedItem = null;

    public PWB_TabItem? SelectedItem {
        get { return selectedItem; }
        set {
            var oi = selectedItem;
            selectedItem = value;
            SelectionChanged?.Invoke( this, new SelectedTabItemChangedEventArgs( selectedTabGroup, oi, selectedTabGroup, selectedItem ) );
        }

    }

    private Stack<(TabGroup, PWB_TabItem)> closedTabs = new Stack<(TabGroup, PWB_TabItem)>();

    private TabGroup? selectedTabGroup = null;
    #endregion

    #region Class ctor..
    public ucTabbers() {
        InitializeComponent();

        var tg = new TabGroup( "Main Group", IsPinOk, Item_TabItemSelected, Item_PinnedChanged, Item_CloseButtonPressed );
        TabGroups.Add( tg );

        cmbTabGroups.ItemsSource = TabGroups;
        cmbTabGroups.SelectionChanged += CmbTabGroups_SelectionChanged;
        cmbTabGroups.SelectedIndex = 0;



        //for (int i = 0; i < 50; i++) {
        //    selectedTabGroup?.NormalItems.Add( selectedTabGroup!.CreateNewTabItem() );
        //}
        //for (int i = 0; i < 10; i++) {
        //    TabGroups[1].NormalItems.Add( TabGroups[1].CreateNewTabItem() );
        //}
    }
    #endregion

    private void CmbTabGroups_SelectionChanged( object sender, SelectionChangedEventArgs e ) {
        ClearSelection();
        if (selectedTabGroup is not null) {
            ClearAllSelections();
        }
        selectedTabGroup = cmbTabGroups.SelectedItem as TabGroup;
        if (selectedTabGroup != null) {
            PinnedItems.ItemsSource = selectedTabGroup.PinnedItems;
            NormalItems.ItemsSource = selectedTabGroup.NormalItems;

            var a = selectedTabGroup.PinnedItems.Where( a => a.IsSelected ).ToList();
            if (a.Count == 0)
                a = selectedTabGroup.NormalItems.Where( a => a.IsSelected ).ToList();

            if (a.Count == 1)
                SelectedItem = a[0];
        }
    }

    #region public methods
    public PWB_TabItem CreateNewTab( string url ) {
        if (selectedTabGroup is null)
            cmbTabGroups.SelectedIndex = 0;

        var ti = selectedTabGroup!.CreateNewTabItem();
        ti.IsSelected = true;
        ti.WebPanel = new ucBrowser( ti );
        ti.Address = url;

        return ti;
    }
    public void ClearSelection() {
        SelectedItem = null;

    }
    public void ClearAllSelections() {
        SelectedItem = null;
        PinnedItems.ItemsSource = null;
        NormalItems.ItemsSource = null;
        selectedTabGroup = null;
    }

    public bool IsPinOk() => PinnedItems.Items.Count < MaxPinnedPageCount;
    #endregion

    #region TabItems' event handlers
    private void Item_TabItemSelected( object sender, TabItemEventArgs e ) {
        if (e.TabItem is not null) {
            if (selectedItem is not null)
                selectedItem.IsSelected = false;
            SelectedItem = e.TabItem;
        }
    }

    private void Item_PinnedChanged( object sender, RoutedEventArgs e ) {
        if (sender is PWB_TabItem item) {
            if (item.IsPinned) {
                selectedTabGroup?.NormalItems.Remove( item );
                item.Margin = new Thickness( 2, 0, 0, 0 );
                selectedTabGroup?.PinnedItems.Add( item );
                return;
            }
            selectedTabGroup?.PinnedItems.Remove( item );
            item.Margin = new Thickness( 1, 0, 0, 0 );
            NormalItems.Items.Add( item );
        }
    }

    private void Item_CloseButtonPressed( object sender, RoutedEventArgs e ) {
        if (sender is PWB_TabItem item) {
            if (item.IsPinned) {
                selectedTabGroup?.PinnedItems.Remove( item );
            } else {
                selectedTabGroup?.NormalItems.Remove( item );
            }

            closedTabs.Push( (selectedTabGroup!, item) );

            if (object.ReferenceEquals( selectedItem, item ))
                selectedItem = null;
        }
    }
    #endregion
}
