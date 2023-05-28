using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shell;
using System.Windows.Threading;

using PWB_CCLibrary.Classes;

namespace PWB_CCLibrary.Controls;

public class TabGroupControl : Control {
    public const int DefaultWidth = 200;

    #region static ctor..
    static TabGroupControl() {
        DefaultStyleKeyProperty.OverrideMetadata( typeof( TabGroupControl ), new FrameworkPropertyMetadata( typeof( TabGroupControl ) ) );
    }
    #endregion

    #region Dependency Properties
    public string ImageKey {
        get { return (string)GetValue( ImageKeyProperty ); }
        set {
            SetValue( ImageKeyProperty, value );
        }
    }

    // Using a DependencyProperty as the backing store for DataKey.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ImageKeyProperty =
    DependencyProperty.Register(nameof( ImageKey ), typeof(string), typeof(TabGroupControl), new PropertyMetadata(""));

    public string ImageSource {
        get { return (string)GetValue( ImageSourceProperty ); }
        set { SetValue( ImageSourceProperty, value ); }
    }

    // Using a DependencyProperty as the backing store for ImageSource.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ImageSourceProperty =
    DependencyProperty.Register(nameof( ImageSource ), typeof(string), typeof(TabGroupControl), new PropertyMetadata(""));

    public int RotateAngle {
        get { return (int)GetValue( RotateAngleProperty ); }
        set { SetValue( RotateAngleProperty, value ); }
    }

    // Using a DependencyProperty as the backing store for RotateAngle.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty RotateAngleProperty =
    DependencyProperty.Register(nameof( RotateAngle ), typeof(int), typeof(TabGroupControl), new PropertyMetadata(0));
    #endregion

    #region Members and Properties
    Grid? _myInnerGrid, _myOuterGrid;
    StackPanel? _myTabDragger;
    Button? _myButton;

    Window? _thisWindow;

    private Dictionary<TabGroup, TabGroupButton> relation = new Dictionary<TabGroup, TabGroupButton>();
    private Dictionary<int, TabGroupButton> positions = new Dictionary<int, TabGroupButton>();
    private List<Border> borders = new List<Border>();

    public ObservableCollection<TabGroup>? TabGroups {
        get => Tabber?.TabGroups;
    }

    private IMainContent mainContent;

    public IMainContent MainContent {
        get => mainContent;
        set {
            mainContent = value;
        }
    }

    private ITabber _tabber;

    public ITabber Tabber {
        get => _tabber;
        set {
            _tabber = value;
            _tabber.TabGroups.CollectionChanged += TabGroups_CollectionChanged;
            _tabber.SelectedGroupChanged += _tabber_SelectedGroupChanged;
        }
    }

    TabGroupButton? SelectedTabGroup { get; set; }
    #endregion

    #region Class ctor.. and Loaded event handler
    public TabGroupControl() {
        this.Margin = new System.Windows.Thickness( 7, 10, 5, 0 );
        this.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
        this.SetValue( WindowChrome.IsHitTestVisibleInChromeProperty, true );

        this.Loaded += TabGroupControl_Loaded;
        this.SizeChanged += TabGroupControl_SizeChanged;
    }

    private void TabGroupControl_Loaded( object sender, System.Windows.RoutedEventArgs e ) {
        _thisWindow = System.Windows.Window.GetWindow( this );
        _myInnerGrid!.MouseLeave += _myInnerGrid_MouseLeave;
        SetInnerGridMaxSize();
        PrepareTabGroups();
    }

    private void PrepareTabGroups() {
        foreach (var grp in TabGroups!) {
            AddNewGroupButton( grp );
        }
    }

    private void TabGroupControl_SizeChanged( object sender, SizeChangedEventArgs e ) => SetInnerGridMaxSize();

    private void _myInnerGrid_MouseLeave( object sender, MouseEventArgs e ) => SetInnerGridSize();

