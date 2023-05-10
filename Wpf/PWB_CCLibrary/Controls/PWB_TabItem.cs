using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using PWB_CCLibrary.Delegates;
using PWB_CCLibrary.Interfaces;

namespace PWB_CCLibrary.Controls;

public class PWB_TabItem : Control, INotifyPropertyChanged {

    public Func<bool> IsPinOk { get; private set; }

    #region static ctor.. and members
    private static string DefaultImage = "pack://application:,,,/PWB_CCLibrary;component/Assets/chrome.png";
    private static string PinImageUrl = "pack://application:,,,/PWB_CCLibrary;component/Assets/pin.png";
    private static string PinnedImageUrl = "pack://application:,,,/PWB_CCLibrary;component/Assets/pinned.png";
    private static string CloseImageUrl = "pack://application:,,,/PWB_CCLibrary;component/Assets/close.png";
    static PWB_TabItem() {
        DefaultStyleKeyProperty.OverrideMetadata( typeof( PWB_TabItem ), new FrameworkPropertyMetadata( typeof( PWB_TabItem ) ) );
    }
    #endregion

    #region members and Properties
    Stack<string> previousAddresses = new Stack<string>();
    Stack<string> nextAddresses = new Stack<string>();

    public bool CanGoPreviousPage => previousAddresses.Count > 0;
    public bool CanGoNextPage => nextAddresses.Count > 0;
    public string? PreviousAddress => previousAddresses.Count > 0 ? previousAddresses.Peek() : null;
    public string? NextAddress => nextAddresses.Count > 0 ? nextAddresses.Peek() : null;
    private void RaiseAddressChangedEvents() {
        RaisePropertyChanged( nameof( CanGoPreviousPage ) );
        RaisePropertyChanged( nameof( CanGoNextPage ) );
        RaisePropertyChanged( nameof( PreviousAddress ) );
        RaisePropertyChanged( nameof( NextAddress ) );
    }
    #endregion

    #region Dependency Properties
    protected string WebPageImage {
        get => (string)GetValue( WebPageImageProperty );
        set {
            SetValue( WebPageImageProperty, value );
            PrepareWebPageImage();
        }
    }

    public static readonly DependencyProperty WebPageImageProperty =
                           DependencyProperty.Register(nameof( WebPageImage ), typeof(string), typeof(PWB_TabItem), new PropertyMetadata( DefaultImage ));

    public string Address {
        get => (string)GetValue( AddressProperty );
        set {
            if (Address.Equals( value ))
                return;
            if (!string.IsNullOrEmpty( Address )) {
                previousAddresses.Push( Address );
                nextAddresses.Clear();
                RaiseAddressChangedEvents();
            }
            SetAddressProp( value );
            // SetValue( AddressProperty, value );
            //Title = string.Empty;
            //PageTitle = value;
            //WebPanel!.Goto( value );
        }
    }

    private void SetAddressProp( string newAddress ) {
        SetValue( AddressProperty, newAddress );
        Title = string.Empty;
        PageTitle = newAddress;
        WebPanel!.Goto( newAddress );
    }

    public static readonly DependencyProperty AddressProperty =
                           DependencyProperty.Register(nameof( Address ), typeof(string), typeof(PWB_TabItem), new PropertyMetadata(string.Empty));

    private string title = string.Empty;

    public string Title {
        get => title;
        set {
            PageTitle = title = value;
        }
    }

    public string PageTitle {
        get => (string)GetValue( PageTitleProperty );
        set => SetValue( PageTitleProperty, value );
    }

    public static readonly DependencyProperty PageTitleProperty =
                           DependencyProperty.Register(nameof( PageTitle ), typeof(string), typeof(PWB_TabItem), new PropertyMetadata(string.Empty));

    public bool IsPinned {
        get => (bool)GetValue( IsPinnedProperty );
        set => SetValue( IsPinnedProperty, value );
    }

    public static readonly DependencyProperty IsPinnedProperty =
                           DependencyProperty.Register("IsPinned", typeof(bool), typeof(PWB_TabItem), new PropertyMetadata(false));

    public bool IsSelected {
        get => (bool)GetValue( IsSelectedProperty );
        set {
            if (IsSelected == value)
                return;
            SetValue( IsSelectedProperty, value );
            if (value)
                TabItemSelected?.Invoke( this, new TabItemEventArgs( this ) );
            else
                TabItemUnselected?.Invoke( this, new TabItemEventArgs( this ) );
        }
    }

