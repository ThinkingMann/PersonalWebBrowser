using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

using PWB_CCLibrary.Classes;
using PWB_CCLibrary.Controls;

using PWB_CCLibrary.Delegates;

namespace WpfBrowser.Controls.Main;
/// <summary>
/// Interaction logic for MainContent.xaml
/// </summary>
public partial class ucMainContent : UserControl, INotifyPropertyChanged {
    private static string HomePage = "https://github.com/ThinkingMann/PersonalWebBrowser";
    private static string DefaultSearchPage = "https://www.google.com/search?q={0}&oq={1}&aqs=pwb..{2}&ie=UTF-8";

    public event PropertyChangedEventHandler? PropertyChanged;

    private void RaisePropertyChanged( [CallerMemberName] string? propertyname = null ) =>
                            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyname ) );

    public Guid Id { get; } = Guid.NewGuid();

    private PWB_TabItem? SelectedTabItem { get; set; } = null;

    public bool CanGoPreviousPage => SelectedTabItem?.CanGoPreviousPage ?? false;
    public bool CanGoNextPage => SelectedTabItem?.CanGoNextPage ?? false;
    public string? PreviousAddress => SelectedTabItem?.PreviousAddress;
    public string? NextAddress => SelectedTabItem?.NextAddress;
    private void RaiseTabItemChangedEvents() {
        RaisePropertyChanged( nameof( CanGoPreviousPage ) );
        RaisePropertyChanged( nameof( CanGoNextPage ) );
        RaisePropertyChanged( nameof( PreviousAddress ) );
        RaisePropertyChanged( nameof( NextAddress ) );
    }

    private bool isNewPageMode;

    public bool IsNewPageMode {
        get { return isNewPageMode; }
        set {
            if (isNewPageMode != value) {
                isNewPageMode = value;
                RaisePropertyChanged( nameof( IsNewTabEnabled ) );
            }
        }
    }

    public bool IsNewTabEnabled => !IsNewPageMode;

    public ucMainContent() {
        InitializeComponent();

        Tabber.SelectionChanged += Tabber_SelectionChanged;
        Loaded += UcMainContent_Loaded;
    }

    private void UcMainContent_Loaded( object sender, System.Windows.RoutedEventArgs e ) {
        //LoadOperations();
        if (Tabber.SelectedItem is null)
            GotoHomePage( true );

        // Test Code
        Tabber.CreateNewTab( "www.google.com", false );
        Tabber.CreateNewTab( "www.hotmail.com", false );
        Tabber.CreateNewTab( "www.msn.com", false );
        Tabber.CreateNewTab( "www.gmail.com", false );
        Tabber.CreateNewTab( "www.e-devlet.gov.tr", false );
        Tabber.CreateNewTab( "www.turkiye.gov.tr", false );
        Tabber.CreateNewTab( "www.nasa.gov", false );
        Tabber.CreateNewTab( "www.facebook.gov", false );
        Tabber.CreateNewTab( "www.youtube.com", false );
        Tabber.CreateNewTab( "www.amazon.com", false );
        Tabber.CreateNewTab( "www.gitlab.com", false );
        Tabber.CreateNewTab( "www.iconscout.com", false );
        Tabber.CreateNewTab( "workspace.google.com", false );
        Tabber.CreateNewTab( "www.ebay.com", false );
        Tabber.CreateNewTab( "www.stackexchange.com", false );
        Tabber.CreateNewTab( "www.instagram.com", false );
        Tabber.CreateNewTab( "play.google.com", false );
        Tabber.CreateNewTab( "www.microsoft.com", false );
        Tabber.CreateNewTab( "visualstudio.microsoft.com", false );
        Tabber.CreateNewTab( "dotnet.microsoft.com", false );
        Tabber.CreateNewTab( "www.jetbrains.com", false );
        Tabber.CreateNewTab( "medium.com", false );
        Tabber.CreateNewTab( "openai.com", false );
        Tabber.CreateNewTab( "cloud.google.com", false );
        Tabber.CreateNewTab( "azure.microsoft.com", false );
        Tabber.CreateNewTab( "www.wikipedia.org", false );
        Tabber.CreateNewTab( "www.gnoosic.com", false );
        Tabber.CreateNewTab( "www.patatap.com", false );
        Tabber.CreateNewTab( "notalwaysright.com", false );
        Tabber.CreateNewTab( "www.flashbynight.com/drench", false );
        Tabber.CreateNewTab( "riverstyx.com", false );

        RaiseTabItemChangedEvents();
    }

    public void GotoHomePage( bool createTabItem = false ) {
        PWB_TabItem ti;
        if (createTabItem || Tabber.SelectedItem is null) {
            ti = Tabber.CreateNewTab( HomePage );
        } else {
            ti = Tabber.SelectedItem;
            ti.Address = HomePage;
        }
        ContentCtrl.Content = ti.WebPanel;
        abTargetAddress.TargetAddress = ti.Address;
    }

    private void abTargetAddress_TargetAddressChanged( object sender, AddressChangedEventArgs e ) {
        var newAddress = e.NewAddress;
        if (newAddress is not null &&
            (newAddress.Contains( ' ' ) || !newAddress.Replace( "www.", "" ).Contains( '.' ))) {
            var formatted = newAddress.Replace(' ', '+');
            newAddress = String.Format( DefaultSearchPage, formatted, formatted, Id.ToString() );
            abTargetAddress.TargetAddress = newAddress;
        }

        if (string.IsNullOrEmpty( newAddress )) {
            if (IsNewPageMode) {
                Tabber.CloseTabItem( newTabItem! );
            }
        } else {
            if (!(Tabber.SelectedItem is PWB_TabItem ti)) {
                ti = Tabber.CreateNewTab( newAddress! );
                ContentCtrl.Content = ti.WebPanel;
            }
            ti.Address = newAddress;
        }
        IsNewPageMode = false;
        newTabItem = null;
        RaiseTabItemChangedEvents();
    }

    TabGroup? newItemsGroup = null;
    PWB_TabItem? newTabItem = null;
    bool isInAddNewTabMethod = false;

    private void abbNewTab_Click( object sender, System.Windows.RoutedEventArgs e ) {
        isInAddNewTabMethod = true;
        IsNewPageMode = true;
        newItemsGroup = Tabber.SelectedTabGroup;
        var ti = newTabItem = Tabber.CreateNewTab( string.Empty );
        ContentCtrl.Content = ti.WebPanel;
        abTargetAddress.TargetAddress = string.Empty;
        abTargetAddress.SetFocus();
        isInAddNewTabMethod = false;
    }

    private void Tabber_SelectionChanged( object sender, PWB_CCLibrary.Delegates.SelectedTabItemChangedEventArgs e ) {
        if (!isInAddNewTabMethod && IsNewPageMode && string.IsNullOrEmpty( newTabItem?.Address ) && !object.ReferenceEquals( newTabItem, e.NewTabItem )) {
            IsNewPageMode = false;
            newItemsGroup!.CloseTabItem( newTabItem! );
        }
        if (e.NewTabItem is null) {
            ContentCtrl.Content = null;
            return;
        }
        ContentCtrl.Content = e.NewTabItem.WebPanel;
        abTargetAddress.TargetAddress = e.NewTabItem.Address;
        SelectedTabItem = e.NewTabItem;
        RaiseTabItemChangedEvents();
    }

    private void abbPreviousPage_Click( object sender, System.Windows.RoutedEventArgs e ) {
        Tabber.GoPreviousPage();
        if (SelectedTabItem is { })
            abTargetAddress.TargetAddress = SelectedTabItem.Address;
        RaiseTabItemChangedEvents();
    }

    private void abbNextPage_Click( object sender, System.Windows.RoutedEventArgs e ) {
        Tabber.GoNextPage();
        if (SelectedTabItem is { })
            abTargetAddress.TargetAddress = SelectedTabItem.Address;
        RaiseTabItemChangedEvents();
    }

    private void abbHomePage_Click( object sender, System.Windows.RoutedEventArgs e ) {
        if (SelectedTabItem is { })
            abTargetAddress.TargetAddress = SelectedTabItem.Address = HomePage;
        RaiseTabItemChangedEvents();
    }
}
