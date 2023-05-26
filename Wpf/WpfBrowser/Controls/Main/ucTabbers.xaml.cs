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
public partial class ucTabbers : UserControl, ITabber {
    public static readonly int MaxPinnedPageCount = 7;

    public static int TabGroupCounter = 0;

    public event SelectedTabItemChangedEventHandler? SelectionChanged;
    public event SelectedTabGroupChangedEventHandler? SelectedGroupChanged;

    public ObservableCollection<TabGroup> TabGroups { get; } = new ObservableCollection<TabGroup>();

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

    public TabGroup? SelectedTabGroup => selectedTabGroup;

    public void SetSelectedTabGroup( TabGroup tabGroup ) {
        cmbTabGroups.SelectedItem = tabGroup;
    }

    public void RemoveTabGroup( TabGroup tg, TabGroup? newTabGroupToSelect ) {
        TabGroups.Remove( tg );
        if (newTabGroupToSelect is not null) {
            cmbTabGroups.SelectedItem = newTabGroupToSelect;
        }
    }
    #endregion

    #region Class ctor..
    public ucTabbers() {
        InitializeComponent();

        cmbTabGroups.ItemsSource = TabGroups;
        cmbTabGroups.SelectionChanged += CmbTabGroups_SelectionChanged;

        _ = AddNewTabGroup();

        //cmbTabGroups.SelectedIndex = 0;
    }
    #endregion

    public TabGroup AddNewTabGroup() {
        var tg = new TabGroup( $"PWB Tab Group ({++TabGroupCounter})", IsPinOk, Item_TabItemSelected, Item_PinnedChanged, Item_CloseButtonPressed );
        TabGroups.Add( tg );
        cmbTabGroups.SelectedItem = tg;



        return tg;
    }

    private void CmbTabGroups_SelectionChanged( object sender, SelectionChangedEventArgs e ) {
        var oldTabGroup = selectedTabGroup;
        ClearSelection();
        if (selectedTabGroup is not null) {
            ClearAllSelections();
        }
        selectedTabGroup = cmbTabGroups.SelectedItem as TabGroup;
        SelectedGroupChanged?.Invoke( this, new SelectedTabGroupChangedEventArgs( oldTabGroup, selectedTabGroup ) );

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
    public PWB_TabItem CreateNewTab( string url, bool selectIt = true ) {
        if (selectedTabGroup is null)
            cmbTabGroups.SelectedIndex = 0;

        var ti = selectedTabGroup!.CreateNewTabItem();
        if (selectIt) {
            ti.IsSelected = true;
        }
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

    public void CloseTabItem( PWB_TabItem item ) {
        if (selectedTabGroup is null)
            return;
        selectedTabGroup.CloseTabItem( item );

        if (!string.IsNullOrEmpty( item.Address ))
            closedTabs.Push( (selectedTabGroup!, item) );

        if (object.ReferenceEquals( selectedItem, item ))
            selectedItem = null;
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
                selectedTabGroup?.PinnedItems.Add( item );
                return;
            }
            selectedTabGroup?.PinnedItems.Remove( item );
            NormalItems.Items.Add( item );
        }
    }

    private void Item_CloseButtonPressed( object sender, RoutedEventArgs e ) {
        if (sender is PWB_TabItem item)
            CloseTabItem( item );
    }
    #endregion

    internal void GoPreviousPage() {
        if (SelectedItem is { } && SelectedItem.CanGoPreviousPage)
            SelectedItem.GoPreviousPage();
    }

    internal void GoNextPage() {
        if (SelectedItem is { } && SelectedItem.CanGoNextPage)
            SelectedItem.GoNextPage();
    }
}