    public static readonly DependencyProperty IsSelectedProperty =
                           DependencyProperty.Register("IsSelected", typeof(bool), typeof(PWB_TabItem), new PropertyMetadata(false));

    public bool IsCMenuShown {
        get { return (bool)GetValue( IsCMenuShownProperty ); }
        set { SetValue( IsCMenuShownProperty, value ); }
    }

    // Using a DependencyProperty as the backing store for IsCMenuShown.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty IsCMenuShownProperty =
    DependencyProperty.Register("IsCMenuShown", typeof(bool), typeof(PWB_TabItem), new PropertyMetadata(false));
    #endregion

    #region Event registrations
    public IBrowserPanel? WebPanel { get; set; }

    public static readonly RoutedEvent PinnedChangedClickEvent = EventManager.RegisterRoutedEvent(nameof( PinnedChanged ), RoutingStrategy.Bubble,
                                                                        typeof(RoutedEventArgs), typeof(PWB_TabItem) );
    public static readonly RoutedEvent CloseButtonClickEvent = EventManager.RegisterRoutedEvent(nameof( CloseButtonPressed ), RoutingStrategy.Bubble,
                                                                        typeof(RoutedEventArgs), typeof(PWB_TabItem) );

    public event RoutedEventHandler PinnedChanged {
        add { AddHandler(PinnedChangedClickEvent, value); }
        remove { RemoveHandler(PinnedChangedClickEvent, value); }
    }

    public event RoutedEventHandler CloseButtonPressed {
        add { AddHandler(CloseButtonClickEvent, value); }
        remove { RemoveHandler(CloseButtonClickEvent, value); }
    }

    private void OnTabItemClicked( object sender, RoutedEventArgs e ) {
        this.IsSelected = true;
    }

    private void OnPinButtonClicked( object sender, RoutedEventArgs e ) {
        PinMe();
        e.Handled = true;
    }

    private void PinMe() {
        if (!IsPinned && !(IsPinOk()!)) {
            MessageBox.Show( "You have reached to the maximum pinned page count.", "Personal Web Browser", MessageBoxButton.OK, MessageBoxImage.Warning );
            return;
        }
        IsPinned = !IsPinned;
        if (pinImage is not null)
            pinImage.Source = PreparePinImage();
        RaiseEvent( new RoutedEventArgs( PinnedChangedClickEvent ) );
    }

    private void OnCloseButtonClicked( object sender, RoutedEventArgs e ) {
        CloseMe();
        e.Handled = true;
    }

    public void CloseMe() {
        RaiseEvent( new RoutedEventArgs( CloseButtonClickEvent ) );
    }

    public event TabItemSelectedHandler TabItemSelected;
    public event TabItemUnselectedHandler TabItemUnselected;
    public event PropertyChangedEventHandler? PropertyChanged;

