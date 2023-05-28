using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

using PWB_CCLibrary.Classes;
using PWB_CCLibrary.Delegates;

namespace PWB_CCLibrary.Controls;

public class TabGroupButton : Button {

    #region static ctor..
    static TabGroupButton() {
        DefaultStyleKeyProperty.OverrideMetadata( typeof( TabGroupButton ), new FrameworkPropertyMetadata( typeof( TabGroupButton ) ) );
    }
    #endregion

    internal event CloseRequestedEventHandler CloseRequested;

    private TabGroup tabGroupItem;

    public TabGroup TabGroupItem {
        get => tabGroupItem;
        set {
            tabGroupItem = value;
            this.Title = tabGroupItem.Name;
            tabGroupItem.PropertyChanged += TabGroupItem_PropertyChanged;
        }
    }

    private void TabGroupItem_PropertyChanged( object? sender, System.ComponentModel.PropertyChangedEventArgs e ) {
        if (e.PropertyName == nameof( tabGroupItem.Name )) {
            this.Title = tabGroupItem.Name;
        }
    }


    #region Dependency Properties
    public bool IsSelected {
        get { return (bool)GetValue( IsSelectedProperty ); }
        set {
            SetValue( IsSelectedProperty, value );
            ActivateFader();
        }
    }

    // Using a DependencyProperty as the backing store for IsSelected.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty IsSelectedProperty =
                           DependencyProperty.Register(nameof( IsSelected ), typeof(bool), typeof(TabGroupButton), new PropertyMetadata(false));

    public int TabPosition {
        get { return (int)GetValue( TabPositionProperty ); }
        set {
            SetValue( TabPositionProperty, value );
            IsFirstTab = value == 0;
        }
    }



    public bool IsFirstTab {
        get { return (bool)GetValue( IsFirstTabProperty ); }
        set { SetValue( IsFirstTabProperty, value ); }
    }

    // Using a DependencyProperty as the backing store for IsFirstTab.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty IsFirstTabProperty =
    DependencyProperty.Register(nameof( IsFirstTab ), typeof(bool), typeof(TabGroupButton), new PropertyMetadata(false));



    // Using a DependencyProperty as the backing store for TabPosition.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty TabPositionProperty =
                           DependencyProperty.Register(nameof( TabPosition ), typeof(int), typeof(TabGroupButton), new PropertyMetadata(0));

    public string CloseButtonKey {
        get { return (string)GetValue( CloseButtonKeyProperty ); }
        set { SetValue( CloseButtonKeyProperty, value ); }
    }

    // Using a DependencyProperty as the backing store for CloseButtonKey.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty CloseButtonKeyProperty =
                           DependencyProperty.Register(nameof( CloseButtonKey ), typeof(string), typeof(TabGroupButton), new PropertyMetadata("geoCloseButton03"));

    public string Title {
        get { return (string)GetValue( TitleProperty ); }
        set {
            SetValue( TitleProperty, value );
            ActivateFader();
        }
    }

    // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty TitleProperty =
                           DependencyProperty.Register(nameof( Title ), typeof(string), typeof(TabGroupButton), new PropertyMetadata("PWB Tab Group"));

    public Color TabColor {
        get { return (Color)GetValue( TabColorProperty ); }
        set {
            SetValue( TabColorProperty, value );
            PrepareTabColor();
        }
    }

    private void PrepareTabColor() {
        if (_myInsiderBorder is not null) {
            if (TabColor == Colors.Transparent) {
                _myInsiderBorder.Visibility = Visibility.Collapsed;
            } else {
                var aa = new LinearGradientBrush( TabColor, Colors.Transparent, 0 );
                aa.StartPoint = new Point( 0, 0 );
                aa.EndPoint = new Point( 0, 1 );
                var semicolor = Color.FromArgb(128, TabColor.R, TabColor.G, TabColor.B );
                aa.GradientStops.Add( new GradientStop( TabColor, 0 ) );
                aa.GradientStops.Add( new GradientStop( semicolor, 0.2 ) );
                aa.GradientStops.Add( new GradientStop( Colors.Transparent, 1 ) );
                _myInsiderBorder.Background = aa;
                _myInsiderBorder.Visibility = Visibility.Visible;
            }
        }
    }

    // Using a DependencyProperty as the backing store for BGColorNormal.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty TabColorProperty =
                           DependencyProperty.Register(nameof(TabColor), typeof(Color), typeof(TabGroupButton), new PropertyMetadata(Colors.Transparent));
    #endregion

    #region Controls from Template
    Path? _myPath;
    Image? _myImage;
    TextBlock? _myTextBlock;
    Button? _myCloseButton;
    ColumnDefinition? _myColumn;
    Border? _myInsiderBorder/*, _mySeparator*/;
    #endregion

    #region Class ctor..
    public TabGroupButton() {
        Loaded += TabGroupButton_Loaded;
        MouseEnter += TabGroupButton_MouseEnter;
        MouseLeave += TabGroupButton_MouseLeave;
        MouseRightButtonUp += TabGroupButton_MouseRightButtonUp;
        SizeChanged += TabGroupButton_SizeChanged;
        MouseUp += TabGroupButton_MouseUp;
    }

    private void TabGroupButton_MouseUp( object sender, System.Windows.Input.MouseButtonEventArgs e ) {
        if (e.ChangedButton == System.Windows.Input.MouseButton.Middle && e.ButtonState == System.Windows.Input.MouseButtonState.Released) {
            CloseRequested?.Invoke( this, new CloseRequestedEventArgs( this ) );
            e.Handled = true;
        }
    }