    private void SetInnerGridMaxSize()
        => _myInnerGrid!.MaxWidth = this.ActualWidth - (_myButton!.ActualWidth + 10);
    private void SetInnerGridSize() {
        var width = Math.Min( _myInnerGrid!.MaxWidth, _myInnerGrid.ColumnDefinitions.Count * DefaultWidth );
        _myInnerGrid!.Width = width;
        var columnWidth = width / _myInnerGrid.ColumnDefinitions.Count;
        foreach (var tgb in positions.Values) {
            tgb.Width = columnWidth;
            tgb.MinWidth = columnWidth;
            tgb.MaxWidth = columnWidth;
        }
        foreach (var clm in _myInnerGrid.ColumnDefinitions) {
            clm.Width = new GridLength( columnWidth );
            clm.MinWidth = columnWidth;
            clm.MaxWidth = columnWidth;
        }
    }
    private void SetInnerGridTempSize( double defWidth ) {
        _myInnerGrid!.Width = _myInnerGrid.ColumnDefinitions.Count * defWidth;
        var columnWidth = defWidth / _myInnerGrid.ColumnDefinitions.Count;
        foreach (var tgb in positions.Values) {
            tgb.Width = columnWidth;
            tgb.MinWidth = columnWidth;
            tgb.MaxWidth = columnWidth;
        }
        foreach (var clm in _myInnerGrid.ColumnDefinitions) {
            clm.Width = new GridLength( columnWidth );
            clm.MinWidth = columnWidth;
            clm.MaxWidth = columnWidth;
        }
    }
    #endregion

    #region Parent Controls' event handlers
    private void _tabber_SelectedGroupChanged( object sender, Delegates.SelectedTabGroupChangedEventArgs e ) {
        if (SelectedTabGroup != null) {
            if (e.NewTabGroup is not null && e.NewTabGroup.Equals( SelectedTabGroup.TabGroupItem ))
                return;
            SelectedTabGroup.IsSelected = false;
        }
        SelectedTabGroup = null;
        if (e.NewTabGroup is { }) {
            SelectedTabGroup = relation[e.NewTabGroup];
            SelectedTabGroup.IsSelected = true;
        }
    }

    private void TabGroups_CollectionChanged( object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e ) {
        switch (e.Action) {
            case System.Collections.Specialized.NotifyCollectionChangedAction.Add: {
                if (e.NewItems is not null) {
                    foreach (var item in e.NewItems) {
                        if (item is TabGroup grp)
                            AddNewGroupButton( grp );
                    }
                    SetInnerGridSize();
                }
                break;
            }
            case System.Collections.Specialized.NotifyCollectionChangedAction.Remove: {
                if (e.OldItems is not null) {
                    foreach (var item in e.OldItems) {
                        if (item is TabGroup grp) {
                            var tgb = relation[grp];
                            tgb.Click -= Tgb_Click;
                            tgb.CloseRequested -= Tgb_CloseRequested;
                            relation.Remove( grp );
                            positions.Remove( tgb.TabPosition );
                            _myInnerGrid!.Children.Remove( tgb );
                            _myInnerGrid!.ColumnDefinitions.RemoveAt( /*tgb.TabPosition*/0 );
                            RemoveLastSeparator();
                            ShiftControlsToLeft( tgb.TabPosition );
                            SetInnerGridTempSize( tgb.ActualWidth );
                        }
                    }
                }
                break;
            }
            default: break;
        }
    }
    #endregion

    private void AddNewGroupButton( TabGroup grp ) {
        var cd = InsertAColumn();
        var tgb = new TabGroupButton();
        tgb.Title = grp.Name;
        tgb.TabPosition = _myInnerGrid!.ColumnDefinitions.Count - 1;
        positions.Add( tgb.TabPosition, tgb );
        tgb.TabGroupItem = grp;
        //tgb.TabColor = Colors.Red;
        if (tgb.IsSelected = Tabber.SelectedTabGroup == grp)
            SelectedTabGroup = tgb;
        tgb.SetValue( Grid.ColumnProperty, tgb.TabPosition );
        tgb.Click += Tgb_Click;
        tgb.CloseRequested += Tgb_CloseRequested;
        tgb.PreviewMouseLeftButtonDown += Tgb_PreviewMouseLeftButtonDown;
        tgb.MouseEnter += Tgb_MouseEnter;
        tgb.MouseLeave += Tgb_MouseLeave;
        relation.Add( grp, tgb );
        _myInnerGrid!.Children.Add( tgb );
    }