    private void RaisePropertyChanged( string property ) => PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( property ) );
    #endregion

    #region Template Contsols as members
    Button? mainButton, pinButton, closeButton;
    System.Windows.Controls.Image? myImage, pinImage, closeImage;
    TextBlock? webPageTitle;
    #endregion

    protected PWB_TabItem() : base() {
        this.Background = Brushes.LightGray;
        this.Loaded += PWB_TabItem_Loaded;
        #region Context menu creation
        this.ContextMenu = new ContextMenu();
        this.ContextMenu.Items.Add( new MenuItem() { Header = "Refresh Page", Tag = "Refresh" } );
        this.ContextMenu.Items.Add( new MenuItem() { Header = "Pin", Tag = "Pin" } );
        this.ContextMenu.Items.Add( new Separator() );
        this.ContextMenu.Items.Add( new MenuItem() { Header = "Close", Tag = "Close" } );
        this.ContextMenuOpening += ContextMenu_ContextMenuOpening;
        this.ContextMenuClosing += ContextMenu_ContextMenuClosing;
        #endregion
    }

    public PWB_TabItem( Func<bool> isPinOK ) : this() => IsPinOk = isPinOK;

    private void ContextMenu_ContextMenuClosing( object sender, ContextMenuEventArgs e ) {
        IsCMenuShown = false;
    }

    private void ContextMenu_ContextMenuOpening( object sender, ContextMenuEventArgs e ) {
        pinMenuItem!.Header = IsPinned ? "Unping" : "Pin";
        IsCMenuShown = true;
    }

    MenuItem? pinMenuItem;


    private void PWB_TabItem_Loaded( object sender, RoutedEventArgs e ) {
        PrepareWebPageImage();
        PageTitle = string.IsNullOrEmpty( Title ) ? Address : Title;

        if (this.ContextMenu is not null) {
            foreach (var item in this.ContextMenu.Items) {
                if (item is MenuItem mi) {
                    if ((mi.Tag as string)!.Equals( "Pin" )) pinMenuItem = mi;
                    mi.AddHandler( MenuItem.ClickEvent, new RoutedEventHandler( ContextMenuItem_Click ) );
                }
            }
        }
    }

    private void ContextMenuItem_Click( object sender, RoutedEventArgs e ) {
        MenuItem item = (e.Source as MenuItem)!;

        var str = item.Tag as string;
        switch (str) {
            case "Refresh":
                WebPanel?.Refresh();
                break;
            case "Pin":
                PinMe();
                break;
            case "Close":
                CloseMe();
                break;
            default: break;
        }
        e.Handled = true;
    }

    #region Public Methods
    public void GoPreviousPage() {
        var addr = previousAddresses.Pop();
        nextAddresses.Push( Address );
        SetAddressProp( addr );
        PrepareWebPageImage();
    }

    public void GoNextPage() {
        var addr = nextAddresses.Pop();
        previousAddresses.Push( Address );
        SetAddressProp( addr );
        PrepareWebPageImage();
    }
    #endregion

    #region Method overridings
    public override void OnApplyTemplate() {

        mainButton = Template.FindName( "PART_MainButton", this ) as Button;
        if (mainButton is null)
            throw new System.Exception( "MainButton control does not exist in the applied template." );
        mainButton.Click += OnTabItemClicked;

        pinButton = Template.FindName( "PART_PinButton", this ) as Button;
        if (pinButton is null)
            throw new System.Exception( "PinButton control does not exist in the applied template." );
        pinButton.Click += OnPinButtonClicked;

        closeButton = Template.FindName( "PART_CloseButton", this ) as Button;
        if (closeButton is null)
            throw new System.Exception( "CloseButton control does not exist in the applied template." );
        closeButton.Click += OnCloseButtonClicked;

        myImage = Template.FindName( "PART_WebPageLogo", this ) as System.Windows.Controls.Image;
        if (myImage is null)
            throw new System.Exception( "WebPageLogo control does not exist in the applied template." );

        pinImage = Template.FindName( "PART_PinImage", this ) as System.Windows.Controls.Image;
        if (pinImage is null)
            throw new System.Exception( "PinImage control does not exist in the applied template." );
        pinImage.Source = PreparePinImage();

        closeImage = Template.FindName( "PART_CloseImage", this ) as System.Windows.Controls.Image;
        if (closeImage is null)
            throw new System.Exception( "CloseImage control does not exist in the applied template." );
        closeImage.Source = PrepareImage( CloseImageUrl );

        webPageTitle = Template.FindName( "PART_WebPageTitle", this ) as TextBlock;
        if (mainButton is null)
            throw new System.Exception( "WebPageTitle control does not exist in the applied template." );

        base.OnApplyTemplate();
    }
    #endregion

    #region PrepareImage() methods
    private void PrepareWebPageImage() {
        try {
            string imgSource = WebPageImage;
            if (String.IsNullOrEmpty( imgSource ))
                return;

            #region Definition of image
            if (myImage is not null)
                myImage.Source = PrepareImage( FixImageUrl( imgSource ) );
            #endregion
        } catch {
        }
    }

    private string FixImageUrl( string url ) {
        var imgsrc = WebPageImage;
        if (!imgsrc.StartsWith( "pack://application:,,," ) && !imgsrc.StartsWith( "http" ))
            imgsrc = "pack://application:,,," + imgsrc;
        return imgsrc;
    }

    private BitmapImage PreparePinImage() => PrepareImage( IsPinned ? PinnedImageUrl : PinImageUrl );

    private BitmapImage PrepareImage( string url ) {
        BitmapImage bi = new BitmapImage();
        bi.BeginInit();
        bi.UriSource = new Uri( url, UriKind.RelativeOrAbsolute );
        bi.EndInit();
        return bi;
    }
    #endregion
}