    int colorPos = -1;

    private void TabGroupButton_MouseRightButtonUp( object sender, System.Windows.Input.MouseButtonEventArgs e ) {
        if ((++colorPos) >= TabColors.Length) {
            colorPos = -1;
            TabColor = Colors.Transparent;
        } else {
            TabColor = TabColors[colorPos];
        }
    }
    #endregion

    public Boolean IsMouseOverTheCloseButton => _myCloseButton!.IsMouseOver;

    public Boolean IsMouseOverYou( Point pos ) => pos.X >= 10 && pos.X <= this.ActualWidth - 10 && pos.Y >= 0 && pos.Y <= this.ActualHeight;
    //public Boolean IsMouseYourLeftSide( Point pos ) => pos.X >= 0 && pos.X <= this.ActualWidth / 2 && pos.Y >= 0 && pos.Y <= this.ActualHeight / 2;
    //public Boolean IsMouseYourRightSide( Point pos ) => pos.X > this.ActualWidth / 2 && pos.X <= this.ActualWidth && pos.Y > this.ActualHeight / 2 && pos.Y <= this.ActualHeight;

    //public void HideSeparator() => _mySeparator!.Visibility = Visibility.Collapsed;
    //public void ShowSeparator() => _mySeparator!.Visibility = Visibility.Visible;

    #region ActivateFader caller event handlers
    private void TabGroupButton_Loaded( object sender, RoutedEventArgs e ) {
        ActivateFader();
        IsFirstTab = TabPosition == 0;
    }

    private void TabGroupButton_SizeChanged( object sender, SizeChangedEventArgs e ) {
        ActivateFader();
    }

    private void TabGroupButton_MouseLeave( object sender, System.Windows.Input.MouseEventArgs e ) {
        ActivateFader();
    }

    private void TabGroupButton_MouseEnter( object sender, System.Windows.Input.MouseEventArgs e ) {
        ActivateFader();
    }

    private void _myTextBlock_SizeChanged( object sender, SizeChangedEventArgs e ) {
        ActivateFader();
    }
    #endregion

    private void _myCloseButton_Click( object sender, RoutedEventArgs e ) {
        CloseRequested?.Invoke( this, new CloseRequestedEventArgs( this ) );
        e.Handled = true;
    }

    public static Color[] TabColors = new Color[] {
            Colors.DarkBlue,
            Colors.Red,
            Colors.Magenta,
            Colors.Blue,
            Colors.LimeGreen,
            Colors.Yellow
        };

    #region ActivateFader() method
    private void ActivateFader() {
        if (_myTextBlock == null) return;
        try {
            Typeface typeface = new Typeface( _myTextBlock!.FontFamily, _myTextBlock!.FontStyle, _myTextBlock!.FontWeight, _myTextBlock!.FontStretch );
            FormattedText formattedText = new FormattedText( _myTextBlock!.Text,
                                                             System.Threading.Thread.CurrentThread.CurrentCulture,
                                                             _myTextBlock!.FlowDirection,
                                                             typeface,
                                                             _myTextBlock!.FontSize,
                                                             _myTextBlock!.Foreground,
                                                             VisualTreeHelper.GetDpi(_myTextBlock).PixelsPerDip) {
                Trimming = TextTrimming.None,
                MaxLineCount = 1
            };

            var maxWidth = _myColumn!.ActualWidth - _myTextBlock.Margin.Left - _myTextBlock!.Margin.Right;

            if ((maxWidth < formattedText.Width)) {
                var aa = new LinearGradientBrush( Colors.Black, Colors.Transparent, 0 );
                aa.MappingMode = BrushMappingMode.Absolute;
                aa.StartPoint = new Point( maxWidth - 30, 0 );
                aa.EndPoint = new Point( maxWidth, 0 );
                aa.GradientStops.Add( new GradientStop( Colors.Black, 0 ) );
                aa.GradientStops.Add( new GradientStop( Colors.Transparent, 1 ) );
                _myTextBlock.OpacityMask = aa;
            } else
                _myTextBlock.OpacityMask = null;
        } catch {
        }
    }
    #endregion

    public override string ToString() {
        return $"TabGroupButton[TabPosition:{TabPosition}, Title:{Title}]";
    }

    #region OnApplyTemplate() method
    public override void OnApplyTemplate() {
        _myPath = Template.FindName( "PART_Path", this ) as Path;
        _myImage = Template.FindName( "PART_Image", this ) as System.Windows.Controls.Image;
        if (_myPath is null || _myImage is null) {
            throw new Exception( "Applied template does not contain either of PART_Image and/or PART_Path" );
        }

        _myColumn = Template.FindName( "PART_Column", this ) as ColumnDefinition;
        _myTextBlock = Template.FindName( "PART_TextBlock", this ) as TextBlock;
        _myCloseButton = Template.FindName( "PART_CloseButton", this ) as Button;
        if (_myTextBlock is null || _myCloseButton is null) {
            throw new Exception( "Applied template does not contain either of PART_TextBlock and/or PART_CloseButton" );
        }

        _myInsiderBorder = Template.FindName( "PART_InsiderBorder", this ) as Border;
        PrepareTabColor();
        //_mySeparator = Template.FindName( "PART_Separator", this ) as Border;


        _myTextBlock.SizeChanged += _myTextBlock_SizeChanged;
        _myCloseButton.Click += _myCloseButton_Click;

        base.OnApplyTemplate();
    }
    #endregion
}