    #region Separator Visibilities
    private void Tgb_MouseLeave( object sender, System.Windows.Input.MouseEventArgs e ) {
        if (e.Source is TabGroupButton tgb && !tgb.IsSelected) {
            if (tgb.TabPosition > 0)
                borders[tgb.TabPosition - 1].Visibility = Visibility.Visible;
            if (borders.Count > tgb.TabPosition)
                borders[tgb.TabPosition].Visibility = Visibility.Visible;
        }
    }

    private void Tgb_MouseEnter( object sender, System.Windows.Input.MouseEventArgs e ) {
        if (e.Source is TabGroupButton tgb && !tgb.IsSelected) {
            if (tgb.TabPosition > 0)
                borders[tgb.TabPosition - 1].Visibility = Visibility.Collapsed;
            borders[tgb.TabPosition].Visibility = Visibility.Collapsed;
        }
    }
    #endregion

    #region Dragging and Dropping of a TabGroupButton
    TabGroupButton? draggingButton = null;
    bool isDragging = false;
    Point mousePos;
    double left = 0;
    Boolean isWaitingForDragging = false;
    enum DragModes { Tab, Window }
    DragModes DragMode = DragModes.Tab;

    private void Tgb_PreviewMouseLeftButtonDown( object sender, System.Windows.Input.MouseButtonEventArgs e ) {
        try {
            if (!isDragging && e.Source is TabGroupButton tgb) {
                if (tgb.IsMouseOverTheCloseButton)
                    return;
                isWaitingForDragging = true;
                DragMode = relation.Count == 1 ? DragModes.Window : DragModes.Tab;
                mousePos = e.GetPosition( _thisWindow );
                if (DragMode == DragModes.Window)
                    mousePos = _thisWindow!.PointToScreen( Mouse.GetPosition( _thisWindow ) );
                if (!tgb.IsSelected)
                    Tabber.SetSelectedTabGroup( tgb.TabGroupItem );
                DispatcherTimer timer = new();
                timer.Tick += ( sender, e ) => {
                    try {
                        if (!isWaitingForDragging)
                            return;
                        isDragging = true;
                        if (DragMode != DragModes.Window) {
                            SetInnerGridSize();
                            if (tgb.TabPosition > 0)
                                borders[tgb.TabPosition - 1].Visibility = Visibility.Visible;
                            borders[tgb.TabPosition].Visibility = Visibility.Visible;
                            draggingButton = tgb;
                            var pos = tgb.TabPosition;
                            var padding = new Thickness(left = pos*tgb.ActualWidth, 0, 0, 0);
                            tgb.MinWidth = tgb.ActualWidth;
                            tgb.MaxWidth = tgb.ActualWidth;
                            _myInnerGrid!.Children.Remove( tgb );
                            tgb.Margin = padding;
                            _myTabDragger!.Children.Add( tgb );
                        }
                        _thisWindow!.CaptureMouse();
                        _thisWindow.PreviewMouseMove += ThisWindow_MouseMove;
                        _thisWindow.PreviewMouseLeftButtonUp += ThisWindow_PreviewMouseLeftButtonUp;
                    } finally {
                        isWaitingForDragging = false;
                        ((DispatcherTimer)sender!).Stop();
                    }
                };
                timer.Interval = TimeSpan.FromMilliseconds( 200 );
                timer.Start();
            }
        } finally {
            e.Handled = false;
        }
    }

    private void ThisWindow_PreviewMouseLeftButtonUp( object sender, System.Windows.Input.MouseButtonEventArgs e ) {
        try {
            isWaitingForDragging = false;
            if (_thisWindow!.IsMouseCaptured) {
                _thisWindow!.ReleaseMouseCapture();
            }
            if (isDragging) {
                _thisWindow!.PreviewMouseMove -= ThisWindow_MouseMove;
                _thisWindow!.PreviewMouseLeftButtonUp -= ThisWindow_PreviewMouseLeftButtonUp;
                isDragging = false;
                if (DragMode == DragModes.Window)
                    return;
                _myTabDragger!.Children.Remove( draggingButton );
                draggingButton!.Margin = new Thickness( 0 );
                draggingButton!.MinWidth = 0;
                draggingButton!.MaxWidth = DefaultWidth;
                _myInnerGrid!.Children.Add( draggingButton );
                draggingButton = null;

                tmpCheckOrder();
            }
        } finally {
            e.Handled = false;
        }
    }

    private void tmpCheckOrder() {
        for (int i = 0; i < relation.Count; i++) {
            if (!positions.ContainsKey( i ))
                break;
        }
    }

    private double MaxLeftMargin => _myOuterGrid!.ActualWidth - (_myButton!.ActualWidth + 10) - draggingButton!.ActualWidth;

    private void ThisWindow_MouseMove( object sender, System.Windows.Input.MouseEventArgs e ) {
        if (isDragging) {
            if (DragMode == DragModes.Window) {
                var wndPos = _thisWindow!.PointToScreen( Mouse.GetPosition( _thisWindow ) );
                var diffX = wndPos.X - mousePos.X;
                var diffY = wndPos.Y - mousePos.Y;
                mousePos = wndPos;
                _thisWindow.Left += diffX;
                _thisWindow.Top += diffY;
                // TODO: Eğer diğer bir PWB'nin outer grid'ine girerse tgb'nin yerleşimi orada yapılacaktır.
            } else {
                #region Moving the DraggingTabGroup
                var newPos = e.GetPosition( _thisWindow );
                var objPos = e.GetPosition( _myInnerGrid );
                var diff = newPos.X - mousePos.X;
                if (newPos.X < 0 || objPos.X < -7 || newPos.Y < 0 || objPos.X > _myInnerGrid!.ActualHeight || objPos.Y > _myInnerGrid!.ActualWidth) {
                    // TODO: Burada yeni pencere açılacaktır...
                    //System.Diagnostics.Debug.WriteLine( $"Open A New Window: objPos ({objPos}), newPos ({newPos})" );
                }
                var leftMargin = (left += diff);
                if (leftMargin < 0) leftMargin = 0;
                if (leftMargin > MaxLeftMargin) leftMargin = MaxLeftMargin;
                draggingButton!.Margin = new Thickness( leftMargin, 0, 0, 0 );
                mousePos = newPos;
                #endregion

                var item = positions.Values
                    .Where( a => !object.ReferenceEquals(a, draggingButton) && a.IsMouseOverYou( e.GetPosition(a) ) )
                    .FirstOrDefault();
                if (item is not null)
                    SwitchTabsPositions( draggingButton, item );
            }
            e.Handled = true;
        }
    }
    #endregion

    #region Switching TabGroupButtons with Animation while Drag&Drop operation
    bool isSwitching = false;
    private void SwitchTabsPositions( TabGroupButton draggingButton, TabGroupButton item ) {
        Border? animator;
        ColumnDefinition draggingTGBNewPosCD, draggingTGBOrgCD;
        Storyboard? storyboard1;
        int targetPos = -1;
        TabGroupButton realTarget;
        //int storyCounter = 0;

        if (isSwitching /*|| (switchingMaster == draggingButton && switchingTarget == item)*/) return;
        //System.Diagnostics.Debug.WriteLine( "" );
        //System.Diagnostics.Debug.WriteLine( $"Starting: {++storyCounter}" );
        //System.Diagnostics.Debug.WriteLine( $"SwitchTabsPositions[1-1]: dragging:{draggingButton.TabPosition}; item:{item.TabPosition}" );
        //System.Diagnostics.Debug.WriteLine( $"SwitchTabsPositions[1-2]: dragging:{draggingButton?.GetValue( Grid.ColumnProperty )}; item:{item.GetValue( Grid.ColumnProperty )}" );
        isSwitching = true;

        realTarget = item;
        targetPos = item.TabPosition;
        #region determining object to switch
        Thickness margin;
        var inWhich = 0;
        var count = 0;
        Border? separator;
        if (draggingButton.TabPosition > item.TabPosition) {
            inWhich = 1;
            separator = borders[item.TabPosition];
            //foreach (var tgb in positions.Values) {
            //    if (tgb == item || (targetPos <= tgb.TabPosition && tgb.TabPosition < draggingButton.TabPosition)) {
            //        if (tgb.TabPosition > targetPos)
            //            realTarget = tgb;
            //        item.SetValue( Grid.ColumnProperty, ++tgb.TabPosition );
            //        count++;
            //    }
            //}

            margin = new Thickness( 0, 0, draggingButton.ActualWidth, 0 );
        } else {
            inWhich = 2;
            separator = borders[draggingButton.TabPosition];
            //foreach (var tgb in positions.Values) {
            //    if (tgb == item || (targetPos >= tgb.TabPosition && tgb.TabPosition > draggingButton.TabPosition)) {
            //        if (tgb.TabPosition < targetPos)
            //            realTarget = tgb;
            //        item.SetValue( Grid.ColumnProperty, --tgb.TabPosition );
            //        count++;
            //    }
            //}

            margin = new Thickness( draggingButton.ActualWidth, 0, 0, 0 );
        }
        #endregion

        #region Setting the switching values
        // Eski column remove ediliyor
        _myInnerGrid!.ColumnDefinitions.RemoveAt( draggingButton.TabPosition );
        // Aynı yere bir column ekleniyor
        draggingTGBOrgCD = new MyColumnDef( _myInnerGrid, draggingButton.TabPosition );
        draggingTGBOrgCD.MaxWidth = draggingButton.ActualWidth * 2;

        // ReadTarger bu sütun'a taşınıyor.
        realTarget.MinWidth = realTarget.ActualWidth;
        realTarget.Margin = margin;
        realTarget.SetValue( Grid.ColumnProperty, realTarget.TabPosition = draggingButton.TabPosition );

        separator.Visibility = Visibility.Collapsed;

        draggingTGBNewPosCD = _myInnerGrid.ColumnDefinitions[targetPos];
        draggingTGBNewPosCD.Width = GridLength.Auto;
        #region Creating Animator Border
        animator = new Border() {
            BorderThickness = new Thickness( 100 ),
            Background = Brushes.Transparent,
            Width = 0,
            Height = 20,
        };
        animator.SetValue( Grid.ColumnProperty, targetPos );
        _myInnerGrid.Children.Add( animator );
        #endregion
        #endregion


        #region Starting the animation
        TimeSpan animationDuration = TimeSpan.FromMilliseconds( 75 );
        var da = new DoubleAnimation() {
            To = draggingButton.ActualWidth,
            Duration = animationDuration
        };
        var ta = new ThicknessAnimation() {
            To = new Thickness( 0 ),
            Duration = animationDuration
        };
        storyboard1 = new Storyboard();
        storyboard1.Children.Add( ta );
        storyboard1.Children.Add( da );
        Storyboard.SetTarget( da, animator );
        Storyboard.SetTargetProperty( da, new PropertyPath( "Width" ) );
        Storyboard.SetTarget( ta, realTarget );
        Storyboard.SetTargetProperty( ta, new PropertyPath( "Margin" ) );
        storyboard1.Completed += ( s, e ) => {
            separator.Visibility = Visibility.Visible;
            //storyboard1!.Completed -= Storyboard1_Completed;
            storyboard1 = null;
            draggingTGBNewPosCD.MinWidth = draggingButton.ActualWidth;
            draggingTGBNewPosCD.Width = GridLength.Auto;
            _myInnerGrid!.Children.Remove( animator );
            draggingTGBOrgCD.MaxWidth = DefaultWidth;
            animator = null;
            isSwitching = false;
            //DispatcherTimer dt2 = new DispatcherTimer();
            //dt2.Tick += ( sender, e ) => {
            //    dt2.Stop();
            //    System.Diagnostics.Debug.WriteLine( "" );
            //    System.Diagnostics.Debug.WriteLine( $"SwitchTabsPositions[Wdt]: draggingTGBOrgCD:{draggingTGBOrgCD.ActualWidth}; item:{draggingTGBNewPosCD.ActualWidth}" );
            //    System.Diagnostics.Debug.WriteLine( "" );
            //    System.Diagnostics.Debug.WriteLine( $"SwitchTabsPositions[3-1]: dragging:{draggingButton?.TabPosition}; item:{realTarget.TabPosition}" );
            //    System.Diagnostics.Debug.WriteLine( $"SwitchTabsPositions[3-2]: dragging:{draggingButton?.GetValue( Grid.ColumnProperty )}; item:{realTarget.GetValue( Grid.ColumnProperty )}" );
            //    System.Diagnostics.Debug.WriteLine( "Finished" );
            //    System.Diagnostics.Debug.WriteLine( "" );
            //};
            //dt2.Interval = TimeSpan.FromMilliseconds( 0 );
            //dt2.Start();
        };

        //DispatcherTimer dt = new DispatcherTimer();
        //dt.Tick += ( sender, e ) => {
        //    System.Diagnostics.Debug.WriteLine( "" );
        //    System.Diagnostics.Debug.WriteLine( $"SwitchTabsPositions[Wdt]: draggingTGBOrgCD:{draggingTGBOrgCD.ActualWidth}; item:{draggingTGBNewPosCD.ActualWidth}" );
        //    System.Diagnostics.Debug.WriteLine( "" );
        //    System.Diagnostics.Debug.WriteLine( $"SwitchTabsPositions[2-1]: dragging:{draggingButton.TabPosition}; item:{item.TabPosition}" );
        //    System.Diagnostics.Debug.WriteLine( $"SwitchTabsPositions[2-2]: dragging:{draggingButton?.GetValue( Grid.ColumnProperty )}; item:{realTarget.GetValue( Grid.ColumnProperty )}" );
        //    dt.Stop();
        draggingButton!.SetValue( Grid.ColumnProperty, draggingButton.TabPosition = targetPos );
        positions = positions.Values.ToDictionary( a => a.TabPosition, a => a );
        storyboard1.Begin();
        //};
        //dt.Interval = TimeSpan.FromMilliseconds( 0 );
        //dt.Start();
        #endregion
    }
    #endregion

    #region Animasyonsuz Drag&Drop Codları. Devre Dışıdır...
#if false
    TabGroupButton switchingMaster, switchingTarget;
    ColumnDefinition column, toRemove;
    private void SwitchTabsPositionsOld( TabGroupButton draggingButton, TabGroupButton item ) {
        if (isSwitching /*|| (switchingMaster == draggingButton && switchingTarget == item)*/) return;
        isSwitching = true;

        #region Preparations for animation
        switchingMaster = draggingButton;
        switchingTarget = item;

        toRemove = _myInnerGrid!.ColumnDefinitions[draggingButton.TabPosition];

        // new column
        var plus = 0;
        if (switchingMaster.TabPosition < switchingTarget.TabPosition) plus = 1;
        var cd = column = CreateAGridColumn(_myInnerGrid!, item.TabPosition + plus, 0);
        ShiftControlsToRight( item.TabPosition + plus );
        AddSeparator();
        #endregion

        // Animation need to be done here...
        // NOTE: GridLengthAnimation düzgün çalışır hale gelmeden açma...
        //MyCounter = 0;
        //cd.BeginAnimation( ColumnDefinition.WidthProperty, DoubleAnimationToWidth( switchingTarget.ActualWidth ) );
        //toRemove.BeginAnimation( ColumnDefinition.WidthProperty, DoubleAnimationToWidth() );

        // NOTE: Animasyonu becerene kadar bununla idare et...
        cd.Width = toRemove.Width;
        toRemove.Width = new GridLength( 0 );
        DispatcherTimer dt = new DispatcherTimer();
        dt.Tick += ( sender, e ) => {
            dt.Stop();
            Da_Completed();
        };
        dt.Interval = TimeSpan.FromMilliseconds( 100 );
        dt.Start();
    }

    private GridLengthAnimation DoubleAnimationToWidth( double width = 0 ) {
        var da = new GridLengthAnimation() {
            To = new GridLength( 0 ),
            Duration = TimeSpan.FromSeconds( 0.1 ),
        };
        da.Completed += Da_Completed;
        return da;
    }

    int MyCounter = 0;
    private void Da_Completed( object? sender, EventArgs e ) {
        MyCounter++;
        if (MyCounter == 2) Da_Completed();
    }

    private void Da_Completed() {
        #region Repositioning TabGroupsButtons
        RemoveLastSeparator();
        column.Width = new GridLength( switchingMaster.ActualWidth );
        toRemove.Width = new GridLength( 0 );
        _myInnerGrid!.ColumnDefinitions.Remove( toRemove );
        var pos = switchingTarget.TabPosition;
        var a = 0;
        if (switchingMaster.TabPosition > switchingTarget.TabPosition) {
            a = 1;
            positions.Values
                .Where( a => switchingTarget.TabPosition <= a.TabPosition && a.TabPosition < switchingMaster.TabPosition )
                .ToList()
                .ForEach( tgb => tgb.SetValue( Grid.ColumnProperty, ++tgb.TabPosition ) );
        } else if (switchingMaster.TabPosition < switchingTarget.TabPosition) {
            a = 2;
            positions.Values
                .Where( a => switchingMaster.TabPosition < a.TabPosition && a.TabPosition <= switchingTarget.TabPosition )
                .ToList()
                .ForEach( tgb => tgb.SetValue( Grid.ColumnProperty, --tgb.TabPosition ) );
        }
        switchingMaster.SetValue( Grid.ColumnProperty, switchingMaster.TabPosition = pos );
        SetControlPositions();

        positions = positions.Values.ToDictionary( a => a.TabPosition, a => a );
        #endregion
        isSwitching = false;
    }
#endif
    #endregion

    #region TabGroupButton_EventHandlers
    private void Tgb_CloseRequested( object sender, Delegates.CloseRequestedEventArgs e ) {
        if (e.Source.TabGroupItem.TotalCount > 1) {
            var rv = MessageBox.Show( $"Are you sure to close the tab group ({e.Source.Title}).", "Personal Web Browser", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (rv == MessageBoxResult.No)
                return;
        }
        if (TabGroups!.Count == 1) {
            Application.Current.Shutdown();
        }
        TabGroup? newTabGroupToSelect = null;
        if (e.Source.IsSelected) {
            var tabPos = e.Source.TabPosition;
            var next = relation.Values.Where( (t) => t.TabPosition == tabPos + 1 ).FirstOrDefault();
            if (next is null) {
                next = relation.Values.Where( ( t ) => t.TabPosition == tabPos - 1 ).FirstOrDefault();
            }
            if (next is not null) {
                newTabGroupToSelect = next.TabGroupItem;
            }
        }
        Tabber.RemoveTabGroup( e.Source.TabGroupItem, newTabGroupToSelect );
    }

    private void Tgb_Click( object sender, RoutedEventArgs e ) {
        isWaitingForDragging = false;
        if (e.Source is TabGroupButton tgb)
            Tabber.SetSelectedTabGroup( tgb.TabGroupItem );
    }
    #endregion

    private void NewTabGroupButton_Click( object sender, RoutedEventArgs e ) {
        Tabber.AddNewTabGroup();
        MainContent.CreateNewTabItem();
    }

    #region InsertAColumn and ShiftControls methods
    /// <summary>
    /// Inserts a new column to the grid and shifts new group button to right
    /// </summary>
    /// <returns></returns>
    private ColumnDefinition InsertAColumn( int pos = -1 ) {
        ColumnDefinition cd = CreateAGridColumn(_myInnerGrid!, pos);
        AddSeparator();
        SetInnerGridSize();
        return cd;
    }

    private ColumnDefinition CreateAGridColumn( Grid parent, int pos = -1, double width = -1 ) => new MyColumnDef( parent, pos, width );

    private void AddSeparator() => borders.Add( new MySeparator( _myInnerGrid! ) );

    private void RemoveLastSeparator() {
        _myInnerGrid!.Children.Remove( borders[^1] );
        borders.Remove( borders[^1] );
    }

    /// <summary>
    /// Shifts related TabGroupButtons to one column left
    /// </summary>
    /// <param name="tabPosition"></param>
    /// <param name="maxPos"></param>
    private void ShiftControlsToLeft( int tabPosition, int maxPos = 1000 ) {
        var lst = positions.Values.Where(a=> a.TabPosition >= tabPosition && a.TabPosition < maxPos).OrderBy(a => a.TabPosition).ToList();
        lst.ForEach( tgb => {
            positions.Remove( tgb.TabPosition );
            tgb.TabPosition--;
            tgb.SetValue( Grid.ColumnProperty, tgb.TabPosition );
        } );
        lst.ForEach( tgb => positions.Add( tgb.TabPosition, tgb ) );
        tmpCheckOrder();
    }

    private void ShiftControlsToRight( int tabPosition ) {
        var lst = positions.Values.Where(a=> a.TabPosition >= tabPosition).OrderBy(a => a.TabPosition).ToList();
        lst.ForEach( tgb => {
            tgb.SetValue( Grid.ColumnProperty, tgb.TabPosition + 1 );
        } );
    }
    private void SetControlPositions() {
        foreach (var tgb in positions.Values) {
            tgb.SetValue( Grid.ColumnProperty, tgb.TabPosition );
        }
    }
    #endregion


    #region OnApplyTemplate method override
    public override void OnApplyTemplate() {
        _myInnerGrid = Template.FindName( "PART_InnerGrid", this ) as Grid;
        if (_myInnerGrid == null)
            throw new Exception( "InnerGrid does not exist in the applied template" );

        _myOuterGrid = Template.FindName( "PART_OuterGrid", this ) as Grid;
        if (_myOuterGrid == null)
            throw new Exception( "InnerGrid does not exist in the applied template" );

        _myTabDragger = Template.FindName( "PART_TabDragger", this ) as StackPanel;
        if (_myTabDragger == null)
            throw new Exception( "StackPanel (TabDragger does not exist in the applied template" );

        _myButton = Template.FindName( "PART_NewGroup", this ) as AddressBarButton;
        if (_myButton == null)
            throw new Exception( "NewGroup button does not exist in the applied template" );
        _myButton.Click += NewTabGroupButton_Click;

        base.OnApplyTemplate();
    }
    #endregion

    #region private classes
    class MySeparator : Border {
        public MySeparator() {
            HorizontalAlignment = HorizontalAlignment.Right;
            VerticalAlignment = VerticalAlignment.Center;
            Margin = new Thickness( 0, 10, 0, 0 );
            Height = 22;
            Width = 1;
            BorderThickness = new Thickness( 1 );
            BorderBrush = Brushes.Wheat;
            Tag = "Separator";
        }
        public MySeparator( Grid parentGrid ) : this() {
            SetValue( Grid.ColumnProperty, parentGrid.ColumnDefinitions.Count - 1 );
            parentGrid.Children.Add( this );
        }
    }

    class MyColumnDef : ColumnDefinition {
        public MyColumnDef() {
            MaxWidth = DefaultWidth;
            Width = new GridLength( 1, GridUnitType.Star );
        }

        public MyColumnDef( Grid parent, int pos = -1, double width = -1 ) : this() {
            if (width > -1)
                Width = new GridLength( width );
            if (pos == -1)
                parent.ColumnDefinitions.Add( this );
            else {
                parent.ColumnDefinitions.Insert( pos, this );
            }
        }

        public override string ToString() {
            return $"MyColumnDef[Width:{Width}, ActualWidth:{ActualWidth}]";
        }
    }
    #endregion
}
